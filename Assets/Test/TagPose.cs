using UnityEngine;

public struct TagPose
{
    public readonly int id;
    public readonly Vector3 position;
    public readonly Quaternion rotation;

    public TagPose(int id, Vector3 position, Quaternion rotation)
    {
        this.id = id;
        this.position = position;
        this.rotation = rotation;
    }
}
