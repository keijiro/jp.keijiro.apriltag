using Unity.Mathematics;
using System.Collections.Generic;

struct TagPose
{
    public readonly int id;
    public readonly UnityEngine.Vector3 position;
    public readonly UnityEngine.Quaternion rotation;

    public TagPose
      (int id, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
    {
        this.id = id;
        this.position = position;
        this.rotation = rotation;
    }
}

sealed class BulkDetector : System.IDisposable
{
    AprilTag.Detector _detector;
    AprilTag.Family _family;
    AprilTag.ImageU8 _image;
    List<TagPose> _detectedTags = new List<TagPose>();

    public IEnumerable<TagPose> DetectedTags
      => _detectedTags;

    public BulkDetector(int width, int height, int threadCount)
    {
        _detector = AprilTag.Detector.Create();
        _family = AprilTag.Family.CreateTagStandard41h12();
        _image = AprilTag.ImageU8.Create(width, height);

        _detector.ThreadCount = threadCount;
        _detector.QuadDecimate = 4;
        _detector.AddFamily(_family);
    }

    public void Dispose()
    {
        _detector?.RemoveFamily(_family);
        _detector?.Dispose();
        _family?.Dispose();
        _image?.Dispose();

        _detector = null;
        _family = null;
        _image = null;
    }

    public void DetectTags
      (UnityEngine.Color32[] image, float fov, float tagSize)
    {
        ImageUtil.Convert(image, _image);

        using var detections = _detector.Detect(_image);

        var width = _image.Width;
        var height = _image.Height;
        var flen = height * 0.5 / System.Math.Tan(fov * 0.5);

        _detectedTags.Clear();

        for (var i = 0; i < detections.Length; i++)
        {
            ref var det = ref detections[i];

            var info = new AprilTag.DetectionInfo
              (ref det, tagSize, flen, flen, width * 0.5, height * 0.5);

            using var pose = new AprilTag.Pose(ref info);

            var p = pose.t.AsFloat3() * math.float3(1, -1, 1);

            var r = math.quaternion(pose.R.AsFloat3x3());
            r = r.value * math.float4(-1, 1, -1, 1);

            _detectedTags.Add(new TagPose(det.ID, p, r));
        }
    }
}
