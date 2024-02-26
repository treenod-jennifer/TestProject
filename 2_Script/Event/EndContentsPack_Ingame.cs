using UnityEngine;

public class EndContentsPack_Ingame : MonoBehaviour
{
    [Header("Atlas")]
    [SerializeField]
    private NGUIAtlas ingameAtlas;
    [SerializeField]
    private NGUIAtlas uiAtlas;
    
    [Header("Spine")]
    [SerializeField]
    private GameObject scoreSpineObj;
    [SerializeField]
    private GameObject resultSpineObj;
    
    public NGUIAtlas UIAtlas
    { get { return uiAtlas; } }
    public NGUIAtlas IngameAtlas
    { get { return ingameAtlas; } }
    public GameObject ScoreSpine
    { get { return scoreSpineObj; } }
    public GameObject ResultSpineObj
    { get { return resultSpineObj; } }
}
