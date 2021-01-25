using UnityEngine;
using AprilTag;

sealed class Test : MonoBehaviour
{
    void Start ()
    {
        using var detector = Detector.Create();
        using var family = Family.CreateTagStandard41h12();
        using var image = ImageU8.Create(256, 256);

        detector.AddFamily(family);

        using (var detections = detector.Detect(image))
        {
            Debug.Log("HELLO");
        }

        detector.RemoveFamily(family);
    }
}
