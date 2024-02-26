using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageTarget : MonoBehaviour {

    [System.NonSerialized]
    public Transform _transform;

    public UISprite targetSprite;
    public UILabel targetCount;
    public UILabel targetCountShadow;

    public UISprite checkSprite;

    public UILabel ScoreLabelSprite;
    public UILabel ScoreLabelShadowSprite;

    [System.NonSerialized]
    public int index;
    public TARGET_TYPE targetType;
    public BlockColorType targetColor;
    public int count;

        void Awake()
    {
        _transform = transform;
    }

    public void initScore(int score)
    {
        targetSprite.gameObject.SetActive(false);

        targetCount.gameObject.SetActive(false);
        ScoreLabelShadowSprite.gameObject.SetActive(false);

        checkSprite.gameObject.SetActive(false);

        ScoreLabelSprite.gameObject.SetActive(true);
        ScoreLabelShadowSprite.gameObject.SetActive(true);

        ScoreLabelSprite.text = "[FFCC00]Score[FFFFFF] " + score.ToString();
        ScoreLabelShadowSprite.text = "[FFCC00]Score[FFFFFF] " + score.ToString();
    }

}
