using UnityEngine;

public class SpaceTravelPackIngame : MonoBehaviour
{
    [Header("Atlas")] [SerializeField] private NGUIAtlas _atlas;

    [Header("SelectSpineObj")] [SerializeField]
    private GameObject _selectSpineObj;

    [Header("ResultSpineObj")] [SerializeField]
    private GameObject _resultSpineObj;

    public NGUIAtlas  Atlas          => _atlas;
    public GameObject SelectSpineObj => _selectSpineObj;
    public GameObject ResultSpineObj => _resultSpineObj;
}