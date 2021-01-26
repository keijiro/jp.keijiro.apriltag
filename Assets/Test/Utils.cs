using AprilTag;
using Unity.Mathematics;

static class MatdExtensions
{
    public static float3 AsFloat3(this ref Matd3x1 src)
      => math.float3((float)src.e0, (float)src.e1, (float)src.e2);

    public static float3x3 AsFloat3x3(this ref Matd3x3 src)
      => math.float3x3((float)src.e00, (float)src.e01, (float)src.e02,
                       (float)src.e10, (float)src.e11, (float)src.e12,
                       (float)src.e20, (float)src.e21, (float)src.e22);
}
