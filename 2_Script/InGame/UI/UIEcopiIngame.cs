using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIEcopiIngame : MonoBehaviour
{
    public enum IngameEcopiUIState
    {
        HIDE,   //숨어있는 상태
        BLOSSOM // 꽃 피운 상태
    }

    [SerializeField]
    private UISprite spriteFace;
    [SerializeField]
    private UISprite spriteFlower;

    //에코피 UI 루트의 트랜스폼
    private Transform uiRootTr;

    [SerializeField]
    private Transform ecopiRootTr;
    [SerializeField]
    private Transform flowerRootTr;

    //대기 중 코루틴
    private Coroutine waitCoroutine;
    private Tween waitTween;

    //에코피 이미지 원 사이즈.
    private Vector3 originScale_ecopi = Vector3.zero;

    //에코피 상태에 따른 위치 정보
    private Vector3 targetOffsetPos_Wait = new Vector3(25f, -28f, 0f);
    private Vector3 targetOffsetPos_Blossom = new Vector3(25f, 25f, 0f);
    private Vector3 originPos_uiRoot = Vector3.zero;

    private void Start()
    {
        uiRootTr = this.transform;
        originScale_ecopi = ecopiRootTr.transform.localScale;
        originPos_uiRoot = uiRootTr.localPosition;
        flowerRootTr.gameObject.SetActive(false);
    }

    public void InitEcopiUI(ScoreFlowerType maxType_flowerState)
    {
        waitCoroutine = StartCoroutine(CoHideAction());

        string spriteName = string.Format("stage_icon_level_{0:D2}", (int)maxType_flowerState);
        spriteFlower.spriteName = spriteName;
        spriteFlower.MakePixelPerfect();
    }

    private IEnumerator CoHideAction()
    {
        while (true)
        {
            //좌우 액션
            waitTween = uiRootTr.DORotate(new Vector3(0f, 0f, 10f), 0.8f).SetLoops(10, LoopType.Yoyo).SetEase(Ease.Linear);
            yield return waitTween.WaitForElapsedLoops(10);

            waitTween = uiRootTr.DORotate(new Vector3(0f, 0f, 0f), 0.2f);
            yield return new WaitForSeconds(0.2f);

            //위치 액션
            ecopiRootTr.localScale = Vector3.one * 2f;
            waitTween = uiRootTr.DOLocalMove(originPos_uiRoot + targetOffsetPos_Wait, 0.3f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.3f);

            //스케일 액션
            waitTween = ecopiRootTr.DOScale(Vector3.one * 1.95f, 0.4f).SetLoops(4, LoopType.Yoyo);
            yield return waitTween.WaitForElapsedLoops(4);

            //위치 액션
            waitTween = uiRootTr.DOLocalMove(originPos_uiRoot, 0.3f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void BlossomAction()
    {
        Debug.Log("BlossomAction");
        //대기 상태의 코루틴 종료.
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        //대기 상태의 트윈 종료.
        if (waitTween != null)
        {
            waitTween.Kill();
            waitTween = null;
        }

        StartCoroutine(CoBlossomAction());
    }

    private IEnumerator CoBlossomAction()
    {
        flowerRootTr.transform.localScale = Vector3.zero;
        flowerRootTr.gameObject.SetActive(true);

        //에코피 크기 및 위치 설정.
        ecopiRootTr.localScale = originScale_ecopi;
        uiRootTr.DOLocalMove(originPos_uiRoot + targetOffsetPos_Wait, 0.2f);
        yield return new WaitForSeconds(0.2f);

        //에코피 얼굴 설정 및 위치 설정
        spriteFace.spriteName = "ecopi_event_ingame_face_2";
        spriteFace.MakePixelPerfect();
        uiRootTr.DOLocalMove(originPos_uiRoot + targetOffsetPos_Blossom, 0.3f);

        //꽃 스케일 연출.
        flowerRootTr.DOScale(Vector3.one, 1.0f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.5f);

        //이펙트 및 사운드 출력.
        InGameEffectMaker.instance.MakeDuckEffect(flowerRootTr.position);
        ManagerSound.AudioPlay(AudioInGame.GET_CANDY);

        yield return new WaitForSeconds(0.5f);

        //에코피 움직임.
        uiRootTr.DORotate(new Vector3(0f, 0f, -10f), 0.3f);
        yield return new WaitForSeconds(0.3f);

        uiRootTr.DORotate(new Vector3(0f, 0f, 10f), 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
    }
}
