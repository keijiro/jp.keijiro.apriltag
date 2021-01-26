using AprilTag;
using Pose = AprilTag.Pose;
using System.Collections.Generic;
using UnityEngine;

sealed class WebcamTest : MonoBehaviour
{
    [SerializeField, HideInInspector] Shader _visualizerShader = null;

    WebCamTexture _webcamRaw;
    RenderTexture _webcamBuffer;
    Color32 [] _readBuffer;

    Material _visualizer;

    Detector _detector;
    Family _family;
    ImageU8 _image;

    List<Vector2> _vertices;

    Vector2 NormalizeVertex((double x, double y) position)
      => new Vector2(0 + (float)(position.x / _webcamRaw.width),
                     1 - (float)(position.y / _webcamRaw.height));

    void Start()
    {
        _webcamRaw = new WebCamTexture(1280, 720, 60);
        _webcamRaw.Play();

        _webcamBuffer = new RenderTexture(1280, 720, 0);

        _readBuffer = new Color32 [_webcamRaw.width * _webcamRaw.height];

        _visualizer = new Material(_visualizerShader);

        _detector = Detector.Create();
        _family = Family.CreateTagStandard41h12();

        _detector.AddFamily(_family);

        _image = ImageU8.Create(_webcamRaw.width, _webcamRaw.height);

        _vertices = new List<Vector2>();
    }

    void OnDestroy()
    {
        if (_webcamRaw != null) Destroy(_webcamRaw);
        if (_webcamBuffer != null) Destroy(_webcamBuffer);
        if (_visualizer != null) Destroy(_visualizer);

        if (_detector != null && _family != null)
            _detector.RemoveFamily(_family);

        _detector?.Dispose();
        _family?.Dispose();
        _image?.Dispose();
    }

    void Update()
    {
        _webcamRaw.GetPixels32(_readBuffer);
        ImageUtil.Convert(_readBuffer, _image);

        Graphics.Blit(_webcamRaw, _webcamBuffer);

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
        _visualizer.SetPass(0);
        _visualizer.SetTexture("_CameraFeed", _webcamBuffer);
        Graphics.DrawProceduralNow(MeshTopology.Quads, 4, 1);

        for (var i = 0; i < _vertices.Count; i += 4)
        {
            _visualizer.SetPass(1);
            _visualizer.SetVector("_Corner1", _vertices[i + 0]);
            _visualizer.SetVector("_Corner2", _vertices[i + 1]);
            _visualizer.SetVector("_Corner3", _vertices[i + 2]);
            _visualizer.SetVector("_Corner4", _vertices[i + 3]);
            Graphics.DrawProceduralNow(MeshTopology.Quads, 4, 1);
        }
    }
}
