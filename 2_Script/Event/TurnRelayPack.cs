using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnRelayPack : MonoBehaviour
{
    [Header("Texture")]
    [SerializeField] private Texture stageReadyBGTexture;
    [SerializeField] private Texture stageReadyTitleTexture;

    [Header("Trigger")]
    [SerializeField] private GameObject areaActive;

    public Texture StageReadyBGTexture
    { get { return stageReadyBGTexture; } }

    public Texture StageReadyTitleTexture
    { get { return stageReadyTitleTexture; } }

    public GameObject AreaActiveObj
    { get { return areaActive; } }
}
