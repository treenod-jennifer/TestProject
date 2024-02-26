using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRoot : MonoBehaviour
{
    public List<Rect>           _listBlockRects  = new List<Rect>();
    public List<BlockMeshComp>  _listBlockMeshes = new List<BlockMeshComp>();

    public void MakeMesh()
    {
        _listBlockMeshes.Clear();
        transform.DestroyChildren();
        int nCount = _listBlockRects.Count;
        for (int i = 0; i < nCount; i++)
        {
            _MakeMesh(i);
        }
    }

    private void _MakeMesh(int nIndex)
    {
        GameObject go = new GameObject("BlockMesh" + nIndex.ToString("00"));
        BlockMeshComp blockMesh = go.AddComponent<BlockMeshComp>();
        blockMesh.MakeMesh(_listBlockRects[nIndex]);
        blockMesh.gameObject.transform.parent = transform;
        _listBlockMeshes.Add(blockMesh);
    }
}
