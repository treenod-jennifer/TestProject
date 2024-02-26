using UnityEngine;

public class AtelierInGamePack : MonoBehaviour
{
    [SerializeField] private NGUIAtlas  _inGameAtlas;
    [SerializeField] private GameObject _scoreSpineObj;
    [SerializeField] private GameObject _resultSpineObj;

    public NGUIAtlas  InGameAtlas    => _inGameAtlas;
    public GameObject ScoreSpineObj  => _scoreSpineObj;
    public GameObject ResultSpineObj => _resultSpineObj;
}