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
using System.IO;
using System.Text;
using UnityEngine;
using Valve.VR;

namespace ViveTrackers
{
	/// <summary>
	/// This class is used to manage Vive Tracker devices using OpenVR API.
	/// To run correctly, this class needs SteamVR application to run on the same computer.
	/// 1) To create the trackers, call the RefreshTrackers() method. This method can be called multiple times during runtime.
	/// - You can define a restricted set of Vive Tracker to create during runtime using the config file ViveTrackers.csv.
	/// - Using the config file or not, only the available connected devices in SteamVR are instantiated during runtime.
	/// 2) Once the trackers are created, you can update trackers'transforms using the UpdateTrackers() method.
	/// Example of config file content (# is used to comment):
	/// SerialNumber;Name;
	/// LHR-5850D511;A;
	/// LHR-9F7F5582;B;
	/// #LHR-3CECF391;C;
	/// #LHR-D5918492;D;
	/// #LHR-AC3ABE2E;E;
	/// </summary>
	public sealed class ViveTrackersManager : ViveTrackersManagerBase
	{
		[Tooltip("The path of the file containing the list of the restricted set of trackers to use")]
		public string configFilePath = "ViveTrackers.csv";
		[Tooltip("True, to create only the trackers declared in the config file. False, to create all connected trackers available in SteamVR.")]
		public bool createDeclaredTrackersOnly = false;
		[Tooltip("Log tracker detection or not. Useful to discover trackers' serial numbers")]
		public bool logTrackersDetection = true;

		private bool _ovrInit = false;
		private CVRSystem _cvrSystem = null;
		// Trackers declared in config file [TrackedDevice_SerialNumber, Name]
		private Dictionary<string, string> _declaredTrackers = new Dictionary<string, string>();
		// Poses for all tracked devices in OpenVR (HMDs, controllers, trackers, etc...).
		private TrackedDevicePose_t[] _ovrTrackedDevicePoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];

		private void Awake()
		{
			if (createDeclaredTrackersOnly)
			{
				if (File.Exists(configFilePath))
				{
					// Read config file
					using (StreamReader reader = File.OpenText(configFilePath))
					{
						// Read Header
						string line = reader.ReadLine();
						char separator = line.Contains(";") ? ';' : ',';
						// Read Data
						while ((line = reader.ReadLine()) != null)
						{
							// # is used to comment line
							if (!line.StartsWith("#", System.StringComparison.InvariantCulture))
							{
								string[] items = line.Split(separator);
								_declaredTrackers.Add(items[0], items[1]);
							}
						}
					}
					Debug.Log("[ViveTrackersManager] " + _declaredTrackers.Count + " trackers declared in config file : " + configFilePath);
				}
				else
				{
					Debug.LogWarning("[ViveTrackersManager] config file not found : " + configFilePath + " !");
				}
			}
		}

		/// <summary>
		/// Update ViveTracker transforms using the corresponding Vive Tracker devices.
		/// </summary>
		public override void UpdateTrackers(float pDeltaTime)
		{
			if (!_ovrInit)
			{
				return;
			}
			// Fetch last Vive Tracker devices poses.
			_cvrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, _ovrTrackedDevicePoses);
			// Apply poses to ViveTracker objects.
			foreach (var tracker in _trackers)
			{
				TrackedDevicePose_t pose = _ovrTrackedDevicePoses[tracker.ID.trackedDevice_Index];
				SteamVR_Utils.RigidTransform rigidTransform = new SteamVR_Utils.RigidTransform(pose.mDeviceToAbsoluteTracking);
				tracker.UpdateState(pose.bDeviceIsConnected, pose.bPoseIsValid, (pose.eTrackingResult == ETrackingResult.Running_OK), 
					rigidTransform.pos, rigidTransform.rot, pDeltaTime);
			}
		}

		/// <summary>
		/// Scan for available Vive Tracker devices and creates ViveTracker objects accordingly.
		/// Init OpenVR if not already done.
		/// </summary>
		public override void RefreshTrackers()
		{
			// Delete existing ViveTrackers
			foreach (ViveTracker tracker in _trackers)
			{
				Destroy(tracker.gameObject);
			}
			_trackers.Clear();

			// OVR check
			if (!_ovrInit)
			{
				_ovrInit = _InitOpenVR();
				if (!_ovrInit)
				{
					return;
				}
			}

			Debug.Log("[ViveTrackersManager] Scanning for Tracker devices...");
			for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; ++i)
			{
				ETrackedDeviceClass deviceClass = _cvrSystem.GetTrackedDeviceClass(i);
				if (deviceClass == ETrackedDeviceClass.GenericTracker)
				{
					string sn = _GetTrackerSerialNumber(i);
					if (logTrackersDetection)
					{
						Debug.Log("[ViveTrackersManager] Tracker detected : " + sn);
					}

					if (sn != "")
					{
						string trackerName = "";
						bool declared = _declaredTrackers.TryGetValue(sn, out trackerName);
						// Creates only trackers declared in config file or all (if !createDeclaredTrackersOnly).
						if (declared || !createDeclaredTrackersOnly)
						{
							ViveTracker vt = GameObject.Instantiate<ViveTracker>(prefab, origin.transform.position, origin.transform.rotation, origin.transform);
							vt.Init(new ViveTrackerID(i, sn), declared ? trackerName : sn);
							_trackers.Add(vt);
						}
					}
				}
			}

			// Check
			if (_trackers.Count == 0)
			{
				Debug.LogWarning("[ViveTrackersManager] No trackers available !");
				return;
			}

			// Sort Trackers by name.
			_trackers.Sort((ViveTracker x, ViveTracker y) => { return string.Compare(x.name, y.name); });

			Debug.Log(string.Format("[ViveTrackersManager] {0} trackers declared and {1} trackers available:", _declaredTrackers.Count, _trackers.Count));
			foreach (ViveTracker tracker in _trackers)
			{
				Debug.Log(string.Format("[ViveTrackersManager] -> Tracker : Name = {0} ; SN = {1} ; Index = {2}", tracker.name, tracker.ID.trackedDevice_SerialNumber, tracker.ID.trackedDevice_Index));
			}

			// Fire Action.
			if (TrackersFound != null)
			{
				TrackersFound(_trackers);
			}
		}

		private bool _InitOpenVR()
		{
			// OpenVR Init
			EVRInitError error = EVRInitError.None;
			_cvrSystem = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Other);
			if (error != EVRInitError.None)
			{
				Debug.LogError("[ViveTrackersManager] OpenVR Error : " + error);
				return false;
			}
			Debug.Log("[ViveTrackersManager] OpenVR initialized.");
			return true;
		}

		private string _GetTrackerSerialNumber(uint pTrackerIndex)
		{
			string sn = "";
			ETrackedPropertyError error = new ETrackedPropertyError();
			StringBuilder sb = new StringBuilder();
			_cvrSystem.GetStringTrackedDeviceProperty(pTrackerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String, sb, OpenVR.k_unMaxPropertyStringSize, ref error);
			if (error == ETrackedPropertyError.TrackedProp_Success)
			{
				sn = sb.ToString();
			}
			return sn;
		}
	}
}