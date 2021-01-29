using AprilTag;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;

static class SystemConfig
{
    // TODO: It can be too much for high-end multicore CPUs.
    public static int PreferredThreadCount
      => Mathf.Max(1, Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount);
}

static class NativeArrayExtensions
{
    public static void
      CopyTo<T>(this NativeArray<T> array, List<T> list) where T : unmanaged
    {
        list.Clear();
        list.Capacity = array.Length;
        for (var i = 0; i < array.Length; i++) list.Add(array[i]);
    }
}

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

            offs_src += width;
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

            offs_src += width;
            offs_dst -= stride;
        }
    }
}

static class MeshUtil
{
    public static Mesh BuildTagMesh()
    {
        var vtx = new Vector3 [] { new Vector3(-0.5f, -0.5f, 0),
                                   new Vector3(+0.5f, -0.5f, 0),
                                   new Vector3(+0.5f, +0.5f, 0),
                                   new Vector3(-0.5f, +0.5f, 0),
                                   new Vector3(-0.5f, -0.5f, -1),
                                   new Vector3(+0.5f, -0.5f, -1),
                                   new Vector3(+0.5f, +0.5f, -1),
                                   new Vector3(-0.5f, +0.5f, -1),
                                   new Vector3(-0.2f, 0, 0),
                                   new Vector3(+0.2f, 0, 0),
                                   new Vector3(0, -0.2f, 0),
                                   new Vector3(0, +0.2f, 0),
                                   new Vector3(0, 0, 0),
                                   new Vector3(0, 0, -1.5f) };

        var idx = new int [] { 0, 1, 1, 2, 2, 3, 3, 0,
                               4, 5, 5, 6, 6, 7, 7, 4,
                               0, 4, 1, 5, 2, 6, 3, 7,
                               8, 9, 10, 11, 12, 13 };

        var mesh = new Mesh();
        mesh.vertices = vtx;
        mesh.SetIndices(idx, MeshTopology.Lines, 0);

        return mesh;
    }
}
