using Unity.Collections;
using Unity.Jobs;
using System;
using System.Collections.Generic;

using Color32 = UnityEngine.Color32;

namespace AprilTag {

//
// Multithreaded tag detector and pose estimator
//
public sealed class TagDetector : System.IDisposable
{
    #region Public properties

    public IEnumerable<TagPose> DetectedTags
      => _detectedTags;

    public IEnumerable<(string name, long time)> ProfileData
      => _profileData ?? (_profileData = GenerateProfileData());

    #endregion

    #region Constructor

    public TagDetector(int width, int height, int decimation = 2)
    {
        // Object creation
        _detector = Interop.Detector.Create();
        _family = Interop.Family.CreateTagStandard41h12();
        _image = Interop.ImageU8.Create(width, height);

        // Detector configuration
        _detector.ThreadCount = SystemConfig.PreferredThreadCount;
        _detector.QuadDecimate = decimation;
        _detector.AddFamily(_family);
    }
    /// <summary>
    /// Initialized the Detector
    /// </summary>
    /// <param name="width">input width</param>
    /// <param name="height">input height</param>
    /// <param name="decimation">detection of quads can be done on a lower-resolution image, improving speed at a cost of pose accuracy and a slight decrease in detection rate. Decoding the binary payload is still done at full resolution.</param>
    /// <param name="decodeSharpening">How much sharpening should be done to decoded images? This can help decode small tags but may or may not help in odd lighting conditions or low light conditions</param>
    /// <param name="quadSigma">What Gaussian blur should be applied to the segmented image (used for quad detection?) Parameter is the standard deviation in pixels. Very noisy images benefit from non-zero values (e.g. 0.8).</param>
    /// <param name="refineEdges">When non-zero, the edges of the each quad are adjusted to "snap to" strong gradients nearby. This is useful when decimation is employed, as it can increase the quality of the initial quad estimate substantially. Generally recommended to be on (1). Very computationally inexpensive.</param>
    public TagDetector(int width, int height,  int decimation = 1,float decodeSharpening = 0.25f, float quadSigma=0, int refineEdges =1)
    {
        // Object creation
        _detector = Interop.Detector.Create();
        _family = Interop.Family.CreateTagStandard41h12();
        _image = Interop.ImageU8.Create(width, height);
   
        // Detector configuration
        _detector.DecodeSharpening = decodeSharpening;
        _detector.RefineEdges = refineEdges;
        _detector.QuadSigma = quadSigma;
        _detector.ThreadCount = SystemConfig.PreferredThreadCount;
        _detector.QuadDecimate = decimation;
        _detector.AddFamily(_family);
    }
    #endregion

    #region Public methods

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

    public void ProcessImage
      (ReadOnlySpan<Color32> image, float fov, float tagSize)
    {
        ImageConverter.Convert(image, _image);
        RunDetectorAndEstimator(fov, tagSize);
    }

    public void ProcessImage(Color32[] image, float focalLength, float focalCenterX, float focalCenterY, float tagSize)
    {
        ImageConverter.Convert(image, _image);
        RunDetectorAndEstimator(focalLength, focalCenterX, focalCenterY, tagSize);
    }

    public void ProcessImage(Color32[] image, float focalLengthX, float focalLengthY, float focalCenterX, float focalCenterY, float tagSize)
    {
        ImageConverter.Convert(image, _image);
        RunDetectorAndEstimator(focalLengthX, focalLengthY, focalCenterX, focalCenterY, tagSize);
    }

    public void ProcessFlippedImage(Color32[] image, float fov, float tagSize)
    {
        ImageConverter.ConvertFlipped(image, _image);
        RunDetectorAndEstimator(fov, tagSize);
    }

    public void ProcessFlippedImage(Color32[] image, float focalLength, float focalCenterX, float focalCenterY, float tagSize)
    {
        ImageConverter.ConvertFlipped(image, _image);
        RunDetectorAndEstimator(focalLength, focalCenterX, focalCenterY, tagSize);
    }

