#region License

// Copyright (c) 2013, ClearCanvas Inc.
// All rights reserved.
// http://www.ClearCanvas.ca
//
// This file is part of the ClearCanvas RIS/PACS open source project.
//
// The ClearCanvas RIS/PACS open source project is free software: you can
// redistribute it and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// The ClearCanvas RIS/PACS open source project is distributed in the hope that it
// will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General
// Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// the ClearCanvas RIS/PACS open source project.  If not, see
// <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using Macro.Common;
using Macro.Common.Utilities;
using Macro.Desktop;
using Macro.Dicom.Iod;
using Macro.ImageViewer.AdvancedImaging.Fusion.Utilities;
using Macro.ImageViewer.Common;
using Macro.ImageViewer.Graphics;
using Macro.ImageViewer.Imaging;
using Macro.ImageViewer.InteractiveGraphics;
using Macro.ImageViewer.Mathematics;
using Macro.ImageViewer.StudyManagement;
using Macro.ImageViewer.Volume.Mpr;
using VolumeData = Macro.ImageViewer.Volume.Mpr.Volume;

namespace Macro.ImageViewer.AdvancedImaging.Fusion
{
	//TODO (CR Sept 2010): to the extent that this can be a pure class, unrelated to progress graphics,
	//that would be better from a design standpoint. Generally, this class is trying to do too many things, 
	//and it would be easier to deal with thread safety if it were separated into 2 or 3 classes.

	//TODO (CR Sept 2010): use the word "volume" instead of OverlayData?  I keep thinking this is a per-frame object
	//and confuse it with OverlayFrameData.
	public partial class FusionOverlayData : IDisposable, ILargeObjectContainer, IProgressGraphicProgressProvider
	{
		private readonly object _syncVolumeDataLock = new object();
		private readonly IList<VoiWindow> _voiWindows;
		private IList<IFrameReference> _frames;
		private VolumeData _volume;

		public FusionOverlayData(IEnumerable<Frame> overlaySource)
		{
			var frames = new List<IFrameReference>();
			foreach (Frame frame in overlaySource)
				frames.Add(frame.CreateTransientReference());
			_frames = frames.AsReadOnly();

			if (frames.Count > 0)
				_voiWindows = new List<VoiWindow>(VoiWindow.GetWindows(frames[0].Sop.DataSource)).AsReadOnly();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// this will unload _volume
				this.UnloadVolume();

				if (_volumeLoaderTask != null)
				{
					_volumeLoaderTask.RequestCancel();
					_volumeLoaderTask = null;
				}

				if (_frames != null)
				{
					foreach (IFrameReference frame in _frames)
						frame.Dispose();
					_frames = null;
				}
			}
		}

		public string SourceSeriesInstanceUid
		{
			get { return Volume.SourceSeriesInstanceUid; }
		}

		public string FrameOfReferenceUid
		{
			get { return this.Volume.FrameOfReferenceUid; }
		}

		public string Modality
		{
			get { return Volume.Modality; }
		}

		public int MinVolumeValue
		{
			get { return Volume.MinimumVolumeValue; }
		}

		public int MaxVolumeValue
		{
			get { return Volume.MaximumVolumeValue; }
		}

		/// <summary>
		/// Gets a list of the source frames from which the overlay data was constructed.
		/// </summary>
		public IList<Frame> Frames
		{
			get { return CollectionUtils.Map<IFrameReference, Frame>(_frames, f => f.Frame).AsReadOnly(); }
		}

		public IList<VoiWindow> VoiWindows
		{
			get { return _voiWindows; }
		}

		protected VolumeData Volume
		{
			get
			{
				// update the last access time
				_largeObjectData.UpdateLastAccessTime();

				// if the data is already available without blocking, return it immediately
				VolumeData volume = _volume;
				if (volume != null)
					return volume;

				return LoadVolume(null);
			}
		}

