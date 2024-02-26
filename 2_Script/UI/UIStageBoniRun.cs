using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStageBoniRun : MonoBehaviour
{
    [SerializeField]
    private UISprite sprCharacter;
    [SerializeField]
    private UISprite _sprBoniArrow;

    private int _nNum = 1;
    private float _nTimer = 0.0f;

    Transform _transform;
    Vector3 _initPosition;
    string characterName;

    private bool bBoni = false;

    void Start()
    {
        _transform = transform;
        _initPosition = _transform.localPosition;
    }

    public void SetCharacter(bool bCharBoni)
    {
        bBoni = bCharBoni;
        if (bBoni == true)
            characterName = "stage_icon_boni_0";
        else
            characterName = "stage_icon_bluebird_0";
    }

    public void SetArrow(BtnStageArrowDir _dir)
    {
        //방향 데이터에 따라 이미지 회전, 위치, 크기 설정.
        if (_dir == BtnStageArrowDir.none)
        {
            _sprBoniArrow.enabled = false;
            return;
        }
        else
        {
            if (_sprBoniArrow.enabled == false)
                _sprBoniArrow.enabled = true;

            if (_dir == BtnStageArrowDir.down)
            {
                _sprBoniArrow.transform.localScale = new Vector3(1, -1, 1);
                _sprBoniArrow.transform.localPosition = new Vector3(0, -43, 0);
                _sprBoniArrow.transform.localRotation = Quaternion.Euler(0, 0, -90);
            }
            else
            {
                _sprBoniArrow.transform.localScale = Vector3.one;
                _sprBoniArrow.transform.localPosition = new Vector3(35, -5, 0);
                _sprBoniArrow.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
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

    private void LateUpdate()
    {
        _nTimer += 8.0f * Time.deltaTime;
        if (_nTimer > 1.0f)
        {
            sprCharacter.spriteName = string.Format(characterName + "{0}", _nNum);
            sprCharacter.MakePixelPerfect();

            if (_nNum == 1)
                _nNum = 2;
            else
                _nNum = 1;
            _nTimer = 0f;
        }

        if (bBoni == true)
        {
            _transform.localPosition = _initPosition + new Vector3(Mathf.Sin(Time.time * 15f) * 2f, Mathf.Abs(Mathf.Cos(Time.time * 15f) * 5f), 0f);
        }
    }
}
