using UnityEngine;
using System.Linq;

sealed class WebcamTest : MonoBehaviour
{
    [SerializeField] float _tagSize = 0.05f;
    [SerializeField] Material _tagMaterial = null;
    [SerializeField] UnityEngine.UI.RawImage _webcamPreview = null;
    [SerializeField] UnityEngine.UI.Text _debugText = null;

    const int Width = 1280;
    const int Height = 720;

    TagDetector _detector;
    WebCamTexture _webcamRaw;
    RenderTexture _webcamBuffer;
    Color32 [] _readBuffer;
    Mesh _tagMesh;

    void Start()
    {
        _detector = new TagDetector(Width, Height);
        _webcamRaw = new WebCamTexture(Width, Height, 60);
        _webcamBuffer = new RenderTexture(Width, Height, 0);
        _readBuffer = new Color32 [Width * Height];
        _tagMesh = MeshUtil.BuildTagMesh();

        _webcamRaw.Play();
        _webcamPreview.texture = _webcamBuffer;
    }

    void OnDestroy()
    {
        _detector.Dispose();
        Destroy(_webcamRaw);
        Destroy(_webcamBuffer);
        Destroy(_tagMesh);
    }

    void Update()
    {
        if (_webcamRaw.width <= 16) return; // macOS

        if (_webcamRaw.videoVerticallyMirrored)
            _webcamPreview.transform.localScale = new Vector3(1, -1, 1);

        _webcamRaw.GetPixels32(_readBuffer);
        Graphics.Blit(_webcamRaw, _webcamBuffer);

        var fov = GetComponent<Camera>().fieldOfView * Mathf.Deg2Rad;
        _detector.DetectTags(_readBuffer, fov, _tagSize);

        var tagScale = Vector3.one * _tagSize;
        foreach (var tag in _detector.DetectedTags)
        {
            var mtx = Matrix4x4.TRS(tag.position, tag.rotation, tagScale);
            Graphics.DrawMesh(_tagMesh, mtx, _tagMaterial, gameObject.layer);
        }

        if (Time.frameCount % 30 == 0)
            _debugText.text = _detector.ProfileData.Aggregate
              ("Profile (usec)", (c, n) => $"{c}\n{n.name} : {n.time}");
    }
}
