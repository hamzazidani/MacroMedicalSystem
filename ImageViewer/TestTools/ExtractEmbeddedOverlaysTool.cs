﻿#region License

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
using System.IO;
using Macro.Common;
using Macro.Desktop;
using Macro.Desktop.Tools;
using Macro.Dicom;
using Macro.Dicom.Iod.Modules;
using Macro.ImageViewer.Explorer.Local;
using Macro.Desktop.Actions;
using Path=Macro.Desktop.Path;

namespace Macro.ImageViewer.TestTools
{
	[MenuAction("extractEmbeddedOverlays", "explorerlocal-contextmenu/Extract Embedded Overlays", "Go")]
	[ExtensionOf(typeof(LocalImageExplorerToolExtensionPoint))]
	public class ExtractEmbeddedOverlaysTool : Tool<ILocalImageExplorerToolContext>
	{
		public void Go()
		{
			string[] files = BuildFileList();
			var args = new SelectFolderDialogCreationArgs
			{
				Path = GetDirectoryOfFirstPath(),
				AllowCreateNewFolder = true,
				Prompt = "Select output folder"
			};

			var result = base.Context.DesktopWindow.ShowSelectFolderDialogBox(args);
			if (result.Action != DialogBoxAction.Ok)
				return;

			try
			{
				foreach (string file in files)
				{
					DicomFile dicomFile = new DicomFile(file);
					dicomFile.Load();
					if (!new OverlayPlaneModuleIod(dicomFile.DataSet).ExtractEmbeddedOverlays())
						continue;

					string sourceFileName = System.IO.Path.GetFileNameWithoutExtension(file);
					string fileName = System.IO.Path.Combine(result.FileName, sourceFileName);
					fileName += ".overlays-extracted.dcm";
					dicomFile.Save(fileName);
				}
			}
			catch (Exception e)
			{
				ExceptionHandler.Report(e, Context.DesktopWindow);
			}
		}

		private string GetDirectoryOfFirstPath()
		{
			foreach (string path in Context.SelectedPaths)
				return System.IO.Path.GetDirectoryName(path);

			return null;
		}

		private string[] BuildFileList()
		{
			List<string> fileList = new List<string>();

			foreach (string path in this.Context.SelectedPaths)
			{
				if (File.Exists(path))
					fileList.Add(path);
				else if (Directory.Exists(path))
					fileList.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
			}

			return fileList.ToArray();
		}
	}
}
