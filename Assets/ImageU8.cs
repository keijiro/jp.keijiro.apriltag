using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AprilTag {

public sealed class ImageU8 : SafeHandleZeroOrMinusOneIsInvalid
{
    #region SafeHandle implementation

    ImageU8() : base(true) {}

    protected override bool ReleaseHandle()
    {
        _Destroy(handle);
        return true;
    }

    #endregion

    #region Public methods

    public static ImageU8 Create(int width, int height)
      => _Create((uint)width, (uint)height);

    #endregion

    #region Unmanaged interface

    [DllImport("AprilTag", EntryPoint="image_u8_create")]
    static extern ImageU8 _Create(uint width, uint height);

    [DllImport("AprilTag", EntryPoint="image_u8_destroy")]
    static extern void _Destroy(IntPtr image);

    #endregion
}

} // namespace AprilTag
