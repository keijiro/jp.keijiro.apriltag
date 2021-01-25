using UnityEngine;
using AprilTag;

sealed class Test : MonoBehaviour
{
    [SerializeField] Texture2D _image = null;

    void Start ()
    {
        using var detector = Detector.Create();
        detector.QuadDecimate = 4;
        detector.Debug = true;
        Debug.Log(detector.QuadDecimate);

        using var family = Family.CreateTagStandard41h12();
        detector.AddFamily(family);

        using var image = ImageU8.Create(256, 256);
        image.SetImage(_image.GetRawTextureData<byte>());

        using (var detections = detector.Detect(image))
            Debug.Log(detections.Length);

        detector.RemoveFamily(family);
    }
}
