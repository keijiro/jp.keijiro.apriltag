using UnityEngine;
using AprilTag;

sealed class Test : MonoBehaviour
{
    void Start ()
    {
        using var detector = Detector.Create();
        using var family = Family.CreateTagStandard41h12();

        detector.AddFamily(family);

        Debug.Log("Hello");

        detector.RemoveFamily(family);
    }
}
