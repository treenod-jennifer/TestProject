using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMeshComp : MonoBehaviour {
    
    public Mesh _mesh = null;
    public Rect _rect;
    public Collider _collider = null;
    public MeshCollider _meshCollider = null;

    public BlockMeshComp MakeMesh(Rect rect)
    {
        _mesh = new Mesh();
        _rect = rect;

        Vector3[] vertices = new Vector3[4];
        int[] triangles = new int[6];
        Vector2[] uvs = new Vector2[4];

        vertices[0] = new Vector3(_rect.x,                  0f, _rect.y);
        vertices[1] = new Vector3(_rect.x + _rect.width,    0f, _rect.y);
        vertices[2] = new Vector3(_rect.x,                  0f, _rect.y + _rect.height);
        vertices[3] = new Vector3(_rect.x + _rect.width,    0f, _rect.y + _rect.height);

        triangles[0] = 0;
        triangles[1] = 3;
        triangles[2] = 1;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

       
        _mesh.vertices  = vertices;
        _mesh.triangles = triangles;
        _meshCollider   = gameObject.AddComponent<MeshCollider>();
        _meshCollider.sharedMesh = _mesh;
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();

        return this;
    }

}
