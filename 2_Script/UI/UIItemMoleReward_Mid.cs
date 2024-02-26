using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class UIItemMoleReward_Mid : MonoBehaviour, IMoleReward
{

    [SerializeField] public GenericReward reward;

    [SerializeField] UITexture twinkleTexture;

    [SerializeField] Texture2D[] twinkleTex;
    
    public IEnumerator CoAppear()
    {
        yield return null;
    }

    public IEnumerator CoDisappear()
    {
        this.transform.DOShakePosition(0.5f, 10f, 40);
        yield return new WaitForSeconds(0.6f);

        Vector3 orgPos = transform.localPosition;
        Vector3 orgScl = transform.localScale;
        orgScl.x *= -1;

        ManagerSound.AudioPlay(AudioLobby.event_mole_star);

        //reward.gameObject.SetActive(false);
        //이미지 비활성화
        reward.rewardIcon_T.enabled = false;
        reward.rewardIcon_S.enabled = false;

        //텍스트 연출.
        //StartCoroutine(CoTextAction());
        float textActionTime = 0.8f;

        //반짝이
        float twinkleActionTime = 0f;
        twinkleTexture.gameObject.SetActive(true);
        twinkleTexture.enabled = true;
        for (int i = 0; i < twinkleTex.Length; ++i)
        {   
            twinkleTexture.mainTexture = twinkleTex[i];
            twinkleTexture.MakePixelPerfect();
            yield return new WaitForSeconds(0.15f);

            if ((twinkleTex.Length == 1 && i == 0) ||
                i == 1)
            {
                twinkleActionTime += 0.15f;
                StartCoroutine(CoTextAction());
            }
        }
        twinkleTexture.gameObject.SetActive(false);

        //남은 연출시간 동안 기다려줌.
        if (twinkleActionTime < textActionTime)
            yield return new WaitForSeconds(textActionTime - twinkleActionTime);

        yield return new WaitForSeconds(0.1f);
        Destroy(this.gameObject, 0.3f);
        yield break;
    }

    private void ShowText()
    {
        for (int i = 0; i < reward.rewardCount.Length; i++)
        {
            reward.rewardCount[i].gameObject.SetActive(true);
        }
    }

    private IEnumerator CoTextAction()
    {
        ShowText();
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < reward.rewardCount.Length; i++)
        {
            UILabel countText = reward.rewardCount[i];
            float targetY = countText.transform.localPosition.y + 10f;
            countText.transform.DOLocalMoveY(targetY, 0.5f);
            DOTween.ToAlpha(() => countText.color, x => countText.color = x, 0f, 0.5f);
        }
        yield return new WaitForSeconds(0.5f);
        yield return null;
    }

    public void SelfDestroy()
    {
        Destroy(this.gameObject);
    }
}
