using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStageBoniRun : MonoBehaviour
{
    [SerializeField]
    private UISprite sprCharacter;
    [SerializeField]
    private UISprite _sprBoniArrow;
    [SerializeField]
    private AnimationCurve arrow_AniController;

    private int _nNum = 1;
    private float _nTimer = 0.0f;

    //오브젝트 스프라이트 변경 속도
    private float runSpeed = 8.0f;

    Vector3 _initPosition;
    string characterName;

    private bool bBoni = false;
    private bool bStopRunning = false;

    public void InitCharacterSprite()
    {
        if (bStopRunning == true) _nNum = 2;

        sprCharacter.spriteName = string.Format(characterName + "{0}", _nNum);
        sprCharacter.MakePixelPerfect();
    }

    public void SetCharacter(bool bCharBoni, bool _bStopRunning = false)
    {
        bStopRunning = _bStopRunning;
        bBoni = bCharBoni;
        if (bBoni == true)
            characterName = "stage_icon_boni_0";
        else
            characterName = "stage_icon_bluebird_0";

        InitCharacterSprite();
    }

    public void SetArrow(BtnStageArrowDir _dir)
    {
        switch (_dir)
        {
            case BtnStageArrowDir.none:
                _sprBoniArrow.enabled = false;
                break;
            case BtnStageArrowDir.right:
                _sprBoniArrow.enabled = true;
                _sprBoniArrow.transform.localScale = Vector3.one;
                _sprBoniArrow.transform.localPosition = new Vector3(80, 5, 0);
                _sprBoniArrow.transform.localRotation = Quaternion.Euler(0, 0, -90);
                break;
            case BtnStageArrowDir.left:
                _sprBoniArrow.enabled = true;
                _sprBoniArrow.transform.localScale = Vector3.one;
                _sprBoniArrow.transform.localPosition = new Vector3(-90, 5, 0);
                _sprBoniArrow.transform.localRotation = Quaternion.Euler(0, 0, 90);
                break;
            case BtnStageArrowDir.down:
                _sprBoniArrow.enabled = true;
                _sprBoniArrow.transform.localScale = Vector3.one;
                _sprBoniArrow.transform.localPosition = new Vector3(0, -90, 0);
                _sprBoniArrow.transform.localRotation = Quaternion.Euler(0, 0, 180);
                break;
            case BtnStageArrowDir.up:
                _sprBoniArrow.enabled = true;
                _sprBoniArrow.transform.localScale = Vector3.one;
                _sprBoniArrow.transform.localPosition = new Vector3(0, 95, 0);
                _sprBoniArrow.transform.localRotation = Quaternion.Euler(0, 0, 0);
                break;
            default:
                break;
        }
    }

    public IEnumerator CoArrowAnimation()
    {
        float timeCount = 0.0f;

        Vector3 test = _sprBoniArrow.transform.localPosition;

        while (true)
        {
            timeCount += Time.unscaledDeltaTime;
            _sprBoniArrow.transform.localPosition = test * arrow_AniController.Evaluate(timeCount);

            yield return null;
        }
    }

    public void SetBlueBirdLook(float boniXPos)
    {
        float dis = boniXPos - sprCharacter.transform.position.x;
        if (dis > 0)
        {
            sprCharacter.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            sprCharacter.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    public void SetBoniPos(bool _bRight)
    {
        if (_bRight == false)
        {
            sprCharacter.transform.localScale = new Vector3(-1, 1, 1);
            sprCharacter.transform.localPosition = new Vector3(-8, -10, 0);
        }
        else
        {
            sprCharacter.transform.localScale = Vector3.one;
            sprCharacter.transform.localPosition = new Vector3(0, -10, 0);
        }
    }

    public void SetBoniRunSpeed(float speed)
    {
        runSpeed = speed;
    }

    private Vector3 pokotaOffsetPos = new Vector3(-6, -10, 0);

    private void LateUpdate()
    {
        if (bStopRunning == true) return;

        _nTimer += runSpeed * Time.deltaTime;
        if (_nTimer > 1.0f)
        {
            InitCharacterSprite();

            if (_nNum == 1)
                _nNum = 2;
            else
                _nNum = 1;
            _nTimer = 0f;
        }

        if (bBoni == true)
        {
            sprCharacter.transform.localPosition = pokotaOffsetPos + new Vector3(Mathf.Sin(Time.time * 15f) * 2f, Mathf.Abs(Mathf.Cos(Time.time * 15f) * 5f), 0f);
        }
    }
}
