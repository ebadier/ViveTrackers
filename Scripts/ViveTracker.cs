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

namespace ViveTrackers
{
	public struct ViveTrackerID
	{
		public readonly uint trackedDevice_Index;
		public readonly string trackedDevice_SerialNumber;

		public ViveTrackerID(uint pTrackedDevice_Index, string pTrackedDevice_SerialNumber)
		{
			trackedDevice_Index = pTrackedDevice_Index;
			trackedDevice_SerialNumber = pTrackedDevice_SerialNumber;
		}
	}

	/// <summary>
	/// Represents a HTC ViveTracker object.
	/// </summary>
	public sealed class ViveTracker : MonoBehaviour
	{
		public DebugTransform debugTransform;
		public ViveTrackerID ID { get; private set; }
		public bool IsTracked { get; private set; }
		public Quaternion Calibration
		{
			get { return _trackerRotationOffset; }
			set { _trackerRotationOffset = value; }
		}

		public Action<bool> TrackedStateChanged;
		public Action<ViveTracker> Calibrated;

		private bool _calibrate = false;
		private Transform _transform = null;
		private Quaternion _trackerRotationOffset = Quaternion.identity;
		private ViveTrackingStateWatcher _trackingStateWatcher = new ViveTrackingStateWatcher();

		public void Init(ViveTrackerID pID, string pName)
		{
			_transform = transform;
			IsTracked = false;
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
		/// Update ViveTracker.
		/// </summary>
		public void UpdateState(bool pIsConnected, bool pIsPoseValid, bool pIsOpticallyTracked, Vector3 pLocalPosition, Quaternion pLocalRotation, float pDeltaTime)
		{
			bool isTracked = _trackingStateWatcher.Update(pIsConnected, pIsPoseValid, pIsOpticallyTracked, pDeltaTime);
			//Debug.Log(string.Format("{0} | Connected : {1} | PoseValid : {2} | OpticalTracking : {3} | IsTracked : {4}", 
			//	name, pIsConnected, pIsPoseValid, pIsOpticallyTracked, isTracked));

			// Warn if tracked state changed.
			if ((IsTracked != isTracked) && (TrackedStateChanged != null))
			{
				TrackedStateChanged(isTracked);
			}
			IsTracked = isTracked;

			// Update only if the tracker is tracked.
			// This way, the tracker keeps its last transform when the tracking is lost (better than a reset of the transform).
			if (isTracked)
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
}