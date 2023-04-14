![alt text](Doc/ViveTrackers_Doc.png)

## Use HTC Vive Tracker devices in Unity3D with the following benefits
- Directly use OpenVR API for best performance
- Current features include update of positions, rotations, and Pogo-Pins buttons
- Small library, easy to integrate in any projects
- A simulator, to develop without having a Vive tracking system connected
- A complete test scene to understand the use of the library
- Some [documentation](Doc/ViveTrackersDocumentation.pdf) to setup Unity3D, SteamVR (**with & without an HMD connected**), and Windows to get the best tracking quality from your Vive Trackers

## Getting Started
#### 1. Open SteamVR, configure your tracking space, connect your Vive Trackers, and leave SteamVR running in the background.
#### 2. Open the ViveTrackersTest scene in Unity and press Play.
#### 3. Hotkeys used to control the application:
- **F1** : show/hide local reference frames of ViveTrackers.
- **F5** : refresh the list of currently connected ViveTrackers.
- **F8** : calibrate the ViveTrackers (make their local reference frames aligned with the **O**rigin reference frame).
- **F6** : save the last calibration.
- **F7** : load the last calibration.
#### 4. Understanding the code: 
- The update of all ViveTrackers (position, rotation, and optionally buttons) happens at the first line in [ViveTrackersTest.Update()](Scripts/ViveTrackersTest.cs#L52-L77), see [ViveTrackersManager.UpdateTrackers()](Scripts/ViveTrackersManager.cs#L117-L144) for more details.
- ViveTrackersManager contains a list of all connected ViveTrackers.
- Optionally, ViveTrackersManager can create only a restricted set of ViveTrackers declared in the file [ViveTrackers.csv](Scripts/ViveTrackers.csv) (see [documentation/"Keep Vive Trackers identification consistent during runtime"](Doc/ViveTrackersDocumentation.pdf)).
- To log your ViveTrackers serial numbers in the console, ensure ViveTrackersManager.logTrackersDetection is enabled in Unity editor.
- You can access the last position/rotation of ViveTrackers using their transform.localPosition/localRotation.
- You can also register to some [actions](Scripts/ViveTracker.cs#L190-L197) to know if the pogo-pins corresponding to Grip/Trigger/TouchPad/Menu buttons are pressed or released.

## System requirements
Unity ***2017.4.35f1*** and newer versions
