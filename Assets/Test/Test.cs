using UnityEngine;
using AprilTag;
using Pose = AprilTag.Pose;
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

sealed class Test : MonoBehaviour
{
    [SerializeField] Texture2D _image = null;

    void Start ()
    {
        using var detector = Detector.Create();
        using var family = Family.CreateTagStandard41h12();
        detector.AddFamily(family);

        using var image = ImageU8.Create(256, 256);
        image.SetImage(_image.GetRawTextureData<byte>());

        using (var detections = detector.Detect(image))
        {
            for (var i = 0; i < detections.Length; i++)
            {
                ref var det = ref detections[i];
                var info = new DetectionInfo(ref det, 1, 128, 128, 128, 128);
                using (var pose = new Pose(ref info))
                {
                    Debug.Log($"{det.ID} {pose.t.AsFloat3()} {pose.R.AsFloat3x3()}");
                }
            }
        }

        detector.RemoveFamily(family);
    }
}