		private VolumeData LoadVolume(IBackgroundTaskContext context)
		{
			// wait for synchronized access
			lock (_syncVolumeDataLock)
			{
				// if the data is now available, return it immediately
				// (i.e. we were blocked because we were already reading the data)
				if (_volume != null)
					return _volume;

				// load the volume data
				if (context == null)
					_volume = VolumeData.Create(_frames);
				else
					_volume = VolumeData.Create(_frames, (n, count) => context.ReportProgress(new BackgroundTaskProgress(n, count, SR.MessageFusionInProgress)));

				// update our stats
				_largeObjectData.BytesHeldCount = 2*_volume.SizeInVoxels;
				_largeObjectData.LargeObjectCount = 1;
				_largeObjectData.UpdateLastAccessTime();

				// regenerating the volume data is easy when the source frames are already in memory!
				_largeObjectData.RegenerationCost = RegenerationCost.Low;

				// register with memory manager
				MemoryManager.Add(this);

				return _volume;
			}
		}

		private void UnloadVolume()
		{
			// wait for synchronized access
			lock (_syncVolumeDataLock)
			{
				// dump our data
				if (_volume != null)
				{
					_volume.Dispose();
					_volume = null;
				}

				// update our stats
				_largeObjectData.BytesHeldCount = 0;
				_largeObjectData.LargeObjectCount = 0;

				// unregister with memory manager
				MemoryManager.Remove(this);
			}

			this.OnUnloaded();
		}

		public FusionOverlayFrameData CreateOverlaySlice(Frame baseFrame)
		{
			return new FusionOverlayFrameData(baseFrame.CreateTransientReference(), this.CreateTransientReference());
		}

		protected internal byte[] GetOverlay(Frame baseFrame, out OverlayFrameParams overlayFrameParams)
		{
			var volume = this.Volume;

			// compute the bounds of the target base image frame in patient coordinates
			var baseTopLeft = baseFrame.ImagePlaneHelper.ConvertToPatient(new PointF(0, 0));
			var baseTopRight = baseFrame.ImagePlaneHelper.ConvertToPatient(new PointF(baseFrame.Columns, 0));
			var baseBottomLeft = baseFrame.ImagePlaneHelper.ConvertToPatient(new PointF(0, baseFrame.Rows));
			var baseFrameCentre = (baseTopRight + baseBottomLeft)/2;

			// compute the rotated volume slicing basis axes
			var volumeXAxis = (volume.ConvertToVolume(baseTopRight) - volume.ConvertToVolume(baseTopLeft)).Normalize();
			var volumeYAxis = (volume.ConvertToVolume(baseBottomLeft) - volume.ConvertToVolume(baseTopLeft)).Normalize();
			var volumeZAxis = volumeXAxis.Cross(volumeYAxis);

			var @params = new VolumeSlicerParams(volumeXAxis, volumeYAxis, volumeZAxis);
			using (var slice = new VolumeSliceSopDataSource(volume, @params, volume.ConvertToVolume(baseFrameCentre)))
			{
				using (var sliceSop = new ImageSop(slice))
				{
					using (var overlayFrame = sliceSop.Frames[1])
					{
						// compute the bounds of the target overlay image frame in patient coordinates
						var overlayTopLeft = overlayFrame.ImagePlaneHelper.ConvertToPatient(new PointF(0, 0));
						var overlayTopRight = overlayFrame.ImagePlaneHelper.ConvertToPatient(new PointF(overlayFrame.Columns, 0));
						var overlayBottomLeft = overlayFrame.ImagePlaneHelper.ConvertToPatient(new PointF(0, overlayFrame.Rows));
						var overlayOffset = overlayTopLeft - baseTopLeft;

						// compute the overlay and base image resolution in pixels per unit patient space (mm).
						var overlayResolutionX = overlayFrame.Columns/(overlayTopRight - overlayTopLeft).Magnitude;
						var overlayResolutionY = overlayFrame.Rows/(overlayBottomLeft - overlayTopLeft).Magnitude;
						var baseResolutionX = baseFrame.Columns/(baseTopRight - baseTopLeft).Magnitude;
						var baseResolutionY = baseFrame.Rows/(baseBottomLeft - baseTopLeft).Magnitude;

						// compute parameters to register the overlay on the base image
						var scale = new PointF(baseResolutionX/overlayResolutionX, baseResolutionY/overlayResolutionY);
						var offset = new PointF(overlayOffset.X*overlayResolutionX, overlayOffset.Y*overlayResolutionY);

						//TODO (CR Sept 2010): could this be negative?
						// validate computed transform parameters
						Platform.CheckTrue(overlayOffset.Z < 0.5f, "Compute OffsetZ != 0");

						overlayFrameParams = new OverlayFrameParams(
							overlayFrame.Rows, overlayFrame.Columns,
							overlayFrame.BitsAllocated, overlayFrame.BitsStored,
							overlayFrame.HighBit, overlayFrame.PixelRepresentation != 0 ? true : false,
							overlayFrame.PhotometricInterpretation == PhotometricInterpretation.Monochrome1 ? true : false,
							overlayFrame.RescaleSlope, overlayFrame.RescaleIntercept,
							scale, offset);

						return overlayFrame.GetNormalizedPixelData();
					}
				}
			}
		}

