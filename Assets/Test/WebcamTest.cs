using AprilTag;
using Pose = AprilTag.Pose;
using System.Collections.Generic;
using UnityEngine;

sealed class WebcamTest : MonoBehaviour
{
    [SerializeField, HideInInspector] Shader _visualizerShader = null;

    const int Width = 1280;
    const int Height = 720;

    WebCamTexture _webcamRaw;
    RenderTexture _webcamBuffer;
    Color32 [] _readBuffer;

    Detector _detector;
    Family _family;
    ImageU8 _image;

    List<Vector2> _vertices;
    Material _visualizer;

    Vector2 NormalizeVertex((double x, double y) p)
      => new Vector2(0 + (float)p.x / Width, 1 - (float)p.y / Height);

    void Start()
    {
        _webcamRaw = new WebCamTexture(Width, Height, 60);
        _webcamBuffer = new RenderTexture(Width, Height, 0);
        _readBuffer = new Color32 [Width * Height];

        _webcamRaw.Play();

        _detector = Detector.Create();
        _family = Family.CreateTagStandard41h12();
        _image = ImageU8.Create(Width, Height);

        _detector.AddFamily(_family);

        _vertices = new List<Vector2>();
        _visualizer = new Material(_visualizerShader);
    }

    void OnDestroy()
    {
        if (_webcamRaw != null) Destroy(_webcamRaw);
        if (_webcamBuffer != null) Destroy(_webcamBuffer);

        if (_detector != null && _family != null)
            _detector.RemoveFamily(_family);

        _detector?.Dispose();
        _family?.Dispose();
        _image?.Dispose();

        if (_visualizer != null) Destroy(_visualizer);
    }

    void Update()
    {
        Graphics.Blit(_webcamRaw, _webcamBuffer);

        _webcamRaw.GetPixels32(_readBuffer);
        ImageUtil.Convert(_readBuffer, _image);

        _vertices.Clear();

        using (var detections = _detector.Detect(_image))
        {
            for (var i = 0; i < detections.Length; i++)
            {
                ref var det = ref detections[i];
                _vertices.Add(NormalizeVertex(det.Corner1));
                _vertices.Add(NormalizeVertex(det.Corner2));
                _vertices.Add(NormalizeVertex(det.Corner3));
                _vertices.Add(NormalizeVertex(det.Corner4));
            }
        }
    }

    void OnPostRender()
    {
        _visualizer.SetTexture("_CameraFeed", _webcamBuffer);
        _visualizer.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Quads, 4, 1);

        for (var i = 0; i < _vertices.Count; i += 4)
        {
            _visualizer.SetVector("_Corner1", _vertices[i + 0]);
            _visualizer.SetVector("_Corner2", _vertices[i + 1]);
            _visualizer.SetVector("_Corner3", _vertices[i + 2]);
            _visualizer.SetVector("_Corner4", _vertices[i + 3]);
            _visualizer.SetPass(1);
            Graphics.DrawProceduralNow(MeshTopology.Quads, 4, 1);
        }
    }
}
