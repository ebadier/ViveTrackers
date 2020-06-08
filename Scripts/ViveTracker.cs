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

		private CVRSystem _cvrSystem;
		private bool _calibrate = false;
		private Transform _transform;
		private Quaternion _trackerRotationOffset = Quaternion.identity;

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
		/// <param name="pTrans"></param>
		public void UpdateTransform(SteamVR_Utils.RigidTransform pTrans)
		{
			if (_calibrate)
			{
				_trackerRotationOffset = Quaternion.Inverse(pTrans.rot);
				_calibrate = false;

				if (Calibrated != null)
				{
					Calibrated(this);
				}
			}
			_transform.localPosition = pTrans.pos;
			_transform.localRotation = pTrans.rot * _trackerRotationOffset;
		}
	}
}