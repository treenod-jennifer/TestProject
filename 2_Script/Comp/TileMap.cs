using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TileChunkDataInfo
{
    public byte _byTexture = 0;

    public void Clear()
    {
        _byTexture = 0;
    }

    public void SetID(byte byID)
    {
        byte by = (byte)(_byTexture & 0x1c0);
        _byTexture = (byte)(by | byID);
    }

    public byte GetID()
    {
        byte by = (byte)(_byTexture & 0x1f);
        return by;
    }

    public void SetDirection(byte byDirection)
    {
        byte by = (byte)(_byTexture & 0x1f);
        _byTexture = (byte)(by | (byDirection << 5));
    }

    public byte GetDirection()
    {
        return (byte)(_byTexture >> 5);
    }

    /*
    public byte _byTextureID = 0;
    public byte _byTextureDirection;
     * */
}

public enum eAreaDataTextureType
{
    Full,
    Half,
    ThreeQuarter,
    Quarter,
    Diagonal,
    HalfDiagonal_R,
    HalfDiagonal_L,
    e7,
    e8,
    e9,
    e10,
    e11,
    e12,
    e13,
    e14,
    e15,
    e16,
    e17,
    e18,
    e19,
    e20,
    e21,
    e22,
    None,
    Count,
}

public enum eAreaDataDirection
{
    Top,
    Left,
    Bottom,
    Right,
    TopInv,
    LeftInv,
    BottomInv,
    RightInv,
    Count,
}

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TileMapChunkData : MonoBehaviour
{
    public MeshDataInfo _meshDataInfo;
    public TileChunkDataInfo[] _tileChunkDataInfo;

}

public class TileMap : MonoBehaviour
{
//    private const int _dTILEMAP_VERSION = 0;
    private const int _dTILEMAP_VERSION = 1;    //  Position Index 추가
    public int _nRows;
    public int _nColumns;
    public float _fTileWidthSize = 1;
    public float _fTileHeightSize = 1;
    public Texture _texture;
    public int _nTextureWidthCount = 8;
    public int _nTextureHeightCount = 8;
    public int _nPosIndex_X = 0;
    public int _nPosIndex_Z = 0;

    private int _nTextureID = 0;
    private float _fHalfSizeX = 0;
    private float _fHalfSizeZ = 0;
    public Transform _transform;

    private Dictionary<int, Vector3> _dicVertices;
    private Dictionary<int, int> _dicTriangles;
    private Dictionary<int, Vector2> _dicUVs;




    private Mesh _mesh;
    private MeshFilter _meshFilter;
    public MeshRenderer _meshRenderer = null;
    public Material _material = null;
    private TileChunkDataInfo[] _tileChunkDataInfo;

    private Material _materialBlind;

    private Mesh _meshC = null;
    Vector3[] _verticesC;
    int[] _trianglesC;
    Vector2[] _uvsC;
    public MeshCollider _meshCollider = null;

    [HideInInspector]
    public Vector3 MarkerPosition;

    public TileMap()
    {
        this._nColumns = 50;
        this._nRows = 50;
    }

    public void ChangeTextureID(int nTextureID)
    {
        _nTextureID = nTextureID;
    }

    public void InitData(MeshDataInfo meshDataInfo, int nTextureID, Material materialBlind)
    {
        _transform = transform;
        _nPosIndex_X = meshDataInfo._nPosIndex_X;
        _nPosIndex_Z = meshDataInfo._nPosIndex_Z;
        _transform.localPosition = new Vector3(meshDataInfo._fPosX, 0f, meshDataInfo._fPosZ);
        _transform.parent.localScale = Vector3.one;
        _nColumns = meshDataInfo._nColumns;
        _nRows = meshDataInfo._nRows;
        _fTileWidthSize = meshDataInfo._fTileWidthSize;
        _fTileHeightSize = meshDataInfo._fTileHeightSize;
        _nTextureID = nTextureID;
        _materialBlind = materialBlind;
        MakeMesh(true, true);
    }

