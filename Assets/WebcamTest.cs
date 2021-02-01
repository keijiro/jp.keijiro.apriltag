using UnityEngine;
using System.Linq;
using UI = UnityEngine.UI;

sealed class WebcamTest : MonoBehaviour
{
    [SerializeField] Vector2Int _resolution = new Vector2Int(1280, 720);
    [SerializeField] int _decimation = 4;
    [SerializeField] float _tagSize = 0.05f;
    [SerializeField] Material _tagMaterial = null;
    [SerializeField] UI.RawImage _webcamPreview = null;
    [SerializeField] UI.Text _debugText = null;

    // Webcam input and buffer
    WebCamTexture _webcamRaw;
    RenderTexture _webcamBuffer;
    Color32 [] _readBuffer;

    // AprilTag detector and drawer
    AprilTag.TagDetector _detector;
    TagDrawer _drawer;

    void Start()
    {
        // Webcam initialization
        _webcamRaw = new WebCamTexture(_resolution.x, _resolution.y, 60);
        _webcamBuffer = new RenderTexture(_resolution.x, _resolution.y, 0);
        _readBuffer = new Color32 [_resolution.x * _resolution.y];

        _webcamRaw.Play();
        _webcamPreview.texture = _webcamBuffer;

        // Detector and drawer
        _detector = new AprilTag.TagDetector(_resolution.x, _resolution.y, _decimation);
        _drawer = new TagDrawer(_tagMaterial);
    }

    void OnDestroy()
    {
        Destroy(_webcamRaw);
        Destroy(_webcamBuffer);

        _detector.Dispose();
        _drawer.Dispose();
    }

    void Update()
    {
        // Check if the webcam is ready (needed for macOS support)
        if (_webcamRaw.width <= 16) return;

        // Check if the webcam is flipped (needed for iOS support)
        if (_webcamRaw.videoVerticallyMirrored)
            _webcamPreview.transform.localScale = new Vector3(1, -1, 1);

        // Webcam image buffering
        _webcamRaw.GetPixels32(_readBuffer);
        Graphics.Blit(_webcamRaw, _webcamBuffer);

        // AprilTag detection
        var fov = GetComponent<Camera>().fieldOfView * Mathf.Deg2Rad;
        _detector.ProcessImage(_readBuffer, fov, _tagSize);

        // Detected tag visualization
        foreach (var tag in _detector.DetectedTags)
            _drawer.Draw(tag.ID, tag.Position, tag.Rotation, _tagSize);

        // Profile data output (with 30 frame interval)
        if (Time.frameCount % 30 == 0)
            _debugText.text = _detector.ProfileData.Aggregate
              ("Profile (usec)", (c, n) => $"{c}\n{n.name} : {n.time}");
    }
}
