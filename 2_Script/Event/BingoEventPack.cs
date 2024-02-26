using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BingoEventPack : MonoBehaviour
{
    [Header("Atlas")] 
    [SerializeField] private NGUIAtlas atlasOutgame;
    [SerializeField] private NGUIAtlas atlasIngame;

    [Header("Spine")] 
    [SerializeField] private GameObject objResultSpine;

    public NGUIAtlas AtlasOutgame
    { get { return atlasOutgame; } }
    public NGUIAtlas AtlasIngame
    { get { return atlasIngame; } }
    public GameObject ObjResultSpine
    { get { return objResultSpine; } }
}
