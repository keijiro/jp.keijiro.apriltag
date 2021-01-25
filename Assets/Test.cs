using UnityEngine;
using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

sealed class Test : MonoBehaviour
{
    [DllImport("AprilTag", EntryPoint="apriltag_detector_create")]
    private static extern IntPtr CreateDetector();

    [DllImport("AprilTag", EntryPoint="apriltag_detector_add_family_bits")]
    private static extern void AddFamilyBits(IntPtr detector, IntPtr family, int correctedBits);

    [DllImport("AprilTag", EntryPoint="apriltag_detector_remove_family")]
    private static extern void RemoveFamily(IntPtr detector, IntPtr family);

    [DllImport("AprilTag", EntryPoint="apriltag_detector_detect")]
    private static extern IntPtr Detect(IntPtr detector, IntPtr image);

    [DllImport("AprilTag", EntryPoint="apriltag_detections_destroy")]
    private static extern void DestroyDetections(IntPtr detections);

    [DllImport("AprilTag", EntryPoint="apriltag_detector_destroy")]
    private static extern void DestroyDetector(IntPtr ptr);

    [DllImport("AprilTag", EntryPoint="tagStandard41h12_create")]
    private static extern IntPtr CreateTagStandard41h12();

    [DllImport("AprilTag", EntryPoint="tagStandard41h12_destroy")]
    private static extern void DestroyTagStandard41h12(IntPtr ptr);

    [DllImport("AprilTag", EntryPoint="image_u8_create_from_pnm")]
    private static extern IntPtr ImageU8CreateFromPnm(string path);

    void Start ()
    {
        var detector = CreateDetector();
        var family = CreateTagStandard41h12();
        var image = ImageU8CreateFromPnm("/Users/keijiro/Documents/AprilTagTest/Assets/Test.png");
        Debug.Log(image);
        AddFamilyBits(detector, family, 2);
        //var detections = Detect(detector, image);
        /*
        DestroyDetections(detections);
        RemoveFamily(detector, family);
        DestroyTagStandard41h12(family);
        DestroyDetector(detector);
        */
    }
}
