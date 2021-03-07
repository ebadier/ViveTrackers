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
		// The maximum duration a tracker can be considered as reliably tracked in a partially tracked state (IMU only).
		// Our tests shown that only the first second without optical tracking (IMU only) can be considered as a reliable tracking quality.
		public const float MaxPartiallyTrackedStateDuration = 1f;

		private bool _wasReliablyTracked; // previous tracking state.
		private float _elapsedTimeSinceLastFullTrack; // Time elapsed in a reliably tracked state since the last time the tracker was fully tracked.

		public ViveTrackingStateWatcher()
		{
			_wasReliablyTracked = false;
			_elapsedTimeSinceLastFullTrack = 0f;
		}

		/// <summary>
		/// Update the tracking state and returns whether the tracker can be considered as reliably tracked.
		/// </summary>
		public bool Update(bool pIsConnected, bool pIsPoseValid, bool pIsOpticallyTracked, float pDeltaTime)
		{
			bool isReliablyTracked = false;

			bool isPartiallyTracked = pIsConnected && pIsPoseValid; // IMU only.
			bool isFullyTracked = isPartiallyTracked && pIsOpticallyTracked; // IMU + Optical.
			if (isFullyTracked)
			{
				_elapsedTimeSinceLastFullTrack = 0f; // reset only when full tracking happens
				isReliablyTracked = true; // full tracking is always reliable
			}
			else if (isPartiallyTracked && _wasReliablyTracked)
			{
				_elapsedTimeSinceLastFullTrack += pDeltaTime; // count time while optical tracking lost
				isReliablyTracked = _elapsedTimeSinceLastFullTrack <= MaxPartiallyTrackedStateDuration;
			}

			// Save state for next iteration.
			_wasReliablyTracked = isReliablyTracked;

			return isReliablyTracked;
		}
	}
}
