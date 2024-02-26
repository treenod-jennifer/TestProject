//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2016 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Similar to UIButtonColor, but adds a 'disabled' state based on whether the collider is enabled or not.
/// </summary>

//[AddComponentMenu("NGUI/Interaction/UIPokoButton")]
public class UIPokoButton : UIWidgetContainer
{
    public UIBasicSprite[] _colorSprites;
    public GameObject _messageTarget;
    public string functionName;
    public bool _bIsCanSkip = true;

    AudioLobby buttonSoundType = AudioLobby.Button_01; 

    Transform _scaleRoot = null;

    static Color _pushColor = new Color(0.5f, 0.5f, 0.5f);
    float _pushTimer = 0f;
    Vector3 _startPos = Vector3.zero;

    void OnPress(bool isPressed)
    {
        // 더블클릭 방지
        if (_pushTimer > Time.time)
            return;

        _pushTimer = Time.time + 0.2f;
        if (isPressed)
            StartCoroutine(CorPressed());

    }
    void SetColor(Color in_color)
    {
        for (int i = 0; i < _colorSprites.Length; i++)
            _colorSprites[i].color = in_color;
    }
    void _Send()
    {
        if (string.IsNullOrEmpty(functionName)) return;
        if (_messageTarget == null) return;

        if (Global._instance == null) return;
        if (Global.CanClickButton() == false) return;
        Global.InitClickTime();

        _messageTarget.SendMessage(functionName, gameObject, SendMessageOptions.DontRequireReceiver);
    }
    IEnumerator CorPressed()
    {
        if (_scaleRoot == null)
        {
            _scaleRoot = transform.GetChild(0);
            _startPos = _scaleRoot.localPosition;
        }

        float timer = 0f;
        //bool sendMessage = false;
        while (true)
        {
            if (timer > 0.5f)
                timer += Time.deltaTime * 2f;
            else
                timer += Time.deltaTime * 6f;

            //if (timer > 0.5f && !sendMessage)
            //{
            //    sendMessage = true;
            //    _Send();
            //}

            if (timer > 1f)
                timer = 1f;
            float sin = Mathf.Sin(timer * Mathf.PI);
            float color = 1f - sin * 0.3f;
            SetColor(new Color(color, color, color));
            _scaleRoot.localPosition = _startPos + new Vector3(5f - timer * 5f, -5f + timer * 5f, 0f);
            _scaleRoot.localScale = new Vector3(1f + Mathf.PingPong(timer, 0.5f) * 0.13f, 1f - Mathf.PingPong(timer, 0.5f) * 0.13f, 1f);
            if (timer == 1f)
                break;

            yield return null;
        }
        //if (!sendMessage)
        //    _Send();

        yield return null;
    }

    IEnumerator CoActionCllick()
    {
        if (_scaleRoot == null)
        {
            _scaleRoot = transform.GetChild(0);
            _startPos = _scaleRoot.localPosition;
        }

        float timer = 0f;
        float timerMessage = 0f;
        bool bSend = false;

        while (true)
        {
            if (timerMessage > 0.1f && !bSend)
            {
                bSend = true;
                _Send();
            }
            timerMessage += Time.deltaTime;

            if (timer > 0.5f)
                timer += Time.deltaTime * 2f;
            else
                timer += Time.deltaTime * 6f;

            if (timer > 1f)
                timer = 1f;
         //   _scaleRoot.localPosition = _startPos + new Vector3(4f - timer * 4f, -4f + timer * 4f, 0f);
         //   _scaleRoot.localScale = new Vector3(1f + Mathf.PingPong(timer, 0.5f) * 0.1f, 1f - Mathf.PingPong(timer, 0.5f) * 0.1f, 1f);
            if (timer == 1f)
                break;
            yield return null;
        }
    }

    void OnClick()
    {
        
        ManagerSound.AudioPlay(buttonSoundType);
        StartCoroutine(CoActionCllick());
   //     StartCoroutine(CoClick());

        
        //ManagerSound.instance.PlayAudio(LobbyScene.instance._defaultTouchObjectAudioClip);
    }

 /*   IEnumerator CoClick()
    {
        yield return new WaitForSeconds(0.02f);
        _Send();
    }*/
}