    private void OnDrawGizmosSelected()
    {
        return;
        var mapWidth = this._nColumns * this._fTileWidthSize;
        var mapHeight = this._nRows * this._fTileHeightSize;
        var position = this.transform.position - new Vector3(_fHalfSizeX, 0, _fHalfSizeZ);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(position, position + new Vector3(mapWidth, 0, 0));
        Gizmos.DrawLine(position, position + new Vector3(0, 0, mapHeight));
        Gizmos.DrawLine(position + new Vector3(mapWidth, 0, 0), position + new Vector3(mapWidth, 0, mapHeight));
        Gizmos.DrawLine(position + new Vector3(0, 0, mapHeight), position + new Vector3(mapWidth, 0, mapHeight));

        Gizmos.color = Color.grey;
        for (float i = 1; i < this._nColumns; i++)
        {
            Gizmos.DrawLine(position + new Vector3(i * this._fTileWidthSize, 0, 0), position + new Vector3(i * this._fTileWidthSize, 0, mapHeight));
        }

        for (float i = 1; i < this._nRows; i++)
        {
            Gizmos.DrawLine(position + new Vector3(0, 0, i * this._fTileHeightSize), position + new Vector3(mapWidth, 0, i * this._fTileHeightSize));
        }

        // Draw marker position
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(this.MarkerPosition, new Vector3(this._fTileWidthSize, 1, this._fTileHeightSize));
    }

    public void MakeMesh(bool bNew, bool bAll)
    {
        _mesh = new Mesh();
        int nfolygonCount = _nColumns * _nRows;
        int nVerCount = nfolygonCount * 4;
        int nUVCount = nVerCount;
        int nTriangleCount = nfolygonCount * 6;

        _dicVertices = new Dictionary<int, Vector3>();
        _dicTriangles = new Dictionary<int, int>();
        _dicUVs = new Dictionary<int, Vector2>();

        if (bNew == true)
        {
            _tileChunkDataInfo = new TileChunkDataInfo[nfolygonCount];
        }

        /*
        if (bAll == true && ManagerLevelBlindStatic.instance._bEditMode == true)
            MakeColliderMesh();
         * */
        /*
        if(bAll == true)
            MakeColliderMesh();
        */
        for (int i = 0; i < _nRows; i++)
        {
            for (int j = 0; j < _nColumns; j++)
            {
                if (bNew == true)
                {
                    _tileChunkDataInfo[(i * _nColumns) + (j)] = new TileChunkDataInfo();
                    _tileChunkDataInfo[(i * _nColumns) + (j)].Clear();
                    _tileChunkDataInfo[(i * _nColumns) + (j)].SetID((byte)_nTextureID);
                    _tileChunkDataInfo[(i * _nColumns) + (j)].SetDirection((byte)eAreaDataDirection.Top);
                }
                AddTileToMesh(j, i, _tileChunkDataInfo[(i * _nColumns) + (j)].GetID(), bAll);
            }
        }

        _mesh.vertices = _dicVertices.Values.ToArray();
        int[] triangles = _dicTriangles.Values.ToArray();
        triangles = RefreashTriangles(triangles);
        _mesh.triangles = triangles;
        _mesh.uv = _dicUVs.Values.ToArray();
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();

        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter == null)
            _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshFilter.mesh = _mesh;
        //        _material = new Material(Shader.Find("Particles/Alpha Blended"));
        //        Shader shader = Shader.Find("Standard");
//        Shader shader = Shader.Find("Particles/Alpha Blended");
        //        Shader shader = Shader.Find("Mobile/Particles/Alpha Blended");
//        _material = new Material(shader);
        _material = new Material(_materialBlind);
//        _material.SetTexture("_MainTex", _texture);
        _material.renderQueue = 5000;
        /*
        if (ManagerLevelBlindStatic.instance._bEditMode == true && Application.isPlaying == false)
        {
            _meshCollider = GetComponent<MeshCollider>();
            if (_meshCollider == null)
                _meshCollider = gameObject.AddComponent<MeshCollider>();
            _meshCollider.sharedMesh = _meshC;
        }
         * */
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = _material;