		/// <summary>
		/// Finds the source frame closest to the specified position in patient coordinates.
		/// </summary>
		/// <param name="patientPosition">The reference position in patient coordinates.</param>
		/// <returns>The source frame closest to the specified position in patient coordinates, or NULL if no such frame exists.</returns>
		public Frame FindSourceFrame(Vector3D patientPosition)
		{
			PointF sourceImagePosition;
			return FindSourceFrame(patientPosition, out sourceImagePosition);
		}

		/// <summary>
		/// Finds the source frame closest to the specified position in patient coordinates.
		/// </summary>
		/// <param name="patientPosition">The reference position in patient coordinates.</param>
		/// <param name="sourceImagePosition">Returns the position on the closest source frame corresponding to the specified patient position.</param>
		/// <returns>The source frame closest to the specified position in patient coordinates, or NULL if no such frame exists.</returns>
		public Frame FindSourceFrame(Vector3D patientPosition, out PointF sourceImagePosition)
		{
			Frame closestFrame = null;
			sourceImagePosition = PointF.Empty;

			var distance = float.MaxValue;
			foreach (var frame in _frames)
			{
				var targetImagePlane = DicomImagePlane.FromFrame(frame.Frame);
				var halfThickness = Math.Abs(targetImagePlane.Thickness/2);
				var halfSpacing = Math.Abs(targetImagePlane.Spacing/2);
				var toleranceDistanceToImagePlane = Math.Max(halfThickness, halfSpacing);

				if (toleranceDistanceToImagePlane > 0)
				{
					var positionTargetImagePlane = targetImagePlane.ConvertToImagePlane(patientPosition);
					var distanceToTargetImagePlane = Math.Abs(positionTargetImagePlane.Z);

					if (distanceToTargetImagePlane <= toleranceDistanceToImagePlane && distanceToTargetImagePlane < distance)
					{
						distance = distanceToTargetImagePlane;
						//The coordinates need to be converted to pixel coordinates because right now they are in mm.
						sourceImagePosition = targetImagePlane.ConvertToImage(new PointF(positionTargetImagePlane.X, positionTargetImagePlane.Y));
						closestFrame = frame.Frame;
					}
				}
			}

			return closestFrame;
		}

		#region Memory Management Support

		private readonly LargeObjectContainerData _largeObjectData = new LargeObjectContainerData(Guid.NewGuid());

		Guid ILargeObjectContainer.Identifier
		{
			get { return _largeObjectData.Identifier; }
		}

		int ILargeObjectContainer.LargeObjectCount
		{
			get { return _largeObjectData.LargeObjectCount; }
		}

		long ILargeObjectContainer.BytesHeldCount
		{
			get { return _largeObjectData.BytesHeldCount; }
		}

		DateTime ILargeObjectContainer.LastAccessTime
		{
			get { return _largeObjectData.LastAccessTime; }
		}

		RegenerationCost ILargeObjectContainer.RegenerationCost
		{
			get { return _largeObjectData.RegenerationCost; }
		}

		bool ILargeObjectContainer.IsLocked
		{
			get { return _largeObjectData.IsLocked; }
		}

		void ILargeObjectContainer.Lock()
		{
			_largeObjectData.Lock();
		}

		void ILargeObjectContainer.Unlock()
		{
			_largeObjectData.Unlock();
		}

		void ILargeObjectContainer.Unload()
		{
			this.UnloadVolume();
		}

		#endregion

		#region Asynchronous Loading Support

		private readonly object _syncLoaderLock = new object();
		private event EventHandler _volumeUnloaded;
		private BackgroundTask _volumeLoaderTask;

		public event EventHandler Unloaded
		{
			add { _volumeUnloaded += value; }
			remove { _volumeUnloaded -= value; }
		}

