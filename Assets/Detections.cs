using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AprilTag {

public sealed class Detections : SafeHandleZeroOrMinusOneIsInvalid
{
    #region SafeHandle implementation

    Detections() : base(true) {}

    protected override bool ReleaseHandle()
    {
        _Destroy(handle);
        return true;
    }

    #endregion

    #region Unmanaged interface

    [DllImport("AprilTag", EntryPoint="apriltag_detections_destroy")]
    static extern void _Destroy(IntPtr detections);

    #endregion
}

} // namespace AprilTag
