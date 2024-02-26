using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindTutorial : MonoBehaviour {

    [System.NonSerialized]
    public Transform _transform;
    public UIPanel _panel = null;

    public GameObject defaultBlindRoot;

    public UITexture _textureCenter;
    public UITexture _textureUp;
    public UITexture _textureDown;
    public UITexture _textureRight;
    public UITexture _textureLeft;

    public BoxCollider _colliderUp;
    public BoxCollider _colliderDown;
    public BoxCollider _colliderRight;
    public BoxCollider _colliderLeft;

    const int maxSize = 3000;

    public BoxCollider _colliderClick;
    public Method.FunctionVoid _clickFunc;
    public bool clickAllowOnce = false;

    public CustomBlindTutorial customBlind;

    void Awake()
    {
        _transform = transform;
        SetAlpha(0f);
        SetSize(0, 0);
    }

    public void SetActiveDefulatBlind(bool isActive, int depth)
    {
        defaultBlindRoot.SetActive(isActive);
        _panel.depth = depth;
    }

    public void SetDepth(int in_depth)
    {
        _textureCenter.depth = in_depth;
        _textureUp.depth = in_depth;
        _textureDown.depth = in_depth;
        _textureRight.depth = in_depth;
        _textureLeft.depth = in_depth;
    }

    public void SetAlpha(float in_alpha)
    {
        Color color = new Color(0f, 0f, 0f, Mathf.Clamp(in_alpha,0.002f,1f));
        _textureCenter.color = color;
        _textureUp.color = color;
        _textureDown.color = color;
        _textureRight.color = color;
        _textureLeft.color = color;
    }

    public void SetSizeCollider(int in_x, int in_y)
    {
        _colliderUp.size = new Vector3((float)in_x, (float)2000, 1f);
        _colliderUp.transform.localPosition = Vector3.up * in_y * 0.5f;

        _colliderDown.size = new Vector3((float)in_x, (float)2000, 1f);
        _colliderDown.transform.localPosition = Vector3.down * in_y * 0.5f;


        _colliderRight.transform.localPosition = Vector3.right * in_x * 0.5f;
        _colliderLeft.transform.localPosition = Vector3.left * in_x * 0.5f;
    }

    public void SetSize(int in_x, int in_y)
    {
        int addSize = 0;

        _textureCenter.SetDimensions(in_x + addSize, in_y + addSize);

        _textureUp.cachedTransform.localPosition = Vector3.up * in_y * 0.5f;
        if (in_x == 0)
            _textureUp.SetDimensions(0, 0);
        else
            _textureUp.SetDimensions(in_x + addSize, maxSize);

        _textureDown.cachedTransform.localPosition = Vector3.down * in_y * 0.5f;
        if (in_x == 0)
            _textureDown.SetDimensions(0, 0);
        else
            _textureDown.SetDimensions(in_x + addSize, maxSize);


        _textureRight.cachedTransform.localPosition = Vector3.right * in_x * 0.5f;
        _textureLeft.cachedTransform.localPosition = Vector3.left * in_x * 0.5f;

    }

    public void SetSizeCollider_ClickFunc(int in_x, int in_y, Method.FunctionVoid func)
    {
        _colliderUp.size = new Vector3((float)in_x, (float)2000, 1f);
        _colliderUp.transform.localPosition = Vector3.up * in_y * 0.5f;

        _colliderDown.size = new Vector3((float)in_x, (float)2000, 1f);
        _colliderDown.transform.localPosition = Vector3.down * in_y * 0.5f;


        _colliderRight.transform.localPosition = Vector3.right * in_x * 0.5f;
        _colliderLeft.transform.localPosition = Vector3.left * in_x * 0.5f;

        _colliderClick.enabled = true;
        _colliderClick.size = new Vector3(in_x, in_y);
        _clickFunc = func;
        clickAllowOnce = true;
    }

    public void DisableClickFunc()
    {
        _clickFunc = null;
        _colliderClick.enabled = false;
    }

    public void OnClick()
    {
        if( _clickFunc != null )
        {
            _clickFunc();
            if( clickAllowOnce )
            {
                DisableClickFunc();
            }
        }
    }

    #region 커스텀 블라인드
    /// <summary>
    ///  커스텀 블라인드 텍스쳐를 생성해주는 함수
    ///  width, height : 화면에 출력될 텍스쳐 사이즈
    ///   textureSize : 생성할 텍스쳐 사이즈(2의 배수)
    /// </summary>
    public void MakeCustomBlindTexture(int width, int height, int textureSize = 512)
    {
        SetActiveCustomBlind(true);
        customBlind.MakeBlindTexture(width, height, textureSize);
    }

    public void SetActiveCustomBlind(bool isActive)
    {
        customBlind.gameObject.SetActive(isActive);
    }
    #endregion
}
