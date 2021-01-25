using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

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

    #region image_u8 struct representation

    [StructLayout(LayoutKind.Sequential)]
    internal struct InternalData
    {
        internal int width;
        internal int height;
        internal int stride;
        internal IntPtr buf;
    }

    unsafe ref InternalData Data
      => ref Unsafe.AsRef<InternalData>((void*)handle);

    #endregion

    #region Public properties and methods

    public int Width => Data.width;
    public int Height => Data.height;
    public int Stride => Data.stride;
    public IntPtr Buffer => Data.buf;

    public static ImageU8 Create(int width, int height)
      => _Create((uint)width, (uint)height);

    unsafe public void SetImage(NativeSlice<byte> image)
    {
        UnsafeUtility.MemCpy((void*)Data.buf, image.GetUnsafeReadOnlyPtr(), image.Length);
    }

    #endregion

    #region Unmanaged interface

    [DllImport("AprilTag", EntryPoint="image_u8_create")]
    static extern ImageU8 _Create(uint width, uint height);

    [DllImport("AprilTag", EntryPoint="image_u8_destroy")]
    static extern void _Destroy(IntPtr image);

    #endregion
}

} // namespace AprilTag
