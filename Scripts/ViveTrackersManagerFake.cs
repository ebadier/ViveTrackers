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
	/// <summary>
	/// Class to simulate a Vive tracking system.
	/// </summary>
	public sealed class ViveTrackersManagerFake : ViveTrackersManagerBase
	{
		[Header("Procedural Trajectories")]
		public float minDuration = 1f;
		public float maxDuration = 5f;
		public float minSpeed = 0f;
		public float maxSpeed = 3f;
		public float targetsHeight = 1.7f;
		[Range(1f, 10f)]
		public float areaRadius = 3f;

		private List<float> _trackerElapsedTimes = new List<float>();
		private List<float> _trackerDurations = new List<float>();
		private List<float> _trackerSpeeds = new List<float>();

		private List<Transform> _waypoints;

		private void Awake()
		{
			_waypoints = new List<Transform>();
			GameObject parentWaypoints = new GameObject("Waypoints");
			parentWaypoints.transform.parent = origin.transform;
			parentWaypoints.transform.localPosition = Vector3.zero;
			parentWaypoints.transform.localRotation = Quaternion.identity;
			for (float angle = 0f; angle < 360f; angle += 10f)
			{
				float radians = angle * Mathf.Deg2Rad;
				Vector2 dir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized * areaRadius;
				GameObject wayPoint = new GameObject(string.Format("Waypoint_{0:0}", angle));
				wayPoint.transform.parent = parentWaypoints.transform;
				wayPoint.transform.localPosition = new Vector3(dir.x, targetsHeight, dir.y);
				wayPoint.transform.localRotation = Quaternion.identity;
				_waypoints.Add(wayPoint.transform);
			}
		}

		/// <summary>
		/// Update fake ViveTrackers' transforms procedurally.
		/// </summary>
		public override void UpdateTrackers(float pDeltaTime)
		{
			float sqrAreaRadius = areaRadius * areaRadius;
			Vector3 volumeCenter = new Vector3(0f, targetsHeight, 0f);
			Vector3 dir, motion;
			Vector3 pos, newPos;
			for(int i = 0; i <  _trackers.Count; ++i)
			{
				dir = _trackers[i].transform.localRotation * Vector3.forward;
				pos = _trackers[i].transform.localPosition;
				dir = _NextTrackerDirection(i, pos, dir, pDeltaTime);
				motion = dir * _trackerSpeeds[i] * pDeltaTime;
				newPos = pos + motion;
				// Check if target will be out of bounds.
				if ((newPos - volumeCenter).sqrMagnitude > sqrAreaRadius)
				{
					// If yes, invert direction.
					dir = -dir;
					motion = -motion;
					newPos = pos + motion;
				}
				_trackers[i].UpdateState(true, true, true, newPos, Quaternion.LookRotation(dir, Vector3.up), pDeltaTime);
			}
		}

		/// <summary>
		/// Generate fake ViveTrackers.
		/// </summary>
		public override void RefreshTrackers()
		{
			// Delete existing ViveTrackers
			foreach (ViveTracker tracker in _trackers)
			{
				Destroy(tracker.gameObject);
			}
			_trackers.Clear();
			_trackerDurations.Clear();
			_trackerElapsedTimes.Clear();
			_trackerSpeeds.Clear();

			Debug.Log("[ViveTrackersManagerFake] Generating fake Tracker devices...");
			for(char trackerName = 'A'; trackerName < 'I'; ++trackerName)
			{
				Vector2 pos = Random.insideUnitCircle * 0.5f;
				Vector3 startPos = new Vector3(pos.x, targetsHeight, pos.y);
				ViveTracker vt = Instantiate<ViveTracker>(prefab, origin.transform.TransformPoint(startPos), Quaternion.identity, origin.transform);
				vt.Init(new ViveTrackerID((uint)_trackers.Count, trackerName.ToString()), trackerName.ToString());
				_trackers.Add(vt);
				_trackerDurations.Add(0f);
				_trackerElapsedTimes.Add(0f);
				_trackerSpeeds.Add(Random.Range(minSpeed, maxSpeed));
			}

			Debug.Log(string.Format("[ViveTrackersManagerFake] {0} trackers created:", _trackers.Count));
			foreach (ViveTracker tracker in _trackers)
			{
				Debug.Log(string.Format("[ViveTrackersManagerFake] -> Tracker : Name = {0} ; SN = {1} ; Index = {2}", tracker.name, tracker.ID.TrackedDevice_SerialNumber, tracker.ID.TrackedDevice_Index));
			}

			// Fire Action.
			if (TrackersFound != null)
			{
				TrackersFound(_trackers);
			}
		}

		private Vector3 _NextTrackerDirection(int pTrackerIndex, Vector3 pCurrentPos, Vector3 pCurrentDir, float pDt)
		{
			Vector3 dir = pCurrentDir;
			_trackerElapsedTimes[pTrackerIndex] += pDt;
			if(_trackerElapsedTimes[pTrackerIndex] >= _trackerDurations[pTrackerIndex])
			{
				// Direction change needed.
				_trackerElapsedTimes[pTrackerIndex] = 0f;
				_trackerDurations[pTrackerIndex] = Random.Range(minDuration, maxDuration);
				_trackerSpeeds[pTrackerIndex] = Random.Range(minSpeed, maxSpeed);
				Vector3 randomTarget = _waypoints[Random.Range(0, _waypoints.Count)].localPosition;
				dir = (randomTarget - pCurrentPos).normalized;
			}
			return dir;
		}
	}
}