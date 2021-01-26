using AprilTag;
using Pose = AprilTag.Pose;
using System.Collections.Generic;
using UnityEngine;

sealed class WebcamTest : MonoBehaviour
{
    [SerializeField, HideInInspector] Shader _visualizerShader = null;

    WebCamTexture _webcam;
    Color32 [] _readBuffer;

    Material _visualizer;

    Detector _detector;
    Family _family;
    ImageU8 _image;

    List<Vector2> _vertices;

    Vector2 NormalizeVertex((double x, double y) position)
      => new Vector2((float)(position.x / _webcam.width),
                     (float)(position.y / _webcam.height));

    void Start()
    {
        _webcam = new WebCamTexture(1280, 720, 60);
        _webcam.Play();

        _readBuffer = new Color32 [_webcam.width * _webcam.height];

        _visualizer = new Material(_visualizerShader);

        _detector = Detector.Create();
        _family = Family.CreateTagStandard41h12();

        _detector.AddFamily(_family);

        _image = ImageU8.Create(_webcam.width, _webcam.height);

        _vertices = new List<Vector2>();
    }

    void OnDestroy()
    {
        if (_webcam != null) Destroy(_webcam);
        if (_visualizer != null) Destroy(_visualizer);

        if (_detector != null && _family != null)
            _detector.RemoveFamily(_family);

        _detector?.Dispose();
        _family?.Dispose();
        _image?.Dispose();
    }

    void Update()
    {
        _webcam.GetPixels32(_readBuffer);
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
        _visualizer.SetPass(0);
        _visualizer.SetTexture("_CameraFeed", _webcam);
        Graphics.DrawProceduralNow(MeshTopology.Quads, 4, 1);

        for (var i = 0; i < _vertices.Count; i += 4)
        {
            _visualizer.SetPass(1);
            _visualizer.SetVector("_Corner1", _vertices[i + 0]);
            _visualizer.SetVector("_Corner2", _vertices[i + 1]);
            _visualizer.SetVector("_Corner3", _vertices[i + 2]);
            _visualizer.SetVector("_Corner4", _vertices[i + 3]);
            Graphics.DrawProceduralNow(MeshTopology.LineStrip, 5, 1);
        }
    }
}
