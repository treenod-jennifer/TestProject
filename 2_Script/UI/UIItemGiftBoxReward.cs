using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemGiftBoxReward : MonoBehaviour {

    [System.NonSerialized]
    public Transform _transform;
    public UILabel   _laberCount;
    public UILabel   _laberLuckyCount;
    public UITexture _reward;
    public UITexture _light1;
    public UITexture _light2;
    public UIItemRewardGradeEffect gradeEffect;
    public UIPanel panel;

    void Awake()
    {
        _transform = transform;
    }
	// Use this for initialization
	void Start () {
		
	}
	public void SetAlpha(float in_alpha)
    {
        _laberCount.alpha = in_alpha;
        _reward.alpha = in_alpha;
        _light1.alpha = in_alpha * 0.5f;
        _light2.alpha = in_alpha * 0.6f;
    }

    public void SetRewardPanel(int depth, int order)
    {
        panel.depth = depth + 1;
        panel.sortingOrder = order + 6;
    }
}
