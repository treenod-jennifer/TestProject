using UnityEngine;

public class AtelierPack : MonoBehaviour
{
    [SerializeField] private NGUIAtlas  _atlasUI;
    [SerializeField] private GameObject _objSpine;

    public NGUIAtlas  AtlasUI  => _atlasUI;
    public GameObject ObjSpine => _objSpine;
}