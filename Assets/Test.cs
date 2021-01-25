using UnityEngine;
using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

sealed class Test : MonoBehaviour
{
    [DllImport("AprilTag", EntryPoint="apriltag_detector_create")]
    private static extern IntPtr CreateDetector();

    [DllImport("AprilTag", EntryPoint="apriltag_detector_add_family_bits")]
    private static extern void AddFamilyBits(IntPtr detector, IntPtr family, int correctedBits);

    [DllImport("AprilTag", EntryPoint="apriltag_detector_destroy")]
    private static extern void DestroyDetector(IntPtr ptr);

    [DllImport("AprilTag", EntryPoint="tagStandard41h12_create")]
    private static extern IntPtr CreateTagStandard41h12();

    [DllImport("AprilTag", EntryPoint="tagStandard41h12_destroy")]
    private static extern void DestroyTagStandard41h12(IntPtr ptr);

    void Start ()
    {
        var detector = CreateDetector();
        var family = CreateTagStandard41h12();
        AddFamilyBits(detector, family, 2);
        DestroyTagStandard41h12(family);
        DestroyDetector(detector);
    }
}
