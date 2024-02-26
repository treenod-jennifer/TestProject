using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupStageTarget : UIPopupBase
{
    static public UIPopupStageTarget instance = null;
    //타겟 생성 위치
    public GameObject TargetRoot;
    
    float _timer = 0f;
    bool _sendDestroy = false;

    List<StageTarget> stageTargets = new List<StageTarget>();

    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        var enumerator = ManagerBlock.instance.dicCollectCount.GetEnumerator();
        while (enumerator.MoveNext())
        {
            TARGET_TYPE targetType = enumerator.Current.Key;

            if (enumerator.Current.Value != null)
            {
                string targetName = (targetType != TARGET_TYPE.COLORBLOCK) ?
                    string.Format("StageTarget_{0}", targetType) : "StageTarget";

                var e = enumerator.Current.Value.GetEnumerator();
                while (e.MoveNext())
                {
                    BlockColorType colorType = e.Current.Key;

                    StageTarget target = NGUITools.AddChild(TargetRoot, ManagerUI._instance._objInGameTarget).GetComponent<StageTarget>();
                    target.targetType = targetType;
                    target.targetColor = colorType;

                    //목표 수 표시
                    string collectCount = e.Current.Value.collectCount.ToString();
                    target.targetCount.text = collectCount;
                    target.targetCountShadow.text = collectCount;

                    //목표 이미지 설정
                    string targetColorName = (colorType != BlockColorType.NONE) ?
                        string.Format("{0}_{1}", targetName, colorType) : targetName;
                    target.targetSprite.spriteName = targetColorName;
                    target.targetSprite.MakePixelPerfect();

                    stageTargets.Add(target);
                }
            }
        }

        float startPos = (1 - stageTargets.Count) * 48;
        for (int i = 0; i < stageTargets.Count; i++)
        {
            stageTargets[i].transform.localPosition = new Vector3(startPos + 96 * i, 0, 0);
        }
    }

    public override void OnClickBtnBack()
    {
        return;
    }

    new void OnDestroy()
    {
        base.OnDestroy();
        for (int i = 0; i < stageTargets.Count; i++)
        {
            Destroy(stageTargets[i].gameObject);
        }
        stageTargets.Clear();

        instance = null;
    }

    void Update()
    {
        _timer += Global.deltaTime;
        if (_timer > 1.3f && !_sendDestroy)
        {
            _sendDestroy = true;
            ManagerUI._instance.ClosePopUpUI();
        }
    }

    void OnClickBtnSkip()
    {
        if (_timer > 0.5f && !_sendDestroy)
        {
            _sendDestroy = true;
            ManagerUI._instance.ClosePopUpUI();
        }
    }
}
