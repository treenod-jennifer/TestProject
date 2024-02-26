using System.Collections.Generic;
using UnityEngine;

public class SpaceTravelPackUI : MonoBehaviour
{
    [Header("Atlas")] [SerializeField] private NGUIAtlas _uiAtlas;

    [Header("SpaceshipSpineObj")] [SerializeField]
    private GameObject _spaceshipSpineObj;

    [Header("GalaxySpineObj")] [SerializeField]
    private GameObject _galaxySpineObj;

    [Header("PlanetTextureList")] [SerializeField]
    private List<Texture2D> _planetTextureList;

    public NGUIAtlas       UIAtlas           => _uiAtlas;
    public GameObject      SpaceshipSpineObj => _spaceshipSpineObj;
    public GameObject      GalaxySpineObj    => _galaxySpineObj;
    public List<Texture2D> PlanetTextureList => _planetTextureList;
}