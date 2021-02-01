using UnityEngine;

sealed class TagDrawer : System.IDisposable
{
    Mesh _mesh;
    Material _sharedMaterial;

    public TagDrawer(Material material)
    {
        _mesh = BuildMesh();
        _sharedMaterial = material;
    }

    public void Dispose()
    {
        Object.Destroy(_mesh);
        _mesh = null;
        _sharedMaterial = null;
    }

    public void Draw(int id, Vector3 position, Quaternion rotation, float scale)
    {
        var xform = Matrix4x4.TRS(position, rotation, Vector3.one * scale);
        Graphics.DrawMesh(_mesh, xform, _sharedMaterial, 0);
    }

    static Mesh BuildMesh()
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
