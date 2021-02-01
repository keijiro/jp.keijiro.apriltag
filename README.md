jp.keijiro.apriltag - AprilTag package for Unity
================================================

![gif](https://i.imgur.com/1iushmq.gif)

**jp.keijiro.apriltag** is a Unity package that provides an implementation of
the AprilTag tracker.

**AprilTag** is a marker based tracking system developed by the APRIL Robotics
Laboratory at the University of Michigan. Please see [the AprilTag web page]
for further details.

[the AprilTag web page]: https://april.eecs.umich.edu/software/apriltag

System requirements
-------------------

- Unity 2020.2

At the moment, this package supports the following systems:

- Windows (x86-64), macOS (x86-64), Linux (x86-64), iOS (arm64), Android (arm64)

How to try the sample project
-----------------------------

Clone this repository and play the `WebCamTest.unity` scene on Unity Editor.

The current version of the TagDetector component only supports the
`tagStandard41h12` tag set. You can download those tag images from
[the apriltag-imgs repository]. Print some of them using a printer. You also can
use a smartphone screen to display the tags.

[the apriltag-imgs repository]:
  https://github.com/AprilRobotics/apriltag-imgs/tree/master/tagStandard41h12

The `WebCamTest` component uses the Field of View value of the main camera to
estimate tag positions. You can try the sample without adjusting it, but it may
give incorrect depth information. To get accurate tag positions, you should
match the FoV value with the actual camera FoV.

![image](https://i.imgur.com/BUVHSnXl.jpg)

For example, I'm using [Zoom Q2n-4K video camera] for testing, which gives about
78 degrees horizontal FoV at the mid-angle mode. So I changed the FOV Axis to
"Horizontal" and the Field of View value to 78.

[Zoom Q2n-4K video camera]:
  https://zoomcorp.com/en/us/video-recorders/video-recorders/q2n-4k-handy-video-recorder/

How to install the package
--------------------------

This package uses the [scoped registry] feature to import dependent packages.
Please add the following sections to the package manifest file
(`Packages/manifest.json`).

To the `scopedRegistries` section:

```
{
  "name": "Unity NuGet",
  "url": "https://unitynuget-registry.azurewebsites.net",
  "scopes": [ "org.nuget" ]
},
{
  "name": "Keijiro",
  "url": "https://registry.npmjs.com",
  "scopes": [ "jp.keijiro" ]
}
```

To the `dependencies` section:

```
"jp.keijiro.apriltag": "1.0.1"
```

After changes, the manifest file should look like below:

```
{
  "scopedRegistries": [
    {
      "name": "Unity NuGet",
      "url": "https://unitynuget-registry.azurewebsites.net",
      "scopes": [ "org.nuget" ]
    },
    {
      "name": "Keijiro",
      "url": "https://registry.npmjs.com",
      "scopes": [ "jp.keijiro" ]
    }
  ],
  "dependencies": {
    "jp.keijiro.apriltag": "1.0.1",
...
```

[scoped registry]: https://docs.unity3d.com/Manual/upm-scoped.html

How to detect tags
------------------

At first, create the `AprilTag.TagDetector` object specifying the input image
dimensions. You can run the detector in a lower resolution by specifying a
decimation factor. It may improve the speed at the cost of accuracy and
detection rate.

```csharp
detector = new AprilTag.TagDetector(imageWidth, imageHeight, decimation);
```

Call the `ProcessImage` method every frame to detect tags from an input image.
You can use a `Color32` array to give an image. At the same time, you have to
specify the camera FoV (horizontal) in degrees and the tag size in meters.

```csharp
webcamTexture.GetPixels32(buffer);
detector.ProcessImage(buffer, fov, tagSize);
```

You can retrieve the detected tags from the `DetectedTags` property.

```csharp
foreach (var tag in detector.DetectedTags)
    Debug.Log($"{tag.ID} {tag.Position} {tag.Rotation}");
```

Dispose the detector object when you no longer need it.

```csharp
detector.Dispose();
```

For details, please check the [WebCamTest.cs] example, which shows how to
detect tags from images with `WebCamTexture`.

[WebCamTest.cs]: /Assets/WebcamTest.cs

Related repositories
--------------------

- The original AprilTag project repository:
  https://github.com/AprilRobotics/apriltag
- Pre-generated tag images:
  https://github.com/AprilRobotics/apriltag-imgs
- A fork of the AprilTag repository used to build Unity plugin binaries.
  https://github.com/keijiro/apriltag
