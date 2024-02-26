using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HeartWayImage : MonoBehaviour
{
    public enum WAYSPRITE_DIR
    {
        SPRITE_STRAIGHT,
        SPRITE_CURVE,
        END,
    }

    [SerializeField] private UISprite spriteWay;
    [SerializeField] private UISprite spriteEffect;
    [SerializeField] private GameObject effectRoot;

    //현재 길이 표시되는 인덱스 리스트
    [HideInInspector] public List<Vector2Int> listWayIdx = new List<Vector2Int>();

    //길 이미지 타입
    private WAYSPRITE_DIR waySpriteState = WAYSPRITE_DIR.END;

    //길 이미지가 향하는 방향
    private BlockDirection spriteDirection = BlockDirection.NONE;
    //블럭이 흐르는 방향
    private BlockDirection moveDirection= BlockDirection.NONE;

    //효과 리스트
    private List<UISprite> listEffect = new List<UISprite>();

    //길 사이즈
    const int HEARTWAY_SPRITE_WIDTH = 79;
    const int HEARTWAY_SPRITE_HEIGHT = 79;

    const int WAYSPRITE_DEPTH = (int)GimmickDepth.DECO_SURFACE;

    private float boardSize = 79f;
    private float actionTime = 1.0f;


    #region 하트길 오브젝트 생성
    /// <summary>
    /// 하트 길 초기화
    /// sDireciton : 하트길 이미지가 꺾이는 방향
    /// mDirection : 해당 길 위에서 실제 블럭이 움직이는 방향(상,하,좌,우)
    /// </summary>
    public void InitHeartWay(List<Vector2Int> listIdx, WAYSPRITE_DIR wState, BlockDirection sDirection, BlockDirection mDirection)
    {
        listWayIdx = new List<Vector2Int>(listIdx);
        waySpriteState = wState;
        spriteDirection = sDirection;
        moveDirection = mDirection;

        //길 이미지 초기화
        InitHeartWaySprite();

        //길 이펙트 등록
        InitHeartWayEffect();
    }

    private void InitHeartWaySprite()
    {
        //이미지 뎁스 설정
        spriteWay.depth = WAYSPRITE_DEPTH;

        //이미지 피봇 및 방향 설정
        switch (spriteDirection)
        {
            case BlockDirection.UP:
                spriteWay.pivot = UIWidget.Pivot.Top;
                break;
            case BlockDirection.DOWN:
                spriteWay.pivot = UIWidget.Pivot.Bottom;
                break;
            case BlockDirection.RIGHT:
                spriteWay.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                spriteWay.pivot = UIWidget.Pivot.Top;
                break;
            case BlockDirection.LEFT:
                spriteWay.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                spriteWay.pivot = UIWidget.Pivot.Bottom;
                break;
            case BlockDirection.UP_RIGHT:
                spriteWay.pivot = UIWidget.Pivot.TopLeft;
                spriteWay.flip = UIBasicSprite.Flip.Horizontally;
                break;
            case BlockDirection.DOWN_RIGHT:
                spriteWay.pivot = UIWidget.Pivot.BottomLeft;
                spriteWay.flip = UIBasicSprite.Flip.Both;
                break;
            case BlockDirection.DOWN_LEFT:
                spriteWay.pivot = UIWidget.Pivot.BottomRight;
                spriteWay.flip = UIBasicSprite.Flip.Vertically;
                break;
            case BlockDirection.UP_LEFT:
                spriteWay.pivot = UIWidget.Pivot.TopRight;
                break;
        }

        //이미지 적용 및 사이즈 조절
        switch (waySpriteState)
        {
            case WAYSPRITE_DIR.SPRITE_CURVE:
                spriteWay.spriteName = "heartWay_Round";
                spriteWay.width = HEARTWAY_SPRITE_WIDTH;
                spriteWay.height = HEARTWAY_SPRITE_HEIGHT;
                break;
            case WAYSPRITE_DIR.SPRITE_STRAIGHT:
                spriteWay.spriteName = "heartWay_Straight";
                spriteWay.width = HEARTWAY_SPRITE_WIDTH;
                spriteWay.height = ((HEARTWAY_SPRITE_HEIGHT - 1) * listWayIdx.Count);
                break;
            case WAYSPRITE_DIR.END:
                spriteWay.spriteName = "heartWay_Straight";
                spriteWay.width = HEARTWAY_SPRITE_WIDTH;
                UIWidget.Pivot originPivot = spriteWay.pivot;
                spriteWay.pivot = (originPivot == UIWidget.Pivot.Top) ?
                    UIWidget.Pivot.Bottom : UIWidget.Pivot.Top;
                spriteWay.height = (int)(HEARTWAY_SPRITE_HEIGHT * 0.5f);
                spriteWay.pivot = originPivot;
                break;
        }
    }

    private void InitHeartWayEffect()
    {
        if (GameManager.instance.state == GameState.EDIT)
        {
            spriteEffect.gameObject.SetActive(false);
            return;
        }

        int effectDepth = spriteWay.depth + 1;

        spriteEffect.gameObject.SetActive(true);
        listEffect.Add(spriteEffect);
        spriteEffect.depth = effectDepth;

        int boardCount = listWayIdx.Count;
        if (boardCount > 1)
        {
            for (int i = 1; i < boardCount; i++)
            {
                UISprite effect = NGUITools.AddChild(effectRoot, spriteEffect.gameObject).GetComponent<UISprite>();
                effect.depth = effectDepth;
                listEffect.Add(effect);
            }
        }

        //연출
        ActionHeartWayEffect();
    }
    #endregion

    #region 하트 길 사이즈 조정 관련
    /// <summary>
    /// 하트 길 사이즈 조절하는 함수(블럭이 움직이는 시간과 동일한 시간동안 동작)
    /// </summary>
    public IEnumerator CoRemoveHeartWay()
    {
        listWayIdx.RemoveAt(0);
        float actionTime = ManagerBlock.instance.GetIngameTime(0.2f);
        int targetSize = spriteWay.height - HEARTWAY_SPRITE_HEIGHT;

        if (waySpriteState == WAYSPRITE_DIR.SPRITE_CURVE)
        {
            if (moveDirection == BlockDirection.RIGHT || moveDirection == BlockDirection.LEFT)
                DOTween.To(() => spriteWay.height, x => spriteWay.height = x, targetSize, actionTime);
            else
                DOTween.To(() => spriteWay.width, x => spriteWay.width = x, targetSize, actionTime);
        }
        else
        {
            DOTween.To(() => spriteWay.height, x => spriteWay.height = x, targetSize, actionTime);
        }

        if (listEffect.Count > 0)
        {
            UISprite tempSprite = listEffect[0];
            DOTween.ToAlpha(() => tempSprite.color, x => tempSprite.color = x, 0, actionTime);
        }
        yield return actionTime;

        if (listEffect.Count > 0)
        { 
            Destroy(listEffect[0].gameObject);
            listEffect.RemoveAt(0);
        }
    }

    public void SetStartWay()
    {
        int addSize = (int)(HEARTWAY_SPRITE_HEIGHT * 0.5f);
        if (waySpriteState == WAYSPRITE_DIR.SPRITE_CURVE)
        {
            if (moveDirection == BlockDirection.UP || moveDirection == BlockDirection.DOWN)
                spriteWay.width += addSize;
            else
                spriteWay.height += addSize;
        }
        else
        {
            spriteWay.height += addSize;
        }
    }

    #endregion

    public void RemoveHeartWay()
    {
        if (listEffect.Count > 0)
        {
            Destroy(listEffect[0].gameObject);
            listEffect.RemoveAt(0);
        }
    }

    private void ActionHeartWayEffect()
    {
        if (waySpriteState == WAYSPRITE_DIR.SPRITE_CURVE)
        {
            ActionHeartWayEffect_Curve();
        }
        else
        {
            ActionHeartWayEffect_Straight();
        }
    }

    private void ActionHeartWayEffect_Straight()
    {
        int offsetY = (spriteDirection == BlockDirection.UP || spriteDirection == BlockDirection.RIGHT) ? 1 : -1;

        int effectCnt = listEffect.Count;
        for (int i = 0; i < listEffect.Count; i++)
        {
            switch (spriteDirection)
            {
                case BlockDirection.LEFT:
                    listEffect[i].flip = UISprite.Flip.Vertically;
                    break;
                case BlockDirection.DOWN:
                    listEffect[i].flip = UISprite.Flip.Vertically;
                    break;
            }    

            float startPosY = ((effectCnt - i) * -boardSize * offsetY);
            float targetPosY = startPosY + (boardSize * offsetY);
            listEffect[i].transform.localPosition = new Vector3(0f, startPosY, 0f);
            listEffect[i].transform.DOLocalMoveY(targetPosY, actionTime).SetEase(Ease.Linear).SetLoops(-1);
        }
    }

    private void ActionHeartWayEffect_Curve()
    {
        float halfSize = boardSize * 0.5f;
        float doubleSIze = boardSize * 2f;
        switch (spriteDirection)
        {
            case BlockDirection.UP_RIGHT:
                if (moveDirection == BlockDirection.UP || moveDirection == BlockDirection.RIGHT)
                {
                    Vector3[] wayPoints = new Vector3[3];
                    wayPoints.SetValue(new Vector3(halfSize, -boardSize, 0f), 0);
                    wayPoints.SetValue(new Vector3(halfSize + 3f, -halfSize - 3f, 0f), 1);
                    wayPoints.SetValue(new Vector3(boardSize, -halfSize, 0f), 2);
                    for (int i = 0; i < listEffect.Count; i++)
                    {
                        listEffect[i].transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        listEffect[i].transform.localPosition = wayPoints[0];
                        listEffect[i].transform.DOLocalPath(wayPoints, actionTime, PathType.Linear).SetEase(Ease.Linear).SetLoops(-1);
                        listEffect[i].transform.DORotate(new Vector3(0f, 0f, -90f), actionTime).SetEase(Ease.Linear).SetLoops(-1);
                    }
                }
                else
                {
                    Vector3[] wayPoints = new Vector3[3];
                    wayPoints.SetValue(new Vector3(boardSize, -halfSize, 0f), 0);
                    wayPoints.SetValue(new Vector3(halfSize + 3f, -halfSize - 3f, 0f), 1);
                    wayPoints.SetValue(new Vector3(halfSize, -boardSize, 0f), 2);
                    for (int i = 0; i < listEffect.Count; i++)
                    {
                        listEffect[i].transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                        listEffect[i].transform.localPosition = wayPoints[0];
                        listEffect[i].transform.DOLocalPath(wayPoints, actionTime, PathType.Linear).SetEase(Ease.Linear).SetLoops(-1);
                        listEffect[i].transform.DORotate(new Vector3(0f, 0f, 180f), actionTime).SetEase(Ease.Linear).SetLoops(-1);
                    }
                }
                break;
            case BlockDirection.UP_LEFT:
                if (moveDirection == BlockDirection.UP || moveDirection == BlockDirection.LEFT)
                {
                    Vector3[] wayPoints = new Vector3[3];
                    wayPoints.SetValue(new Vector3(-halfSize, -boardSize, 0f), 0);
                    wayPoints.SetValue(new Vector3(-halfSize - 3f, -halfSize - 3f, 0f), 1);
                    wayPoints.SetValue(new Vector3(-boardSize, -halfSize, 0f), 2);
                    for (int i = 0; i < listEffect.Count; i++)
                    {
                        listEffect[i].transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        listEffect[i].transform.localPosition = wayPoints[0];
                        listEffect[i].transform.DOLocalPath(wayPoints, actionTime, PathType.Linear).SetEase(Ease.Linear).SetLoops(-1);
                        listEffect[i].transform.DORotate(new Vector3(0f, 0f, 90f), actionTime).SetEase(Ease.Linear).SetLoops(-1);
                    }
                }
                else
                {
                    Vector3[] wayPoints = new Vector3[3];
                    wayPoints.SetValue(new Vector3(-boardSize, -halfSize, 0f), 0);
                    wayPoints.SetValue(new Vector3(-halfSize - 3f, -halfSize - 3f, 0f), 1);
                    wayPoints.SetValue(new Vector3(-halfSize, -boardSize, 0f), 2);
                    for (int i = 0; i < listEffect.Count; i++)
                    {
                        listEffect[i].transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                        listEffect[i].transform.localPosition = wayPoints[0];
                        listEffect[i].transform.DOLocalPath(wayPoints, actionTime, PathType.Linear).SetEase(Ease.Linear).SetLoops(-1);
                        listEffect[i].transform.DORotate(new Vector3(0f, 0f, -180f), actionTime).SetEase(Ease.Linear).SetLoops(-1);
                    }
                }
                break;
            case BlockDirection.DOWN_RIGHT:
                if (moveDirection == BlockDirection.DOWN || moveDirection == BlockDirection.RIGHT)
                {
                    Vector3[] wayPoints = new Vector3[3];
                    wayPoints.SetValue(new Vector3(halfSize, boardSize, 0f), 0);
                    wayPoints.SetValue(new Vector3(halfSize + 3f, halfSize + 3f, 0f), 1);
                    wayPoints.SetValue(new Vector3(boardSize, halfSize, 0f), 2);
                    for (int i = 0; i < listEffect.Count; i++)
                    {
                        listEffect[i].transform.rotation = Quaternion.Euler(0f, 0f, -180f);
                        listEffect[i].transform.localPosition = wayPoints[0];
                        listEffect[i].transform.DOLocalPath(wayPoints, actionTime, PathType.Linear).SetEase(Ease.Linear).SetLoops(-1);
                        listEffect[i].transform.DORotate(new Vector3(0f, 0f, -90f), actionTime).SetEase(Ease.Linear).SetLoops(-1);
                    }
                }
                else
                {
                    Vector3[] wayPoints = new Vector3[3];
                    wayPoints.SetValue(new Vector3(boardSize, halfSize, 0f), 0);
                    wayPoints.SetValue(new Vector3(halfSize + 3f, halfSize + 3f, 0f), 1);
                    wayPoints.SetValue(new Vector3(halfSize, boardSize, 0f), 2);
                    for (int i = 0; i < listEffect.Count; i++)
                    {
                        listEffect[i].transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                        listEffect[i].transform.localPosition = wayPoints[0];
                        listEffect[i].transform.DOLocalPath(wayPoints, actionTime, PathType.Linear).SetEase(Ease.Linear).SetLoops(-1);
                        listEffect[i].transform.DORotate(new Vector3(0f, 0f, 0f), actionTime).SetEase(Ease.Linear).SetLoops(-1);
                    }
                }
                break;
            case BlockDirection.DOWN_LEFT:
                if (moveDirection == BlockDirection.DOWN || moveDirection == BlockDirection.LEFT)
                {
                    Vector3[] wayPoints = new Vector3[3];
                    wayPoints.SetValue(new Vector3(-halfSize, boardSize, 0f), 0);
                    wayPoints.SetValue(new Vector3(-halfSize - 3f, halfSize + 3f, 0f), 1);
                    wayPoints.SetValue(new Vector3(-boardSize, halfSize, 0f), 2);
                    for (int i = 0; i < listEffect.Count; i++)
                    {
                        listEffect[i].transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                        listEffect[i].transform.localPosition = wayPoints[0];
                        listEffect[i].transform.DOLocalPath(wayPoints, actionTime, PathType.Linear).SetEase(Ease.Linear).SetLoops(-1);
                        listEffect[i].transform.DORotate(new Vector3(0f, 0f, 90f), actionTime).SetEase(Ease.Linear).SetLoops(-1);
                    }
                }
                else
                {
                    Vector3[] wayPoints = new Vector3[3];
                    wayPoints.SetValue(new Vector3(-boardSize, halfSize, 0f), 0);
                    wayPoints.SetValue(new Vector3(-halfSize - 3f, halfSize + 3f, 0f), 1);
                    wayPoints.SetValue(new Vector3(-halfSize, boardSize, 0f), 2);
                    for (int i = 0; i < listEffect.Count; i++)
                    {
                        listEffect[i].transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                        listEffect[i].transform.localPosition = wayPoints[0];
                        listEffect[i].transform.DOLocalPath(wayPoints, actionTime, PathType.Linear).SetEase(Ease.Linear).SetLoops(-1);
                        listEffect[i].transform.DORotate(new Vector3(0f, 0f, 0f), actionTime).SetEase(Ease.Linear).SetLoops(-1);
                    }
                }
                break;
        }
    }
}