		protected virtual void OnUnloaded()
		{
			EventsHelper.Fire(_volumeUnloaded, this, EventArgs.Empty);
		}

		public bool IsLoaded
		{
			get { return _volume != null; }
		}

		//TODO (CR Sept 2010): same comment as with the ProgressGraphic stuff; the API is unclear
		//as to what it is doing (return value) because it's trying to account for the async loading.

		/// <summary>
		/// Attempts to start loading the overlay data asynchronously, if not already loaded.
		/// </summary>
		/// <param name="progress">A value between 0 and 1 indicating the progress of the asynchronous loading operation.</param>
		/// <param name="message">A string message detailing the progress of the asynchronous loading operation.</param>
		/// <returns></returns>
		public bool BeginLoad(out float progress, out string message)
		{
			// update the last access time
			_largeObjectData.UpdateLastAccessTime();

			// if the data is already available without blocking, return success immediately

			//TODO (CR Sept 2010): because unloading the volume involves disposing it, if every operation that uses it
			//isn't in the same lock (e.g. lock(_syncVolumeDataLock)) you could be getting a disposed/null volume here.
			VolumeData volume = _volume;
			if (volume != null)
			{
				message = SR.MessageFusionComplete;
				progress = 1f;
				return true;
			}

			lock (_syncLoaderLock)
			{
				message = SR.MessageFusionInProgress;
				progress = 0;
				if (_volumeLoaderTask == null)
				{
					// if the data is available now, return success
					volume = _volume;
					if (volume != null)
					{
						message = SR.MessageFusionComplete;
						progress = 1f;
						return true;
					}

					_volumeLoaderTask = new BackgroundTask(c => this.LoadVolume(c), false, null) {ThreadUICulture = Application.CurrentUICulture};
					_volumeLoaderTask.Run();
					_volumeLoaderTask.Terminated += OnVolumeLoaderTaskTerminated;
				}
				else
				{
					if (_volumeLoaderTask.LastBackgroundTaskProgress != null)
					{
						message = _volumeLoaderTask.LastBackgroundTaskProgress.Progress.Message;
						progress = _volumeLoaderTask.LastBackgroundTaskProgress.Progress.Percent/100f;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Synchronously loads the overlay data.
		/// </summary>
		public void Load()
		{
			this.LoadVolume(null);
		}

		/// <summary>
		/// Synchronously unloads the overlay data.
		/// </summary>
		public void Unload()
		{
			this.UnloadVolume();
		}

		private void OnVolumeLoaderTaskTerminated(object sender, BackgroundTaskTerminatedEventArgs e)
		{
			BackgroundTask volumeLoaderTask = sender as BackgroundTask;
			if (volumeLoaderTask != null)
			{
				volumeLoaderTask.Terminated -= OnVolumeLoaderTaskTerminated;
				volumeLoaderTask.Dispose();
			}
			_volumeLoaderTask = null;
		}

		#endregion

		#region IProgressGraphicProgressProvider Members

		bool IProgressGraphicProgressProvider.IsRunning(out float progress, out string message)
		{
			return !BeginLoad(out progress, out message);
		}

		#endregion

		#region OverlayFrameParams Class

		/// <summary>
		/// Parameters of a fusion image's overlay pixel data, such as pixel format and coregistration values.
		/// </summary>
		protected internal class OverlayFrameParams
		{
			public readonly int Rows, Columns;
			public readonly int BitsAllocated, BitsStored, HighBit;
			public readonly bool IsSigned, IsInverted;
			public readonly double RescaleSlope, RescaleIntercept;
			public readonly PointF CoregistrationScale, CoregistrationOffset;

			internal OverlayFrameParams(int rows, int columns,
			                            int bitsAllocated, int bitsStored, int highBit,
			                            bool isSigned, bool isInverted,
			                            double rescaleSlope, double rescaleIntercept,
			                            PointF scale, PointF offset)
			{
				Rows = rows;
				Columns = columns;
				BitsAllocated = bitsAllocated;
				BitsStored = bitsStored;
				HighBit = highBit;
				IsSigned = isSigned;
				IsInverted = isInverted;
				RescaleSlope = rescaleSlope;
				RescaleIntercept = rescaleIntercept;
				CoregistrationScale = scale;
				CoregistrationOffset = offset;
			}
		}

		#endregion
	}
}