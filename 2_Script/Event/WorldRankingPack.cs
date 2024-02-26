using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class WorldRankingPack : MonoBehaviour
{
    [Header("Texture")]
    [SerializeField]
    private Texture rankingTitle;
    [SerializeField]
    private Texture readyTitle;
    [SerializeField]
    private Texture cooperationTitle;
    [SerializeField]
    private Texture progressBar;
    [SerializeField]
    private Texture progressBox;

    [Header("Atlas")]
    [SerializeField]
    private NGUIAtlas ingameAtlas;

    [Header("Spine")]
    [SerializeField]
    private GameObject scoreSpineObj;
    [SerializeField]
    private GameObject resultSpineObj;
    [SerializeField]
    private GameObject topGaugesSpineObj;

    public Texture RankingTitle
    {get { return rankingTitle; }}

    public Texture ReadyTitle
    { get { return readyTitle; } }

    public Texture CooperationTitle 
    { get { return cooperationTitle; } }

    public Texture ProgressBar 
    { get { return progressBar; } }

    public Texture ProgressBox
    { get { return progressBox; } }

    public NGUIAtlas IngameAtlas
    { get { return ingameAtlas; } }

    public GameObject ScoreSpine
    { get { return scoreSpineObj; } }

    public GameObject ResultSpineObj
    { get { return resultSpineObj; } }

    public GameObject TopGaugesSpineObj
    { get { return topGaugesSpineObj; } }
}
