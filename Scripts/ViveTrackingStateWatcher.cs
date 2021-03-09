/******************************************************************************************************************************************************
* MIT License																																		  *
*																																					  *
* Copyright (c) 2021																																  *
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

namespace ViveTrackers
{
	// Trackers' IMU alone can compute position & orientation with drifting issues.
	// Optical tracking is used to correct IMU tracking at 60Hz, but optical tracking can be lost when trackers are occluded.
	// This class gives you a reliable tracking state in every situation.
	public sealed class ViveTrackingStateWatcher
	{
		private bool _wasPositionPartiallyTracked; // previous position's tracking state.
		private bool _wasPositionReliablyTracked; // previous position's tracking state.
		private bool _wasRotationReliablyTracked; // previous rotation's tracking state.
		private float _elapsedTimeSinceLastFullTrack; // Time elapsed in a reliably tracked state since the last time the tracker was fully tracked.
		private float _elapsedTimeSinceLastInvalidPosition; // Time elapsed in the last reliably tracked state for position.
		private float _elapsedTimeSinceLastInvalidRotation; // Time elapsed in the last reliably tracked state for rotation.

		public ViveTrackingStateWatcher()
		{
			_wasPositionPartiallyTracked = _wasPositionReliablyTracked = _wasRotationReliablyTracked = false;
			_elapsedTimeSinceLastFullTrack = _elapsedTimeSinceLastInvalidPosition = _elapsedTimeSinceLastInvalidRotation = 0f;
		}

		/// <summary>
		/// Update the tracking state and returns whether the tracker can be considered as reliably tracked.
		/// </summary>
		public void Update(out bool pOutPositionValid, out bool pOutRotationValid, bool pIsConnected, bool pIsPoseValid, bool pIsOpticallyTracked, float pDeltaTime, float pIMUOnlyReliablePositionDuration)
		{
			bool isPartiallyTracked = pIsConnected && pIsPoseValid; // IMU only.
			bool isFullyTracked = isPartiallyTracked && pIsOpticallyTracked; // IMU + Optical.

			pOutPositionValid = _UpdatePositionState(isPartiallyTracked, isFullyTracked, pDeltaTime, pIMUOnlyReliablePositionDuration);
			pOutRotationValid = _UpdateRotationState(isPartiallyTracked, pDeltaTime);
		}

		private bool _UpdateRotationState(bool pIsPartiallyTracked, float pDeltaTime)
		{
			// IMU only is always enough to get a reliable rotation while pose is valid.
			bool isRotationReliablyTracked = pIsPartiallyTracked;

			// Save states for next iteration.
			bool wasRotationReliablyTracked = _wasRotationReliablyTracked;
			_wasRotationReliablyTracked = isRotationReliablyTracked;

			// Reject unreliable first frames when going from an untracked state to a reliable tracked state.
			if (isRotationReliablyTracked)
			{
				if (wasRotationReliablyTracked)
				{
					// count time elapsed since last invalid rotation.
					_elapsedTimeSinceLastInvalidRotation += pDeltaTime;
				}
				else
				{
					// reset counter if transitioning from an untracked state to a reliable tracked state.
					_elapsedTimeSinceLastInvalidRotation = pDeltaTime;
				}
				isRotationReliablyTracked = _elapsedTimeSinceLastInvalidRotation > ViveTracker.ImuOnlyMinDuration;
			}

			// Debug
			//if (isRotationReliablyTracked)
			//{
			//	UnityEngine.Debug.Log("Rotation reliable since : " + _elapsedTimeSinceLastInvalidRotation);
			//}
			//else
			//{
			//	UnityEngine.Debug.LogWarning("Rotation is not reliable !");
			//}

			return isRotationReliablyTracked;
		}

		private bool _UpdatePositionState(bool pIsPartiallyTracked, bool pIsFullyTracked, float pDeltaTime, float pIMUOnlyReliablePositionDuration)
		{
			bool isPositionReliablyTracked = false;
			if (pIsFullyTracked)
			{
				_elapsedTimeSinceLastFullTrack = 0f; // reset only when full tracking happens
				isPositionReliablyTracked = true; // full tracking is always reliable
			}
			else if (pIsPartiallyTracked && _wasPositionReliablyTracked)
			{
				_elapsedTimeSinceLastFullTrack += pDeltaTime; // count time elapsed without optical tracking
				isPositionReliablyTracked = _elapsedTimeSinceLastFullTrack <= pIMUOnlyReliablePositionDuration; // only this duration without optical tracking is reliable
			}

			// Save states for next iteration.
			bool wasPositionPartiallyTracked = _wasPositionPartiallyTracked;
			bool wasPositionReliablyTracked = _wasPositionReliablyTracked;
			_wasPositionPartiallyTracked = pIsPartiallyTracked;
			_wasPositionReliablyTracked = isPositionReliablyTracked;

			// Reject unreliable first frames when going from an untracked state to a reliable tracked state.
			if (isPositionReliablyTracked)
			{
				if (wasPositionReliablyTracked)
				{
					// count time elapsed since the last invalid position.
					_elapsedTimeSinceLastInvalidPosition += pDeltaTime;
				}
				else if (!wasPositionPartiallyTracked)
				{
					// reset counter if transitioning from an untracked state to a reliable tracked state.
					_elapsedTimeSinceLastInvalidPosition = pDeltaTime;
				}
				isPositionReliablyTracked = _elapsedTimeSinceLastInvalidPosition > ViveTracker.ImuOnlyMinDuration;
			}

			// Debug
			//if (isPositionReliablyTracked)
			//{
			//	UnityEngine.Debug.Log("Position reliable since : " + _elapsedTimeSinceLastInvalidPosition);
			//}
			//else
			//{
			//	UnityEngine.Debug.LogWarning("Position is not reliable !");
			//}

			return isPositionReliablyTracked;
		}
	}
}
