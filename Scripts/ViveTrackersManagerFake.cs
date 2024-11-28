/******************************************************************************************************************************************************
* MIT License																																		  *
*																																					  *
* Copyright (c) 2024																																  *
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

		private List<bool> _trackerDoingHalfTurn = new List<bool>();
		private List<Vector3> _trackerTargetLocalDirections = new List<Vector3>();
		private List<float> _trackerElapsedTimes = new List<float>();
		private List<float> _trackerDurations = new List<float>();
		private List<float> _trackerSpeeds = new List<float>();

		private List<Transform> _waypoints;

		private void Awake()
		{
			_waypoints = new List<Transform>();
			for (float angle = 0f; angle < 360f; angle += 10f)
			{
				float radians = angle * Mathf.Deg2Rad;
				Vector2 dir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized * areaRadius;
				GameObject wayPoint = new GameObject(string.Format("Waypoint_{0:0}", angle));
				wayPoint.transform.parent = origin.transform; // same origin as Trackers
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
			Vector3 localDir, newLocalDir;
			Vector3 localPos, newLocalPos;
			for (int i = 0; i < _trackers.Count; ++i)
			{
				localDir = Quaternion.Inverse(_trackers[i].Calibration) * _trackers[i].transform.localRotation * Vector3.forward;
				localPos = _trackers[i].transform.localPosition;
				_NextTrackerPose(i, localPos, localDir, sqrAreaRadius, pDeltaTime, out newLocalPos, out newLocalDir);
				_trackers[i].UpdatePose(true, true, true, newLocalPos, Quaternion.LookRotation(newLocalDir, Vector3.up), pDeltaTime);
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
			_trackerDoingHalfTurn.Clear();
			_trackerTargetLocalDirections.Clear();
			_trackerElapsedTimes.Clear();
			_trackerDurations.Clear();
			_trackerSpeeds.Clear();

			Debug.Log("[ViveTrackersManagerFake] Generating fake Tracker devices...");
			for(int i = 0; i < 8; ++i)
			{
				Vector2 pos = Random.insideUnitCircle * 0.5f;
				Vector3 startPos = new Vector3(pos.x, targetsHeight, pos.y);
				ViveTracker vt = Instantiate<ViveTracker>(prefab, origin.transform.TransformPoint(startPos), Quaternion.identity, origin.transform);
				string trackerName = i.ToString();
				vt.Init(new ViveTrackerID((uint)i, trackerName), trackerName);
				_trackers.Add(vt);
				_trackerDoingHalfTurn.Add(false);
				_trackerTargetLocalDirections.Add(Vector3.forward);
				_trackerElapsedTimes.Add(0f);
				_trackerDurations.Add(0f);
				_trackerSpeeds.Add(Random.Range(minSpeed, maxSpeed));
			}

			Debug.Log(string.Format("[ViveTrackersManagerFake] {0} trackers created:", _trackers.Count));
			foreach (ViveTracker tracker in _trackers)
			{
				Debug.Log(string.Format("[ViveTrackersManagerFake] -> Tracker : Name = {0} ; SN = {1} ; Index = {2}", tracker.name, tracker.ID.trackedDevice_SerialNumber, tracker.ID.trackedDevice_Index));
			}

			// Fire Action.
			if (TrackersFound != null)
			{
				TrackersFound(_trackers);
			}
		}

		private void _NextTrackerPose(int pTrackerIndex, Vector3 pCurrentLocalPos, Vector3 pCurrentLocalDir, float pSqrAreaRadius, float pDt, out Vector3 pNewLocalPos, out Vector3 pNewLocalDir)
		{
			_trackerElapsedTimes[pTrackerIndex] += pDt;
			if (_trackerElapsedTimes[pTrackerIndex] >= _trackerDurations[pTrackerIndex])
			{
				Vector3 randomLocalTarget = _waypoints[Random.Range(0, _waypoints.Count)].localPosition;
				_trackerTargetLocalDirections[pTrackerIndex] = (randomLocalTarget - pCurrentLocalPos).normalized;
				_trackerElapsedTimes[pTrackerIndex] = 0f;
				_trackerDurations[pTrackerIndex] = Random.Range(minDuration, maxDuration);
				_trackerSpeeds[pTrackerIndex] = Random.Range(minSpeed, maxSpeed);
				_trackerDoingHalfTurn[pTrackerIndex] = false;
			}

			pNewLocalDir = Vector3.Lerp(pCurrentLocalDir, _trackerTargetLocalDirections[pTrackerIndex], _trackerElapsedTimes[pTrackerIndex] / _trackerDurations[pTrackerIndex]);
			Vector3 move = pNewLocalDir * _trackerSpeeds[pTrackerIndex] * pDt;
			pNewLocalPos = pCurrentLocalPos + move;

			// Check if a half-turn is needed
			if ((!_trackerDoingHalfTurn[pTrackerIndex]) && (pNewLocalPos.sqrMagnitude > pSqrAreaRadius))
			{
				_trackerTargetLocalDirections[pTrackerIndex] = -pCurrentLocalDir; // negate the direction that led us here.
				_trackerElapsedTimes[pTrackerIndex] = 0f;
				_trackerDoingHalfTurn[pTrackerIndex] = true;
			}

			if (_trackerDoingHalfTurn[pTrackerIndex])
			{
				pNewLocalDir = Vector3.Lerp(pCurrentLocalDir, _trackerTargetLocalDirections[pTrackerIndex], _trackerElapsedTimes[pTrackerIndex] / _trackerDurations[pTrackerIndex]);
				pNewLocalPos = pCurrentLocalPos; // Stay in place while half-turning
			}
		}
	}
}