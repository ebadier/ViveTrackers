/******************************************************************************************************************************************************
* MIT License																																		  *
*																																					  *
* Copyright (c) 2020																																  *
* Emmanuel Badier <emmanuel.badier@gmail.com>																										  *
* 																																					  *
* Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),  *
* to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,  *
* and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:		  *
* 																																					  *
* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.					  *
* 																																					  *
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, *
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 																							  *
* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 		  *
* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.							  *
******************************************************************************************************************************************************/

using System;
using UnityEngine;
using Valve.VR;

namespace ViveTrackers
{
	public sealed class ViveTrackerID
	{
		public uint TrackedDevice_Index { get; private set; }
		public string TrackedDevice_SerialNumber { get; private set; }

		public ViveTrackerID(uint pTrackedDevice_Index, string pTrackedDevice_SerialNumber)
		{
			TrackedDevice_Index = pTrackedDevice_Index;
			TrackedDevice_SerialNumber = pTrackedDevice_SerialNumber;
		}
	}

	/// <summary>
	/// Represents a HTC ViveTracker object.
	/// </summary>
	public sealed class ViveTracker : MonoBehaviour
	{
		public DebugTransform debugTransform;
		public ViveTrackerID ID { get; private set; }

		public Action<ViveTracker> Calibrated;
		public bool IsConnected { get { return (_cvrSystem != null) && _cvrSystem.IsTrackedDeviceConnected(ID.TrackedDevice_Index); } }

		private CVRSystem _cvrSystem = null;
		private bool _calibrate = false;
		private Transform _transform = null;
		private Quaternion _trackerRotationOffset = Quaternion.identity;

		public Quaternion Calibration
		{
			get { return _trackerRotationOffset; }
			set { _trackerRotationOffset = value; }
		}

		public void Init(CVRSystem pCVRSystem, ViveTrackerID pID, string pName)
		{
			_cvrSystem = pCVRSystem;
			_transform = transform;
			ID = pID;
			name = pName;
		}

		/// <summary>
		/// Align transformation with origin's transformation and keep the offset to apply during the next frames. 
		/// </summary>
		public void Calibrate()
		{
			_calibrate = true;
		}

		/// <summary>
		/// Update transformation using ViveTracker device transformation.
		/// </summary>
		public void UpdateTransform(Vector3 pLocalPosition, Quaternion pLocalRotation)
		{
			if (_calibrate)
			{
				_trackerRotationOffset = Quaternion.Inverse(pLocalRotation);
				_calibrate = false;

				if (Calibrated != null)
				{
					Calibrated(this);
				}
			}
			_transform.localPosition = pLocalPosition;
			_transform.localRotation = pLocalRotation * _trackerRotationOffset;
		}
	}
}