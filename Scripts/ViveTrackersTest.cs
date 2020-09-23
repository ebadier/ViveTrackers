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

using System.Collections.Generic;
using UnityEngine;

namespace ViveTrackers
{
	public sealed class ViveTrackersTest : MonoBehaviour
	{
		public ViveTrackersManager viveTrackersManager;

		private Transform _mainCameraTransform = null;
		private bool _debugActive = true;
		private List<ViveTracker> _initializedTrackers = new List<ViveTracker>();

		private void Start()
		{
			_mainCameraTransform = Camera.main.transform;
			viveTrackersManager.origin.Init("O", Color.white, _mainCameraTransform);
			viveTrackersManager.TrackersFound += _OnTrackersFound;
			viveTrackersManager.RefreshTrackers();
		}

		private void Update()
		{
			viveTrackersManager.UpdateTrackers();

			if (Input.GetKeyUp(KeyCode.R))
			{
				viveTrackersManager.RefreshTrackers();
			}
			else if (Input.GetKeyUp(KeyCode.D))
			{
				_debugActive = !_debugActive;
				viveTrackersManager.SetDebugActive(_debugActive);
			}
			else if (Input.GetKeyUp(KeyCode.C))
			{
				viveTrackersManager.CalibrateTrackers();
			}
		}

		private void _OnTrackersFound(List<ViveTracker> pTrackers)
		{
			foreach (ViveTracker viveTracker in pTrackers)
			{
				if(!_initializedTrackers.Exists(vt => vt.name == viveTracker.name))
				{
					viveTracker.debugTransform.Init(viveTracker.name, Random.ColorHSV(), _mainCameraTransform);
					viveTracker.Calibrated += _OnTrackerCalibrated;
					_initializedTrackers.Add(viveTracker);
				}
			}
		}

		private void _OnTrackerCalibrated(ViveTracker pTracker)
		{
			Debug.Log("[ViveTrackersTest] ViveTracker " + pTracker.name + " calibrated.");
		}
	}
}