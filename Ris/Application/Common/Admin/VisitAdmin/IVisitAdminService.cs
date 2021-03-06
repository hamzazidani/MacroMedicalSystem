#region License

// Copyright (c) 2013, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
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
using System.ServiceModel;
using ClearCanvas.Enterprise.Common;

namespace ClearCanvas.Ris.Application.Common.Admin.VisitAdmin
{
    [RisApplicationService]
    [ServiceContract]
    public interface IVisitAdminService
    {
        [OperationContract]
        LoadVisitEditorFormDataResponse LoadVisitEditorFormData(LoadVisitEditorFormDataRequest request);

        /// <summary>
        /// Lists visits for a specified patient.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        ListVisitsForPatientResponse ListVisitsForPatient(ListVisitsForPatientRequest request);

        [OperationContract]
        LoadVisitForEditResponse LoadVisitForEdit(LoadVisitForEditRequest request);

        /// <summary>
        /// Updates an existing visit.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(RequestValidationException))]
        [FaultContract(typeof(ConcurrentModificationException))]
        UpdateVisitResponse UpdateVisit(UpdateVisitRequest request);

        /// <summary>
        /// Adds a new visit to the system.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(RequestValidationException))]
        AddVisitResponse AddVisit(AddVisitRequest request);
    }
}
