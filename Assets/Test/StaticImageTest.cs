using UnityEngine;
using AprilTag;
using Pose = AprilTag.Pose;

sealed class StaticImageTest : MonoBehaviour
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
