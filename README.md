# jp.keijiro.apriltag – AprilTag Tracker for Unity

![AprilTag Demo](https://i.imgur.com/1iushmq.gif)

**`jp.keijiro.apriltag`** is a Unity plugin that provides a native implementation of the [AprilTag](https://april.eecs.umich.edu/software/apriltag) marker tracking system.

AprilTag is a high-performance fiducial marker system developed by the APRIL Robotics Lab at the University of Michigan. It enables fast, robust tracking of 2D markers in video feeds, making it ideal for computer vision applications in robotics, AR/VR, and motion tracking.

---

## ✅ System Requirements

* **Unity version**: 2021.3 or newer
* **Supported platforms**:

  * Windows (x86-64)
  * macOS (x86-64)
  * Linux (x86-64)
  * iOS (arm64)
  * Android (arm64)

---

## 📦 Installation

This package is distributed via a scoped NPM registry:

1. Add the following scoped registry to your project’s `manifest.json`:

```json
"scopedRegistries": [
  {
    "name": "Keijiro",
    "url": "https://registry.npmjs.com",
    "scopes": ["jp.keijiro"]
  }
]
```

2. Add the package to your dependencies:

```json
"jp.keijiro.apriltag": "latest"
```

For step-by-step guidance, check out [Keijiro's scoped registry setup gist](https://gist.github.com/keijiro/f8c7e8ff29bfe63d86b888901b82644c).

---

## 🧪 Try the Sample Scene

1. Clone this repository.
2. Open the `DetectionTest.unity` scene in the Unity Editor.
3. Press Play to run the tag detection demo.

⚠️ The current implementation supports the `tagStandard41h12` tag family only. Download printable versions from the [apriltag-imgs repository](https://github.com/AprilRobotics/apriltag-imgs/tree/master/tagStandard41h12). You can either print them or display them on a screen (e.g., a smartphone).

### 🎥 Camera Setup

Accurate tag position estimation depends on correctly configuring the camera’s Field of View (FoV).

* Set the camera’s **FoV axis** to **Horizontal**.
* Enter the actual **FoV value in degrees** of your physical camera.

Example: If you're using a [Zoom Q2n-4K](https://zoomcorp.com/en/us/video-recorders/video-recorders/q2n-4k-handy-video-recorder/) in mid-angle mode (78° horizontal FoV), set:

* FoV Axis: `Horizontal`
* FoV: `78`

![FoV Setup Example](https://i.imgur.com/BUVHSnXl.jpg)

---

## 🧠 How to Use the Detector in Code

### 1. Initialize the Detector

```csharp
var detector = new AprilTag.TagDetector(width, height, decimation);
```

* `width`, `height`: dimensions of the input image
* `decimation`: scale factor (e.g., `2.0` for half-resolution input). Improves performance at the cost of precision.

### 2. Process the Image

```csharp
texture.GetPixels32(buffer);
detector.ProcessImage(buffer, fovDegrees, tagSizeMeters);
```

* `buffer`: array of `Color32` pixels (RGBA format)
* `fovDegrees`: camera’s horizontal field of view
* `tagSizeMeters`: physical size of the tag

### 3. Access Detected Tags

```csharp
foreach (var tag in detector.DetectedTags)
{
    Debug.Log($"Tag ID: {tag.ID}, Position: {tag.Position}, Rotation: {tag.Rotation}");
}
```

### 4. Clean Up

```csharp
detector.Dispose();
```

For a working example, check out the [DetectionTest.cs](Assets/DetectionTest.cs) script.

---

## 🔗 Related Resources

* 🔧 AprilTag Core Library:
  [https://github.com/AprilRobotics/apriltag](https://github.com/AprilRobotics/apriltag)
* 🖼️ Pre-Generated Tag Images:
  [https://github.com/AprilRobotics/apriltag-imgs](https://github.com/AprilRobotics/apriltag-imgs)
* 🔌 Unity Plugin Build Fork:
  [https://github.com/keijiro/apriltag](https://github.com/keijiro/apriltag)
