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
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ViveTrackers
{
	/// <summary>
	/// Base class to manage ViveTrackers.
	/// </summary>
	public abstract class ViveTrackersManagerBase : MonoBehaviour
	{
		public const string CalibrationFileFormat = "{0};{1};{2};{3};{4}";

		[Tooltip("Template used to instantiate available trackers")]
		public ViveTracker prefab;
		[Tooltip("The origin of the tracking space (+ used as the default rotation to calibrate Trackers' rotations")]
		public DebugTransform origin;
		[Tooltip("The path of the file containing the calibrations of the lastly used trackers")]
		public string calibFilePath = "ViveTrackers_Calibrations.csv";

		// All trackers found after a call to RefreshTrackers().
		protected List<ViveTracker> _trackers = new List<ViveTracker>();

		public Action<List<ViveTracker>> TrackersFound; // This callback is called everytime you call the RefreshTrackers() method.

		/// <summary>
		/// Update ViveTracker transforms.
		/// </summary>
		public abstract void UpdateTrackers(float pDeltaTime);

		/// <summary>
		/// Scan for available Vive Tracker devices and creates ViveTracker objects accordingly.
		/// Init OpenVR if not already done.
		/// </summary>
		public abstract void RefreshTrackers();

		public void SetDebugActive(bool pActive)
		{
			origin.SetDebugActive(pActive);
			foreach (ViveTracker tracker in _trackers)
			{
				tracker.debugTransform.SetDebugActive(pActive);
			}
		}

		/// <summary>
		/// Align all trackers' transformation with origin's transformation.
		/// </summary>
		public void CalibrateTrackers()
		{
			foreach (var tracker in _trackers)
			{
				tracker.Calibrate();
			}
		}

		/// <summary>
		/// Save trackers calibrations to file.
		/// </summary>
		public void SaveTrackersCalibrations()
		{
			// Overwrite any existing file.
			using (StreamWriter writer = new StreamWriter(calibFilePath, false, Encoding.Default))
			{
				// Write Header
				writer.WriteLine(string.Format(CalibrationFileFormat, "Name", "qx", "qy", "qz", "qw"));
				// Write Data
				Quaternion calib;
				foreach (ViveTracker tracker in _trackers)
				{
					calib = tracker.Calibration;
					writer.WriteLine(string.Format(CalibrationFileFormat, tracker.name, calib.x, calib.y, calib.z, calib.w));
				}
			}
			Debug.Log("[ViveTrackersManagerBase] " + _trackers.Count + " trackers calibrations saved to file : " + calibFilePath);
		}

		/// <summary>
		/// Load trackers calibrations from file.
		/// </summary>
		public void LoadTrackersCalibrations()
		{
			if(File.Exists(calibFilePath))
			{
				int successCount = 0;
				// Read calib file
				using (StreamReader reader = File.OpenText(calibFilePath))
				{
					// Read Header
					string line = reader.ReadLine();
					char separator = line.Contains(";") ? ';' : ',';
					// Read Data
					while ((line = reader.ReadLine()) != null)
					{
						string[] items = line.Split(separator);
						string trackerName = items[0];
						// Set Calibration to the existing tracker.
						ViveTracker tracker = _trackers.Find(trckr => trckr.name == trackerName);
						if (tracker != null)
						{
							float qx = float.Parse(items[1]);
							float qy = float.Parse(items[2]);
							float qz = float.Parse(items[3]);
							float qw = float.Parse(items[4]);
							tracker.Calibration = new Quaternion(qx, qy, qz, qw);
							++successCount;
						}
					}
				}
				Debug.Log("[ViveTrackersManagerBase.LoadTrackersCalibrations()] " + successCount + " trackers calibrations loaded from file : " + calibFilePath);
			}
			else
			{
				Debug.LogWarning("[ViveTrackersManagerBase.LoadTrackersCalibrations()] file not found : " + calibFilePath + " !");
			}
		}
	}
}