using AprilTag;
using UnityEngine;
using Unity.Mathematics;

using Pose = AprilTag.Pose;
using UI = UnityEngine.UI;

sealed class WebcamTest : MonoBehaviour
{
    [SerializeField] float _tagSize = 0.05f;
    [SerializeField] Material _tagMaterial = null;
    [SerializeField] UI.RawImage _webcamPreview = null;

    const int Width = 1280;
    const int Height = 720;

    WebCamTexture _webcamRaw;
    RenderTexture _webcamBuffer;
    Color32 [] _readBuffer;
    Mesh _cubeMesh;

    Detector _detector;
    Family _family;
    ImageU8 _image;

    void Start()
    {
        _webcamRaw = new WebCamTexture(Width, Height, 60);
        _webcamBuffer = new RenderTexture(Width, Height, 0);
        _readBuffer = new Color32 [Width * Height];
        _cubeMesh = BuildCubeMesh();

        _webcamRaw.Play();
        _webcamPreview.texture = _webcamBuffer;

        _detector = Detector.Create();
        _family = Family.CreateTagStandard41h12();
        _image = ImageU8.Create(Width, Height);

        _detector.AddFamily(_family);
    }

    void OnDestroy()
    {
        Destroy(_webcamRaw);
        Destroy(_webcamBuffer);
        Destroy(_cubeMesh);

        if (_detector != null && _family != null)
            _detector.RemoveFamily(_family);

        _detector?.Dispose();
        _family?.Dispose();
        _image?.Dispose();
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
                    var r = math.quaternion(pose.R.AsFloat3x3());
                    r = r.value * math.float4(-1, 1, -1, 1);

                    var mtx = Matrix4x4.TRS(pose.t.AsFloat3() * math.float3(1, -1, 1), r, Vector3.one * _tagSize);
                    Graphics.DrawMesh(_cubeMesh, mtx, _tagMaterial, 0);
                }
            }
        }
    }

    public static Mesh BuildCubeMesh()
    {
        var vtx = new Vector3 [] { new Vector3(-0.5f, -0.5f, 0),
                                   new Vector3(+0.5f, -0.5f, 0),
                                   new Vector3(+0.5f, +0.5f, 0),
                                   new Vector3(-0.5f, +0.5f, 0),
                                   new Vector3(-0.5f, -0.5f, -1),
                                   new Vector3(+0.5f, -0.5f, -1),
                                   new Vector3(+0.5f, +0.5f, -1),
                                   new Vector3(-0.5f, +0.5f, -1),
                                   new Vector3(-0.2f, 0, 0),
                                   new Vector3(+0.2f, 0, 0),
                                   new Vector3(0, -0.2f, 0),
                                   new Vector3(0, +0.2f, 0),
                                   new Vector3(0, 0, 0),
                                   new Vector3(0, 0, -1.5f) };

        var idx = new int [] { 0, 1, 1, 2, 2, 3, 3, 0,
                               4, 5, 5, 6, 6, 7, 7, 4,
                               0, 4, 1, 5, 2, 6, 3, 7,
                               8, 9, 10, 11, 12, 13 };

        var mesh = new Mesh();
        mesh.vertices = vtx;
        mesh.SetIndices(idx, MeshTopology.Lines, 0);

        return mesh;
    }
}
