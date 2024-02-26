using System.Collections.Generic;
using UnityEngine;

public class UIItemSpaceTravelStar : MonoBehaviour
{
    [SerializeField] private GameObject _rewardRoot;
    [SerializeField] private UISprite _spriteStar;
    [SerializeField] private UISprite _spriteBubble;
    [SerializeField] private UISprite _spriteBubbleHide;
    [SerializeField] private Transform _objectCheck;
    [SerializeField] private GameObject _objectHighlight;
    [SerializeField] private GenericReward _rewardItem;
    [SerializeField] private List<UISprite> _spriteList;

    public UIPopUpSpaceTravel.SpaceTravelStage StageData { get; set; }
    public void InitItem(UIPopUpSpaceTravel.SpaceTravelStage stageData, Color color)
    {
        StageData = stageData;
        var rewardList = StageData.rewardList;
        gameObject.SetActive(true);
        var isClear = ManagerSpaceTravel.instance.CurrentStage > stageData.stageIndex + 1;
        _spriteStar.enabled = isClear;
        _objectHighlight.SetActive(isClear);

        _spriteStar.color = color;
        foreach (var sprite in _spriteList)
        {
            sprite.atlas = ManagerSpaceTravel.instance._spaceTravelPackUI.UIAtlas;
        }

        if (rewardList == null || rewardList.Count == 0)
        {
            _spriteBubble.gameObject.SetActive(false);
        }
        else
        {
            var bubbleWidth = 20 + 50 * rewardList.Count;
            _spriteBubble.width     = bubbleWidth;
            _spriteBubbleHide.width = bubbleWidth;
            foreach (var reward in rewardList)
            {
                var item = NGUITools.AddChild(_rewardRoot, _rewardItem.gameObject).GetComponent<GenericReward>();
                item.SetReward(reward);
                item.EnableInfoBtn(false);
                // 리워드 자릿수가 3 이상일 시 말풍선 빠져나가는 이슈 수정
                if (reward.value >= 100)
                {
                    item.rewardCount[0].transform.localPosition = new Vector2(20f, item.rewardCount[0].transform.localPosition.y);
                }
            }
            SetRewardHide();
        }
    }
    
    public void SetRewardHide()
    {
        _spriteBubbleHide.gameObject.SetActive(StageData.stageType == ManagerSpaceTravel.StageType.REWARDED);
        _objectCheck.gameObject.SetActive(StageData.stageType == ManagerSpaceTravel.StageType.REWARDED);
        _objectCheck.localPosition = new Vector2(5 - (float)_spriteBubble.width / 2, 5);
    }

    public void SetRewardRootActive(bool isActive) => _rewardRoot.SetActive(isActive);
}