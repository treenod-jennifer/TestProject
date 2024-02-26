using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemCapsuleitem : MonoBehaviour
{
    [SerializeField] private UISprite sprCapsule;
    [SerializeField] private GameObject rewarRoot;
    [SerializeField] private GenericReward reward;
    [SerializeField] private GameObject objRareItem;

    [Header("LightLink")]
    [SerializeField] private GameObject objWhiteLight;
    [SerializeField] private GameObject objYellowLight;

    private GradeReward _reward = new GradeReward();

    public void UpdataData(GradeReward reward)
    {
        _reward = reward;

        InitCapsule();

        if(_reward.grade == 1)
        {
            objWhiteLight.SetActive(true);
            objYellowLight.SetActive(false);
            objRareItem.SetActive(false);
        }
        else
        {
            objWhiteLight.SetActive(false);
            objYellowLight.SetActive(true);
            objRareItem.SetActive(true);
        }
    }

    void InitCapsule()
    {
        Reward capsuleReward = new Reward();
        capsuleReward.type = _reward.type;
        capsuleReward.value = _reward.value;

        sprCapsule.spriteName = $"capsule_{GetCapsuleColor()}";
        rewarRoot.SetActive(false);

        reward.SetReward(capsuleReward);
    }

    public void OpenCapsule(bool isSkip)
    {
        sprCapsule.spriteName = $"capsule_{GetCapsuleColor()}_op";
        rewarRoot.SetActive(true);

        if (isSkip == false)
        {
            if (_reward.grade == 1)
            {
                ManagerSound.AudioPlay(AudioLobby.event_capsule_open_n);
            }
            else
            {
                ManagerSound.AudioPlay(AudioLobby.event_capsule_open_r);
            }
        }
    }

    string GetCapsuleColor()
    {
        if (_reward.grade == 1)
            return "b";
        else
            return "r";
    }
}
