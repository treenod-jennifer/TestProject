using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIButtonMaterial : MonoBehaviour
{
    public static UIButtonMaterial _instance = null;

    public UISprite mainSprite;
    public UILabel countText;

    private int     materialCount = 0;

    [System.NonSerialized]
    public float _timer = 5f;
    float _shakeScale = 0f;

    private void Awake()
    {   
        _instance = this;
    }

    void OnDestroy()
    {
        _instance = null;
        ManagerUI._instance.anchorBottomLeft.DestroyLobbyButton(gameObject);
    }
    void Update()
    {
        _timer -= Global.deltaTime;
        _shakeScale -= Global.deltaTime;

        if (_timer < 0.5f)
            mainSprite.color = new Color(1f, 1f, 1f, _timer);
        else
            mainSprite.color = Color.white;

        if (_shakeScale >0f && _shakeScale <= 1f)
        {
            transform.localScale = new Vector3(1f - Mathf.Sin(_shakeScale * 20f) * 0.3f * _shakeScale, 1f - Mathf.Cos(_shakeScale * 20f) * 0.3f * _shakeScale, 1f);
        }else
            transform.localScale = Vector3.one;

        if (_timer < 0f)
        {
            GameObject.Destroy(gameObject);
        }
    }

    public void FirstReset(int mCount)
    {
        _timer = 5f;

        List<ServerUserMaterial> materialDataList = ServerRepos.UserMaterials;
        int dataCount = materialDataList.Count;
        long nowTime = Global.GetTime();
        materialCount = 0;
        for (int i = 0; i < dataCount; i++)
        {
            if (ManagerData._instance._materialSpawnProgress.ContainsKey(materialDataList[i].index) == true)
            {
                //기간이 지난 한정 재료의 수는 카운트 하지않음.
                if (ManagerData._instance._materialSpawnProgress[materialDataList[i].index] != 0
                    && ManagerData._instance._materialSpawnProgress[materialDataList[i].index] < nowTime)
                    continue;
            }
            materialCount += ServerRepos.UserMaterials[i].count;
        }
        materialCount -= mCount;
        countText.text = materialCount.ToString();

        ManagerSound.AudioPlay(AudioLobby.m_boni_ohho);
    }
    public void Reset()
    {
        _timer = 5f;
        ManagerSound.AudioPlay(AudioLobby.m_boni_ohho);
    }
    public void AddCount()
    {
        materialCount++;
        countText.text = materialCount.ToString();
        _shakeScale = 1f;
    }

    void OnClickBtnMaterial()
    {
        ManagerUI._instance.OpenPopupMaterial();
    }
}
