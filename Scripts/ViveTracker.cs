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
		public const float ImuOnlyMinDuration = 0.033f; // = 2 frames @60Hz
		public const float ImuOnlyMaxDuration = 2f;

		[Range(ImuOnlyMinDuration, ImuOnlyMaxDuration)]
		[Tooltip("The duration a tracker's position can be considered as reliably tracked with IMU tracking only (no optical tracking). Use min value for data analysis purpose, and up to max value for gameplay purpose.")]
		public float imuOnlyReliablePositionDuration = ImuOnlyMinDuration;
		public DebugTransform debugTransform;

		#region Properties
		public ViveTrackerID ID { get; private set; }
		
		public bool Connected 
		{ 
			get { return _connected; } 
			private set
			{
				if(_connected != value)
				{
					_connected = value;
					if(ConnectedStatusChanged != null)
					{
						ConnectedStatusChanged(this);
					}
				}
			}
		}

		public bool PositionValid 
		{ 
			get { return _positionValid; }
			private set
			{
				if(_positionValid != value)
				{
					_positionValid = value;
					if(PositionValidChanged != null)
					{
						PositionValidChanged(this);
					}
				}
			}
		}

		public bool RotationValid 
		{
			get { return _rotationValid; }
			private set
			{
				if(_rotationValid != value)
				{
					_rotationValid = value;
					if(RotationValidChanged != null)
					{
						RotationValidChanged(this);
					}
				}
			}
		}

		public Quaternion Calibration
		{
			get { return _trackerRotationOffset; }
			set { _trackerRotationOffset = value; }
		}

		public bool GripState
		{
			get { return _gripState; }
			private set
			{
				bool previousGripState = _gripState;
				_gripState = value;

				if ((!previousGripState) && _gripState && (GripPressed != null))
				{
					GripPressed(this);
				}
				else if (previousGripState && (!_gripState) && (GripReleased != null))
				{
					GripReleased(this);
				}
			}
		}

		public bool TriggerState
		{
			get { return _triggerState; }
			private set
			{
				bool previousTriggerState = _triggerState;
				_triggerState = value;

				if ((!previousTriggerState) && _triggerState && (TriggerPressed != null))
				{
					TriggerPressed(this);
				}
				else if (previousTriggerState && (!_triggerState) && (TriggerReleased != null))
				{
					TriggerReleased(this);
				}
			}
		}

		public bool TouchPadState
		{
			get { return _touchPadState; }
			private set
			{
				bool previousTouchPadState = _touchPadState;
				_touchPadState = value;

				if ((!previousTouchPadState) && _touchPadState && (TouchPadPressed != null))
				{
					TouchPadPressed(this);
				}
				else if (previousTouchPadState && (!_touchPadState) && (TouchPadReleased != null))
				{
					TouchPadReleased(this);
				}
			}
		}

		public bool MenuState
		{
			get { return _menuState; }
			private set
			{
				bool previousMenuState = _menuState;
				_menuState = value;

				if ((!previousMenuState) && _menuState && (MenuPressed != null))
				{
					MenuPressed(this);
				}
				else if (previousMenuState && (!_menuState) && (MenuReleased != null))
				{
					MenuReleased(this);
				}
			}
		}
		#endregion

		#region Actions
		public Action<ViveTracker> ConnectedStatusChanged;
		public Action<ViveTracker> PositionValidChanged;
		public Action<ViveTracker> RotationValidChanged;
		public Action<ViveTracker> Calibrated;
		// Buttons
		public Action<ViveTracker> GripPressed;
		public Action<ViveTracker> GripReleased;
		public Action<ViveTracker> TriggerPressed;
		public Action<ViveTracker> TriggerReleased;
		public Action<ViveTracker> TouchPadPressed;
		public Action<ViveTracker> TouchPadReleased;
		public Action<ViveTracker> MenuPressed;
		public Action<ViveTracker> MenuReleased;
		#endregion

		#region Private Attributes
		private bool _connected = false;
		private bool _positionValid = false;
		private bool _rotationValid = false;
		private bool _calibrate = false;
		private Transform _transform = null;
		private Quaternion _trackerRotationOffset = Quaternion.identity;
		private ViveTrackingStateWatcher _trackingStateWatcher = new ViveTrackingStateWatcher();
		// Buttons
		private bool _gripState = false;
		private bool _triggerState = false;
		private bool _touchPadState = false;
		private bool _menuState = false;
		private uint _packetNum = 0u;
		#endregion

		#region Public Methods
		public void Init(ViveTrackerID pID, string pName)
		{
			_transform = transform;
			Connected = PositionValid = RotationValid = false;
			GripState = TriggerState = TouchPadState = MenuState = false;
			_packetNum = 0u;
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
		/// Update ViveTracker Pose.
		/// </summary>
		public void UpdatePose(bool pIsConnected, bool pIsPoseValid, bool pIsOpticallyTracked, Vector3 pLocalPosition, Quaternion pLocalRotation, float pDeltaTime)
		{
			bool isPosValid, isRotValid;
			_trackingStateWatcher.Update(out isPosValid, out isRotValid, pIsConnected, pIsPoseValid, pIsOpticallyTracked, pDeltaTime, imuOnlyReliablePositionDuration);
			//Debug.Log(string.Format("{0} | Connected : {1} | PoseValid : {2} | OpticalTracking : {3} | PosOK : {4} | RotOK : {5}",
			//	name, pIsConnected, pIsPoseValid, pIsOpticallyTracked, isPosValid, isRotValid));

			// Warn for states changed.
			Connected = pIsConnected;
			PositionValid = isPosValid;
			RotationValid = isRotValid;

			// Update only if the tracker is reliably tracked.
			// This way, the tracker keeps its last transform when the tracking is lost (better than using unreliable values).
			// POSITION
			if (isPosValid)
			{
				_transform.localPosition = pLocalPosition;
			}
			// ROTATION
			if(isRotValid)
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
				_transform.localRotation = pLocalRotation * _trackerRotationOffset;
			}
		}

		/// <summary>
		/// Update ViveTracker Buttons (Pogo Pins).
		/// 2 – Ground
		/// 3 – Grip
		/// 4 – Trigger
		/// 5 – Touchpad
		/// 6 – Menu Button
		/// https://dl.vive.com/Tracker/Guideline/HTC%20Vive%20Tracker%20(3.0)%20Developer%20Guidelines_v1.0_01182021.pdf
		/// </summary>
		public void UpdateButtons(uint pPacketNum, ulong pButtonsState)
		{
			//Debug.Log("[ViveTracker.UpdateButtons()] PacketNum : " + pPacketNum);
			//Debug.Log("[ViveTracker.UpdateButtons()] ButtonsState : " + pButtonsState);
			if (pPacketNum != _packetNum) // Check if new packet
			{
				_packetNum = pPacketNum;
				GripState = (pButtonsState & (1ul << (int)EVRButtonId.k_EButton_Grip)) != 0ul;
				TriggerState = (pButtonsState & (1ul << (int)EVRButtonId.k_EButton_SteamVR_Trigger)) != 0ul;
				TouchPadState = (pButtonsState & (1ul << (int)EVRButtonId.k_EButton_SteamVR_Touchpad)) != 0ul;
				MenuState = (pButtonsState & (1ul << (int)EVRButtonId.k_EButton_ApplicationMenu)) != 0ul;
				//Debug.Log(string.Format("[ViveTracker.UpdateButtons()] Grip {0} | Trigger {1} | TouchPad {2} | MenuState {3}", GripState, TriggerState, TouchPadState, MenuState));
			}
		}

		#endregion
	}
}