    public void ProcessFlippedImage(Color32[] image, float focalLengthX, float focalLengthY, float focalCenterX, float focalCenterY, float tagSize)
    {
        ImageConverter.ConvertFlipped(image, _image);
        RunDetectorAndEstimator(focalLengthX, focalLengthY, focalCenterX, focalCenterY, tagSize);
    }
    #endregion

    #region Private objects

    Interop.Detector _detector;
    Interop.Family _family;
    Interop.ImageU8 _image;

    List<TagPose> _detectedTags = new List<TagPose>();
    List<(string, long)> _profileData;

    #endregion

    #region Detection/estimation procedure

    //
    // We can simply use the multithreaded AprilTag detector for tag detection.
    //
    // In contrast, AprilTag only provides single-threaded pose estimator, so
    // we have to manage threading ourselves.
    //
    // We don't want to spawn extra threads just for it, so we run them on
    // Unity's job system. It's a bit complicated due to "impedance mismatch"
    // things (unmanaged vs managed vs Unity DOTS).
    //
    void RunDetectorAndEstimator(float fov, float tagSize)
    {
        _profileData = null;

        // Run the AprilTag detector.
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
          (jobInput, jobOutput, _image.Width, _image.Height, fov, tagSize);

        // Run and wait the jobs.
        job.Schedule(tagCount, 1, default(JobHandle)).Complete();

        // Job output -> managed list
        jobOutput.CopyTo(_detectedTags);
    }


    //
    // We can simply use the multithreaded AprilTag detector for tag detection.
    //
    // In contrast, AprilTag only provides single-threaded pose estimator, so
    // we have to manage threading ourselves.
    //
    // We don't want to spawn extra threads just for it, so we run them on
    // Unity's job system. It's a bit complicated due to "impedance mismatch"
    // things (unmanaged vs managed vs Unity DOTS).
    //
    void RunDetectorAndEstimator(float fov, float focalCenterX, float focalCenterY, float tagSize)
    {
        _profileData = null;

        // Run the AprilTag detector.
        using var tags = _detector.Detect(_image);
        var tagCount = tags.Length;

        // Convert the detector output into a NativeArray to make them
        // accessible from the pose estimation job.
        using var jobInput = new NativeArray<PoseEstimationJob.Input>(tagCount, Allocator.TempJob);

        var slice = new NativeSlice<PoseEstimationJob.Input>(jobInput);

        for (var i = 0; i < tagCount; i++)
            slice[i] = new PoseEstimationJob.Input(ref tags[i]);

        // Pose estimation output buffer
        using var jobOutput
            = new NativeArray<TagPose>(tagCount, Allocator.TempJob);

        // Pose estimation job
        var job = new PoseEstimationJob(jobInput, jobOutput, _image.Height, fov, focalCenterX, focalCenterY, tagSize);

        // Run and wait the jobs.
        job.Schedule(tagCount, 1, default(JobHandle)).Complete();

        // Job output -> managed list
        jobOutput.CopyTo(_detectedTags);
    }

    void RunDetectorAndEstimator(float focalLengthX, float focalLengthY, float focalCenterX, float focalCenterY, float tagSize)
    {
        _profileData = null;

        // Run the AprilTag detector.
        using var tags = _detector.Detect(_image);
        var tagCount = tags.Length;

        // Convert the detector output into a NativeArray to make them
        // accessible from the pose estimation job.
        using var jobInput = new NativeArray<PoseEstimationJob.Input>(tagCount, Allocator.TempJob);

        var slice = new NativeSlice<PoseEstimationJob.Input>(jobInput);

        for (var i = 0; i < tagCount; i++)
            slice[i] = new PoseEstimationJob.Input(ref tags[i]);

        // Pose estimation output buffer
        using var jobOutput
            = new NativeArray<TagPose>(tagCount, Allocator.TempJob);

        // Pose estimation job
        var job = new PoseEstimationJob(jobInput, jobOutput, focalLengthX, focalLengthY, focalCenterX, focalCenterY, tagSize);

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

} // namespace AprilTag
