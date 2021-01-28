using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

//
// Job struct that wraps AprilTag pose estimator
//
struct PoseEstimationJob : Unity.Jobs.IJobParallelFor
{
    // Input data struct that simply wraps pointers to tag detection data
    public struct Input
    {
        unsafe AprilTag.Detection* p;

        unsafe public Input(ref AprilTag.Detection r)
          => p = (AprilTag.Detection*)Unsafe.AsPointer(ref r);

        unsafe public ref AprilTag.Detection Ref
          => ref Unsafe.AsRef<AprilTag.Detection>(p);
    }

    // I/O
    [ReadOnly] public NativeArray<Input> input;
    [WriteOnly] public NativeArray<TagPose> output;

    // Camera parameters
    public double tagSize;
    public double focalLength;
    public double2 focalCenter;

    // Job execution method
    public void Execute(int i)
    {
        var info = new AprilTag.DetectionInfo(ref input[i].Ref, tagSize,
           focalLength, focalLength, focalCenter.x, focalCenter.y);

        using var pose = new AprilTag.Pose(ref info);

        var pos = pose.t.AsFloat3() * math.float3(1, -1, 1);

        var rot = math.quaternion(pose.R.AsFloat3x3());
        rot = rot.value * math.float4(-1, 1, -1, 1);

        output[i] = new TagPose(input[i].Ref.ID, pos, rot);
    }
}
