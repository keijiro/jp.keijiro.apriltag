using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AprilTag {

[StructLayoutAttribute(LayoutKind.Sequential)]
struct Detection
{
    IntPtr family;
    int id;
    int hamming;
    float decision_margin;
    IntPtr H;
    double c0, c1;
    double p00, p01;
    double p10, p11;
    double p20, p21;
    double p30, p31;
}

public sealed class DetectionArray : SafeHandleZeroOrMinusOneIsInvalid
{
    #region SafeHandle implementation

    DetectionArray() : base(true) {}

    protected override bool ReleaseHandle()
    {
        _Destroy(handle);
        return true;
    }

    #endregion

    #region zarray struct representation

    [StructLayoutAttribute(LayoutKind.Sequential)]
    internal struct InternalData
    {
        internal ulong el_sz;
        internal int size;
        internal int alloc;
        internal IntPtr data;
    }

    unsafe ref InternalData Data
      => ref Unsafe.AsRef<InternalData>((void*)handle);

    #endregion

    #region Public methods

    public int Length
      => (int)Data.size;

    #endregion

    #region Unmanaged interface

    [DllImport("AprilTag", EntryPoint="apriltag_detections_destroy")]
    static extern void _Destroy(IntPtr detections);

    #endregion
}

} // namespace AprilTag
