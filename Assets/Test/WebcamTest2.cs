using AprilTag;
using Pose = AprilTag.Pose;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

sealed class WebcamTest2 : MonoBehaviour
{
    [SerializeField] float _tagSize = 0.05f;
    [SerializeField] GameObject _prefab = null;
    [SerializeField, HideInInspector] Shader _visualizerShader = null;

    const int Width = 1280;
    const int Height = 720;

    WebCamTexture _webcamRaw;
    RenderTexture _webcamBuffer;
    Color32 [] _readBuffer;

    Detector _detector;
    Family _family;
    ImageU8 _image;

    Material _visualizer;

    List<GameObject> _tags = new List<GameObject>();

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

        using (var detections = _detector.Detect(_image))
        {
            for (var i = 0; i < detections.Length; i++)
            {
                ref var det = ref detections[i];

                var fov = GetComponent<Camera>().fieldOfView * Mathf.Deg2Rad;
                var fl = Height / (2 * Mathf.Tan(fov / 2));

                var info = new DetectionInfo
                  (ref det, _tagSize, fl, fl, Width / 2.0f, Height / 2.0f);

                using (var pose = new Pose(ref info))
                {
                    if (_tags.Count <= i) _tags.Add(Instantiate(_prefab));
                    var r = math.quaternion(pose.R.AsFloat3x3());
                    r = r.value * math.float4(-1, 1, -1, 1);
                    _tags[i].transform.localPosition = pose.t.AsFloat3() * math.float3(1, -1, 1);
                    _tags[i].transform.localRotation = r;
                }
            }
        }
    }

    void OnPostRender()
    {
        _visualizer.SetTexture("_CameraFeed", _webcamBuffer);
        _visualizer.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Quads, 4, 1);
    }
}
