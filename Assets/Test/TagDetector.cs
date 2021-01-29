using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;

//
// Multithreaded tag detection and pose estimation
//
// We can simply use the multithreaded AprilTag detector for tag detection. In
// contrast, AprilTag only provides single-threaded pose estimator, so we have
// to manage threading ourselves.
//
// We don't want to spawn extra threads just for it, so we run them on Unity's
// job system. It's a bit complicated due to "impedance mismatch" things
// (unmanaged vs managed vs Unity DOTS).
//
sealed class TagDetector : System.IDisposable
{
    #region AprilTag objects (unmanaged resources)

    AprilTag.Detector _detector;
    AprilTag.Family _family;
    AprilTag.ImageU8 _image;

    #endregion

    #region Internal data

    List<TagPose> _detectedTags = new List<TagPose>();
    List<(string, long)> _profileData;

    #endregion

    #region Object lifecycle

    public TagDetector(int width, int height)
    {
        // Object creation
        _detector = AprilTag.Detector.Create();
        _family = AprilTag.Family.CreateTagStandard41h12();
        _image = AprilTag.ImageU8.Create(width, height);

        // Detector configuration
        _detector.ThreadCount = SystemConfig.PreferredThreadCount;
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

    #endregion

    #region Public properties and methods

    public IEnumerable<TagPose> DetectedTags
      => _detectedTags;

    public IEnumerable<(string name, long time)> ProfileData
      => _profileData ?? (_profileData = GenerateProfileData());

    public void DetectTags
      (UnityEngine.Color32[] image, float fov, float tagSize)
    {
        _profileData = null;

        // Run the AprilTag detector.
        ImageUtil.Convert(image, _image);
        using var tags = _detector.Detect(_image);
        var tagCount = tags.Length;

        // Convert the detector output into a NativeArray to make them
        // accessible from the pose estimation job.
        using var jobInput = new NativeArray<PoseEstimationJob.Input>
          (tagCount, Allocator.TempJob);

        var slice = new NativeSlice<PoseEstimationJob.Input>(jobInput);

        for (var i = 0; i < tagCount; i++)
            slice[i] = new PoseEstimationJob.Input(ref tags[i]);

        // Pose estimation output buffer
        using var jobOutput
          = new NativeArray<TagPose>(tagCount, Allocator.TempJob);

        // Pose estimation job
        var job = new PoseEstimationJob
          { input = jobInput,
            tagSize = tagSize,
            focalCenter = math.double2(_image.Width, _image.Height) / 2,
            focalLength = _image.Height / 2 / System.Math.Tan(fov / 2),
            output = jobOutput };

        // Run and wait the jobs.
        job.Schedule(tagCount, 1, default(JobHandle)).Complete();

        // Job output -> managed list
        jobOutput.CopyTo(_detectedTags);
    }

    #endregion

    #region Profile data aggregation

    List<(string, long)> GenerateProfileData()
    {
        var list = new List<(string, long)>();
        var stamps = _detector.TimeProfile.Stamps;
        var time = _detector.TimeProfile.UTime;
        for (var i = 0; i < stamps.Length; i++)
        {
            var stamp = stamps[i];
            list.Add((stamp.Name, stamp.UTime - time));
            time = stamp.UTime;
        }
        return list;
    }

    #endregion
}
