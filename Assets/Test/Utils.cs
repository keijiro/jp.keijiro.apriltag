using AprilTag;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;

static class MatdExtensions
{
    public static float3 AsFloat3(this ref Matd3x1 src)
      => math.float3((float)src.e0, (float)src.e1, (float)src.e2);

    public static float3x3 AsFloat3x3(this ref Matd3x3 src)
      => math.float3x3((float)src.e00, (float)src.e01, (float)src.e02,
                       (float)src.e10, (float)src.e11, (float)src.e12,
                       (float)src.e20, (float)src.e21, (float)src.e22);
}

[BurstCompile]
static class ImageUtil
{
    unsafe public static void
      CopyRawTextureData(NativeArray<byte> data, ImageU8 image)
    {
        var src = (byte*)data.GetUnsafeReadOnlyPtr();
        fixed (byte* dst = &image.Buffer.GetPinnableReference())
            BurstCopy(src, dst, image.Width, image.Height, image.Stride);
    }

    unsafe public static void
      Convert(ReadOnlySpan<Color32> data, ImageU8 image)
    {
        fixed (Color32* src = &data.GetPinnableReference())
            fixed (byte* dst = &image.Buffer.GetPinnableReference())
                BurstConvert(src, dst, image.Width, image.Height, image.Stride);
    }

    [BurstCompile]
    unsafe static void BurstCopy
      (byte* src, byte* dst, int width, int height, int stride)
    {
        var offs_src = 0;
        var offs_dst = stride * (height - 1);

        for (var y = 0; y < height; y++)
        {
            UnsafeUtility.MemCpy(dst + offs_dst, src + offs_src, width);

            offs_src += stride;
            offs_dst -= stride;
        }
    }

    [BurstCompile]
    unsafe static void BurstConvert
      (Color32* src, byte* dst, int width, int height, int stride)
    {
        var offs_src = 0;
        var offs_dst = stride * (height - 1);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
                dst[offs_dst + x] = src[offs_src + x].g;

            offs_src += stride;
            offs_dst -= stride;
        }
    }
}
