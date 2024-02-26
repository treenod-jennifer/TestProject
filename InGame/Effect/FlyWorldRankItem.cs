using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FlyWorldRankItem : MonoBehaviour
{
    [SerializeField]
    private UISprite worldRankItemImage;
    [SerializeField]
    private ParticleSystem ringEffect;

    private Vector3 targetPos;

    private System.Action endAction = null;

    public void InitEffect(Vector3 startPos, Vector3 endPos, System.Action action = null)
    {
        //ManagerUIAtlas.CheckAndApplyIngameBGAtlas(worldRankItemImage);

        this.transform.position = startPos;
        this.targetPos = endPos;
        this.endAction = action;

        StartCoroutine(CoMoveAction());
    }

    private IEnumerator CoMoveAction()
    {
        this.transform.localScale = Vector3.zero;
        this.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        this.transform.DOLocalMoveY(this.transform.localPosition.y + 50f, 0.3f);
        yield return new WaitForSeconds(0.3f);

        float dis = Vector3.Distance(transform.position, targetPos);
        float moveTime = 0.4f + (dis * 0.5f);
        if (moveTime > 0.7f)
            moveTime = 0.7f;
        moveTime = moveTime / Global.timeScalePuzzle;
        this.transform.DOMove(targetPos, moveTime);

        yield return new WaitForSeconds(moveTime);
        ManagerSound.AudioPlayMany(AudioInGame.GET_TARGET);
        worldRankItemImage.enabled = false;
        ringEffect.gameObject.SetActive(true);
        if (endAction != null)
            endAction.Invoke();

        yield return new WaitUntil(() => ringEffect.IsAlive() == false);
        InGameEffectMaker.instance.RemoveEffect(this.gameObject);
    }
}
