using System;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections.LowLevel.Unsafe;

static class TextureReadback
{
    // Texture readback as Span with AsyncGPUReadback in a synced fashion
    public unsafe static ReadOnlySpan<Color32> AsSpan(this Texture source)
    {
        var req = AsyncGPUReadback.Request(source);

        req.WaitForCompletion();
        if (req.hasError) return ReadOnlySpan<Color32>.Empty;

        var data = req.GetData<Color32>(0);

        var ptr = NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(data);
        return new Span<Color32>(ptr, data.Length);
    }
}