        //        _meshRenderer.material.SetColor("_TintColor", new Color(1f, 1f, 1f, 0.5f));
        MakeColliderMesh(true, true);
    }

    public void DeleteColliderMesh()
    {
        if (_meshC != null)
        {
            _verticesC      = null;
            _trianglesC     = null;
            _meshC          = null;
            DestroyImmediate(_meshCollider);
            _meshCollider = null;
        }
    }

    public void MakeColliderMesh(bool bToolEditMode = false, bool bForce = false)
    {
        if (bToolEditMode != true || Application.isPlaying == true)
            return;

        if (bForce == false)
        {
            if (_meshC != null || _mesh == null)
                return;
        }
        else
        {
            if (_mesh == null)
                return;
            if (_meshC != null)
                _meshC = null;
        }

        _meshC = new Mesh();
//        int nVerCount = ;
        int nTriangleCount = _nColumns * _nRows * 6;

//        _verticesC = new Vector3[_nColumns * _nRows * 4];
        _verticesC = new Vector3[(_nColumns + 1) * (_nRows + 1)];
        _trianglesC = new int[nTriangleCount];

        for (int i = 0; i < _nRows + 1; i++)
        {
            for (int j = 0; j < _nColumns + 1; j++)
            {
                _verticesC[(i * (_nColumns + 1)) + j] = new Vector3((j * _fTileWidthSize), 0f, i * _fTileHeightSize);
//                _verticesC[(i * (_nColumns + 1)) + j] = new Vector3((j * _fTileWidthSize) - _fHalfSizeX, 0f, i * _fTileHeightSize - _fHalfSizeZ);
            }
        }

        int nTrCount = 0;
        int nVerCount = 0;
        for (int i = 0; i < _nRows; i++)
        {
            for (int j = 0; j < _nColumns; j++)
            {
                _trianglesC[nTrCount] = j + (i * (_nColumns + 1));               //  0
                nTrCount++;
                _trianglesC[nTrCount] = (j) + ((i + 1) * (_nColumns + 1));       //  3
                nTrCount++;
                _trianglesC[nTrCount] = (j + 1) + ((i + 1) * (_nColumns + 1));   //  4
                nTrCount++;
                _trianglesC[nTrCount] = j + (i * (_nColumns + 1));                   //  0
                nTrCount++;
                _trianglesC[nTrCount] = (j + 1) + ((i + 1) * (_nColumns + 1));   //  4
                nTrCount++;
                _trianglesC[nTrCount] = (j + 1) + ((i) * (_nColumns + 1));           //  1
                nTrCount++;
            }
        }

        _meshC.vertices     = _verticesC;
        _meshC.triangles    = _trianglesC;
        _meshC.RecalculateBounds();
        _meshC.RecalculateNormals();
        _meshCollider = GetComponent<MeshCollider>();
        if (_meshCollider == null)
            _meshCollider = gameObject.AddComponent<MeshCollider>();
        _meshCollider.sharedMesh = _meshC;
    }

    private void RemoveTileToMesh(int nX, int nY, int nTextureID)
    {
        float fUSize = 1 / (float)_nTextureWidthCount;
        float fVSize = 1 / (float)_nTextureHeightCount;
        int nUIndex = nTextureID % _nTextureWidthCount;
        int nVIndex = nTextureID / _nTextureWidthCount;

        for (int i = 0; i < 4; i++)
        {
            if (_dicVertices.ContainsKey((nY * _nColumns * 4) + (nX * 4) + i) == true)
                _dicVertices.Remove((nY * _nColumns * 4) + (nX * 4) + i);
        }

        for (int i = 0; i < 6; i++)
        {
            if (_dicTriangles.ContainsKey((nY * _nColumns * 6) + (nX * 6) + i) == true)
                _dicTriangles.Remove((nY * _nColumns * 6) + (nX * 6) + i);
        }

        for (int i = 0; i < 4; i++)
        {
            if (_dicUVs.ContainsKey((nY * _nColumns * 4) + (nX * 4) + i) == true)
                _dicUVs.Remove((nY * _nColumns * 4) + (nX * 4) + i);
        }
        _mesh.Clear();
        _mesh.vertices = _dicVertices.Values.ToArray();
        int[] triangles = _dicTriangles.Values.ToArray();
        triangles = RefreashTriangles(triangles);
        _mesh.triangles = triangles;
        _mesh.uv = _dicUVs.Values.ToArray();
    }

    private void AddTileToMesh(int nX, int nY, int nTextureID, bool bAll = false)
    {
        float fUSize = 1 / (float)_nTextureWidthCount;
        float fVSize = 1 / (float)_nTextureHeightCount;
        int nUIndex = nTextureID % _nTextureWidthCount;
        int nVIndex = nTextureID / _nTextureWidthCount;

        if (nTextureID != (int)eAreaDataTextureType.None)
        {
            if (!_dicVertices.ContainsKey((nY * _nColumns * 4) + (nX * 4)))
                _dicVertices.Add((nY * _nColumns * 4) + (nX * 4), new Vector3((nX * _fTileWidthSize), 0f, nY * _fTileHeightSize));
            if (_dicVertices.ContainsKey((nY * _nColumns * 4) + (nX * 4) + 1) == false)
                _dicVertices.Add((nY * _nColumns * 4) + (nX * 4) + 1, new Vector3(((nX + 1) * _fTileWidthSize), 0f, nY * _fTileHeightSize));
            if (_dicVertices.ContainsKey((nY * _nColumns * 4) + (nX * 4) + 2) == false)
                _dicVertices.Add((nY * _nColumns * 4) + (nX * 4) + 2, new Vector3((nX * _fTileWidthSize), 0f, (nY + 1) * _fTileHeightSize));
            if (_dicVertices.ContainsKey((nY * _nColumns * 4) + (nX * 4) + 3) == false)
                _dicVertices.Add((nY * _nColumns * 4) + (nX * 4) + 3, new Vector3(((nX + 1) * _fTileWidthSize), 0f, (nY + 1) * _fTileHeightSize));
            /*
            if (!_dicVertices.ContainsKey((nY * _nColumns * 4) + (nX * 4)))
                _dicVertices.Add((nY * _nColumns * 4) + (nX * 4), new Vector3((nX * _fTileWidthSize) - _fHalfSizeX, 0f, nY * _fTileHeightSize - _fHalfSizeZ));
            if (_dicVertices.ContainsKey((nY * _nColumns * 4) + (nX * 4) + 1) == false)
                _dicVertices.Add((nY * _nColumns * 4) + (nX * 4) + 1, new Vector3(((nX + 1) * _fTileWidthSize) - _fHalfSizeX, 0f, nY * _fTileHeightSize - _fHalfSizeZ));
            if (_dicVertices.ContainsKey((nY * _nColumns * 4) + (nX * 4) + 2) == false)
                _dicVertices.Add((nY * _nColumns * 4) + (nX * 4) + 2, new Vector3((nX * _fTileWidthSize) - _fHalfSizeX, 0f, (nY + 1) * _fTileHeightSize - _fHalfSizeZ));
            if (_dicVertices.ContainsKey((nY * _nColumns * 4) + (nX * 4) + 3) == false)
                _dicVertices.Add((nY * _nColumns * 4) + (nX * 4) + 3, new Vector3(((nX + 1) * _fTileWidthSize) - _fHalfSizeX, 0f, (nY + 1) * _fTileHeightSize - _fHalfSizeZ));
            */
            if (_dicTriangles.ContainsKey((nY * _nColumns * 6) + (nX * 6)) == false)
                _dicTriangles.Add((nY * _nColumns * 6) + (nX * 6), 0 + (nY * _nColumns * 4) + (nX * 4));
            if (_dicTriangles.ContainsKey((nY * _nColumns * 6) + (nX * 6) + 1) == false)
                _dicTriangles.Add((nY * _nColumns * 6) + (nX * 6) + 1, 3 + (nY * _nColumns * 4) + (nX * 4));
            if (_dicTriangles.ContainsKey((nY * _nColumns * 6) + (nX * 6) + 2) == false)
                _dicTriangles.Add((nY * _nColumns * 6) + (nX * 6) + 2, 1 + (nY * _nColumns * 4) + (nX * 4));
            if (_dicTriangles.ContainsKey((nY * _nColumns * 6) + (nX * 6) + 3) == false)
                _dicTriangles.Add((nY * _nColumns * 6) + (nX * 6) + 3, 0 + (nY * _nColumns * 4) + (nX * 4));
            if (_dicTriangles.ContainsKey((nY * _nColumns * 6) + (nX * 6) + 4) == false)
                _dicTriangles.Add((nY * _nColumns * 6) + (nX * 6) + 4, 2 + (nY * _nColumns * 4) + (nX * 4));
            if (_dicTriangles.ContainsKey((nY * _nColumns * 6) + (nX * 6) + 5) == false)
                _dicTriangles.Add((nY * _nColumns * 6) + (nX * 6) + 5, 3 + (nY * _nColumns * 4) + (nX * 4));
            ChangeUVS(nX, nY, fUSize, fVSize, nUIndex, nVIndex, bAll);
        }
    }

    private void ChangeUVS(int nX, int nY, float fUSize, float fVSize, int nUIndex, int nVIndex, bool bAll)
    {
        Vector2[] uvs = new Vector2[4];
        switch (_tileChunkDataInfo[(nY * _nColumns) + (nX)].GetDirection())
        {
            case (byte)eAreaDataDirection.Top:
                {
                    uvs[0] = new Vector2(0.5f / 128f + nUIndex * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);               // 0
                    uvs[1] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);    // 1
                    uvs[2] = new Vector2(0.5f / 128f + nUIndex * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);                // 2
                    uvs[3] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);         // 3
                }
                break;
            case (byte)eAreaDataDirection.Left:
                {
                    uvs[0] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);        // 1
                    uvs[1] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);         // 3
                    uvs[2] = new Vector2(0.5f / 128f + nUIndex * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);           // 0
                    uvs[3] = new Vector2(0.5f / 128f + nUIndex * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);                // 2
                }
                break;
            case (byte)eAreaDataDirection.Bottom:
                {
                    uvs[0] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);             // 3
                    uvs[1] = new Vector2(0.5f / 128f + nUIndex * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);                // 2
                    uvs[2] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);    // 1
                    uvs[3] = new Vector2(0.5f / 128f + nUIndex * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);           // 0
                }
                break;
            case (byte)eAreaDataDirection.Right:
                {
                    uvs[0] = new Vector2(0.5f / 128f + nUIndex * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);                    // 2
                    uvs[1] = new Vector2(0.5f / 128f + nUIndex * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);           // 0
                    uvs[2] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);         // 3
                    uvs[3] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);    // 1
                }
                break;
            case (byte)eAreaDataDirection.TopInv:
                {
                    uvs[0] = new Vector2(0.5f / 128f + nUIndex * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);                    // 2
                    uvs[1] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);         // 3
                    uvs[2] = new Vector2(0.5f / 128f + nUIndex * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);           // 0
                    uvs[3] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);    // 1
                }
                break;
            case (byte)eAreaDataDirection.LeftInv:
                {
                    uvs[0] = new Vector2(0.5f / 128f + nUIndex * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);               // 0
                    uvs[1] = new Vector2(0.5f / 128f + nUIndex * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);                // 2
                    uvs[2] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);    // 1
                    uvs[3] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);         // 3
                }
                break;
            case (byte)eAreaDataDirection.BottomInv:
                {
                    uvs[0] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);        // 1
                    uvs[1] = new Vector2(0.5f / 128f + nUIndex * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);           // 0
                    uvs[2] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);         // 3
                    uvs[3] = new Vector2(0.5f / 128f + nUIndex * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);                // 2
                }
                break;
            case (byte)eAreaDataDirection.RightInv:
                {
                    uvs[0] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);             // 3
                    uvs[1] = new Vector2(-0.5f / 128f + (nUIndex + 1) * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);    // 1
                    uvs[2] = new Vector2(0.5f / 128f + nUIndex * fUSize, (-0.5f / 128f) + 1f - nVIndex * fVSize);                // 2
                    uvs[3] = new Vector2(0.5f / 128f + nUIndex * fUSize, (0.5f / 128f) + 1f - (nVIndex + 1) * fVSize);           // 0
                }
                break;
        }
        for (int i = 0; i < 4; i++)
        {
            if (_dicUVs.ContainsKey((nY * _nColumns * 4) + (nX * 4) + i) == true)
                _dicUVs[(nY * _nColumns * 4) + (nX * 4) + i] = uvs[i];
            else
                _dicUVs.Add((nY * _nColumns * 4) + (nX * 4) + i, uvs[i]);
        }
        if (bAll == false)
        {
            _mesh.Clear();
            _mesh.vertices = _dicVertices.Values.ToArray();
            int[] triangles = _dicTriangles.Values.ToArray();
            triangles = RefreashTriangles(triangles);
            _mesh.triangles = triangles;
            _mesh.uv = _dicUVs.Values.ToArray();
            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
        }

    }

    private int[] RefreashTriangles(int[] triangles)
    {
        int nCount = triangles.Length / 6;
        int[] nNewTriangles = new int[triangles.Length];
        for (int i = 0; i < nCount; i++)
        {
            nNewTriangles[i * 6 + 0] = i * 4 + 0;
            nNewTriangles[i * 6 + 1] = i * 4 + 3;
            nNewTriangles[i * 6 + 2] = i * 4 + 1;
            nNewTriangles[i * 6 + 3] = i * 4 + 0;
            nNewTriangles[i * 6 + 4] = i * 4 + 2;
            nNewTriangles[i * 6 + 5] = i * 4 + 3;
        }
        return nNewTriangles;
    }

    public void ChangeTileTextureToMesh(int nX, int nY)
    {
        _tileChunkDataInfo[(nY * _nColumns) + (nX)].SetID((byte)_nTextureID);
        _tileChunkDataInfo[(nY * _nColumns) + (nX)].SetDirection((byte)eAreaDataDirection.Top);

        if (_nTextureID != (int)eAreaDataTextureType.None)
            AddTileToMesh(nX, nY, _nTextureID);
        else
            RemoveTileToMesh(nX, nY, _nTextureID);
    }

    public void ChangeTileDiretionToMesh(int nX, int nY)
    {
        byte byDirection = _tileChunkDataInfo[(nY * _nColumns) + (nX)].GetDirection();
        byDirection++;
        if (byDirection > (byte)eAreaDataDirection.RightInv)
            byDirection = (byte)eAreaDataDirection.Top;
        _tileChunkDataInfo[(nY * _nColumns) + (nX)].SetDirection(byDirection);
        int textureID = _tileChunkDataInfo[(nY * _nColumns) + (nX)].GetID();
        if (textureID != (int)eAreaDataTextureType.None)
            AddTileToMesh(nX, nY, textureID);
        else
            RemoveTileToMesh(nX, nY, _nTextureID);
    }

    public bool SaveTileChunkDataInfoBinary(string strFileName, MeshDataInfo meshDataInfo)
    {
        bool bRet = false;
        FileStream file = File.Create(strFileName);
        BinaryWriter bw = new BinaryWriter(file);
        bw.BaseStream.Seek(0, SeekOrigin.Begin);
        int nCount = _tileChunkDataInfo.Length;
        if (nCount <= 0)
            return bRet;
        bw.Write(_dTILEMAP_VERSION);
        bw.Write(meshDataInfo._fPosX);
        bw.Write(meshDataInfo._fPosZ);
        bw.Write(meshDataInfo._nRows);
        bw.Write(meshDataInfo._nColumns);
        bw.Write(meshDataInfo._fTileWidthSize);
        bw.Write(meshDataInfo._fTileHeightSize);
        bw.Write(meshDataInfo._nPosIndex_X);
        bw.Write(meshDataInfo._nPosIndex_Z);
        bw.Write(nCount);
        for (int i = 0; i < nCount; i++)
        {
            bw.Write(_tileChunkDataInfo[i]._byTexture);
        }
        file.Close();
        /*       
                GameObject root = new GameObject("temp");
                TileMapChunkData tileMapChunkData = root.AddComponent<TileMapChunkData>();
                tileMapChunkData._meshDataInfo = meshDataInfo;
                tileMapChunkData._tileChunkDataInfo = _tileChunkDataInfo;
        //        GameObject root = new GameObject(file);
                PokoUtil.RemoveString_Assets(ref strFileName, true);
                GameObject prefab = PrefabUtility.CreatePrefab("Assets/" + strFileName + ".prefab", root);
                UnityEngine.Object.DestroyImmediate(root);
         * */
        bRet = true;
        return bRet;
    }

    public bool LoadTileChunkDataInfoBinary(string strFileName, Material materialBlind)
    {
        bool bRet = false;
        //        TextAsset asset = Resources.Load(strFileName, typeof(byte)) as TextAsset;
        //        PokoUtil.RemoveString_Assets(ref strFileName, true);
        //        object obj = Resources.Load(strFileName);
        FileStream file = File.Open(strFileName, FileMode.Open);
        BinaryReader br = new BinaryReader(file);
        //        TextAsset asset = Resources.Load(strFileName) as TextAsset;
        //        TextAsset asset = Resources.Load<TextAsset>(strFileName);
        //        if(asset == null)
        //            return bRet;
        //        Stream s = new MemoryStream(asset.bytes);
        //        BinaryReader br = new BinaryReader(s);
        //            br.BaseStream.Seek(0, SeekOrigin.Begin);
        int nVersion = 0;
        nVersion = br.ReadInt32();
        float fPosX, fPosZ;
        fPosX = br.ReadSingle();
        fPosZ = br.ReadSingle();
        _nRows = br.ReadInt32();
        _nColumns = br.ReadInt32();
        _fTileWidthSize = br.ReadSingle();
        _fTileHeightSize = br.ReadSingle();
        if (nVersion < 1)
        {
            _nPosIndex_X = (int)(fPosX / _fTileWidthSize);
            _nPosIndex_Z = (int)(fPosZ / _fTileHeightSize);
        }
        else
        {
            _nPosIndex_X = br.ReadInt32();
            _nPosIndex_Z = br.ReadInt32();
        }
        /*
        _fHalfSizeX = _nColumns * _fTileWidthSize * 0.5f;
        _fHalfSizeZ = _nRows * _fTileHeightSize * 0.5f;
        */
        _transform = transform;
        _transform.localPosition = new Vector3(fPosX, 0f, fPosZ);
        _transform.parent.localScale = Vector3.one;

        int nCount = br.ReadInt32();
        _tileChunkDataInfo = new TileChunkDataInfo[nCount];
        for (int i = 0; i < nCount; i++)
        {
            _tileChunkDataInfo[i] = new TileChunkDataInfo();
            _tileChunkDataInfo[i]._byTexture = br.ReadByte();
        }
        file.Close();
        _materialBlind = materialBlind;
        MakeMesh(false, true);
        bRet = true;
        return bRet;
    }

    public bool LoadTileChunkDataInfoTextAsset(TextAsset asset, Material materialBlind)
    {
        gameObject.SetActive(true);
        bool bRet = false;
        //        TextAsset asset = Resources.Load<TextAsset>(strFileName);
        if (asset == null)
            return bRet;
        Stream s = new MemoryStream(asset.bytes);
        BinaryReader br = new BinaryReader(s);
        //            br.BaseStream.Seek(0, SeekOrigin.Begin);
        int nVersion = 0;
        nVersion = br.ReadInt32();
        float fPosX, fPosZ;
        fPosX = br.ReadSingle();
        fPosZ = br.ReadSingle();
        _nRows = br.ReadInt32();
        _nColumns = br.ReadInt32();
        _fTileWidthSize = br.ReadSingle();
        _fTileHeightSize = br.ReadSingle();
        if (nVersion < 1)
        {
            _nPosIndex_X = (int)(fPosX / _fTileWidthSize);
            _nPosIndex_Z = (int)(fPosZ / _fTileHeightSize);
        }
        else
        {
            _nPosIndex_X = br.ReadInt32();
            _nPosIndex_Z = br.ReadInt32();
        }
        /*
        _fHalfSizeX = _nColumns * _fTileWidthSize * 0.5f;
        _fHalfSizeZ = _nRows * _fTileHeightSize * 0.5f;
        */
        _transform = transform;
        _transform.localPosition = new Vector3(fPosX, 0f, fPosZ);
        _transform.parent.localScale = Vector3.one;

        int nCount = br.ReadInt32();
        _tileChunkDataInfo = new TileChunkDataInfo[nCount];
        for (int i = 0; i < nCount; i++)
        {
            _tileChunkDataInfo[i] = new TileChunkDataInfo();
            _tileChunkDataInfo[i]._byTexture = br.ReadByte();
        }
        _materialBlind = materialBlind;
        MakeMesh(false, true);
        bRet = true;
        return bRet;
    }
    /*
    public void SaveTileChunkDataInfo(string strFileName)
    {
        FileHelper.SaveJson(strFileName, _tileChunkDataInfo, true);
    }

    public void LoadTileChunkDataInfo(string strFileName)
    {
        _tileChunkDataInfo = FileHelper.LoadJson<TileChunkDataInfo[]>(strFileName);
        MakeMesh(false);
    }
    */
    public int GetVerticesCount()
    {
        int nRet = 0;
        if (_dicVertices != null)
            nRet = _dicVertices.Count;
        return nRet;
    }

    public int GetTrianglesCount()
    {
        int nRet = 0;
        if (_dicTriangles != null)
            nRet = _dicTriangles.Count;
        return nRet;
    }

    public int GetBlindAreaIndex()
    {
        int nRet = -1;
        BlindArea blindArea = _transform.parent.GetComponent<BlindArea>();
        if (blindArea != null)
            nRet = blindArea._nIndex;
        return nRet;

    }

}
