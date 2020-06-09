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