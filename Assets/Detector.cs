using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AprilTag {

public sealed class Detector : SafeHandleZeroOrMinusOneIsInvalid
{
    #region SafeHandle implementation

    Detector() : base(true) {}

    protected override bool ReleaseHandle()
    {
        _Destroy(handle);
        return true;
    }

    #endregion

    #region Public methods

    public static Detector Create()
      => _Create();

    public void AddFamily(Family family)
      => _AddFamilyBits(this, family, 2);

    public void RemoveFamily(Family family)
      => _RemoveFamily(this, family);

    public DetectionArray Detect(ImageU8 image)
      => _Detect(this, image);

    #endregion

    #region Unmanaged interface

    [DllImport("AprilTag", EntryPoint="apriltag_detector_create")]
    static extern Detector _Create();

    [DllImport("AprilTag", EntryPoint="apriltag_detector_destroy")]
    static extern void _Destroy(IntPtr detector);

    [DllImport("AprilTag", EntryPoint="apriltag_detector_add_family_bits")]
    static extern void _AddFamilyBits
      (Detector detector, Family family, int correctedBits);

    [DllImport("AprilTag", EntryPoint="apriltag_detector_remove_family")]
    static extern void _RemoveFamily(Detector detector, Family family);

    [DllImport("AprilTag", EntryPoint="apriltag_detector_detect")]
    static extern DetectionArray _Detect(Detector detector, ImageU8 image);

    #endregion
}

} // namespace AprilTag
