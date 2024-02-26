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
    private int[]   count = new int[3];
    private int     moveCount = 3;
    private bool    bStop = false;

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
  /*  public IEnumerator CoSetMaterialIcon(int mCount)
    {
        bStop = true;
        SetCount(mCount);
        mainSprite.color = new Color(1f, 1f, 1f, 0f);
        DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, 0.15f);

        yield return new WaitForSeconds(0.15f);
        bStop = false;
        StartCoroutine(CoMaterialCountUp());
    }*/
    public void FirstReset(int mCount)
    {
        _timer = 5f;

        materialCount = 0;
       // for (int i = 0; i < ServerRepos.UserMaterials.Count; i++)
        //    materialCount += ServerRepos.UserMaterials[i].count;
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
    /*  private void SetCount(int mCount)
      {
          if (mCount == 0)
              return;

          materialCount = 0;
          for (int i = 0; i < ServerRepos.UserMaterials.Count; i++)
          {
              materialCount += ServerRepos.UserMaterials[i].count;
          }
          //Debug.Log("총 재료 수 : " + materialCount + " , 얻은 갯수 :" + mCount + ", 표시된 첫 갯수 : " + (materialCount - mCount));
          materialCount -= mCount;

          //움직일 횟수 & 카운트 설정.
          if (mCount == 1)
          {
              moveCount = 1;
              count[0] = 1;
          }
          else if (mCount == 2)
          {
              moveCount = 2;

              count[0] = 1;
              count[1] = 1;
          }
          else
          {
              moveCount = 3;
              int n1 = mCount / 3;
              int n2 = mCount % 3;

              count[0] = n1;
              count[1] = n1;
              count[2] = n1 + n2;
          }
          countText.text = materialCount.ToString();
      }

      IEnumerator CoMaterialCountUp()
      {
          int itemCount = 0;
          float timer = 0f;
          float scaleRatioY = 0.0f;
          materialCount += count[0];
          //Debug.Log("재료 수 증가 : " + materialCount + " , 증가 수 :" + count[0] +", 현재 또잉 카운트 : "+ 0);
          countText.text = materialCount.ToString();

          //재료 아이콘 움직임(또잉또잉).
          while (true)
          {
              if (bStop == true)
                  break;

              if (timer < 0.3f)
              {
                  scaleRatioY = 1.0f - (timer * 0.5f);
              }
              else if (timer >= 0.3f && timer < 0.8f)
              {
                  scaleRatioY = 0.5f + (timer * 0.5f);
              }
              else if (timer >= 0.8f && timer < 1f)
              {
                  scaleRatioY = 1.0f;
              }
              else
              {
                  itemCount++;
                  if (itemCount >= moveCount)
                  {
                      break;
                  }

                  materialCount += count[itemCount];
                  //Debug.Log("재료 수 증가 : " + materialCount + " , 증가 수 :" + count[itemCount] + ", 현재 또잉 카운트 : " + itemCount);
                  countText.text = materialCount.ToString();
                  countText.transform.localScale = Vector3.one;
                  timer = 0f;
              }
              countText.transform.localScale = new Vector3(1f, scaleRatioY, 1f);
              timer += Global.deltaTime * 4f;
              yield return null;
          }
          yield return null;

          //재료 카운트 올라가고 일정 시간 기다림.
          timer = 0f;
          while (true)
          {
              if (bStop == true || timer >= 2.5f)
                  break;
              timer += Global.deltaTime * 1f;
              yield return null;
          }
          yield return null;
          //재료 아이콘 사라지는 연출.
          timer = 0f;
          while (true)
          {
              if (bStop == true || timer >= 1.0f)
                  break;

              mainSprite.color = new Color(1f, 1f, 1f, 1 - timer);
              timer += Global.deltaTime * 5f;
              yield return null;
          }
          yield return null;

          //재료 아이템 버튼 리스트에서는 사라짐.
          ManagerUI._instance.anchorBottomLeft.DestroyLobbyButton(gameObject);

          if (bStop == false)
          {
              countText.transform.localScale = Vector3.zero;
              gameObject.SetActive(false);
              bStop = false;
            
              // TBD: [DESTROY]
              foreach (var child in transform.GetComponentsInChildren<Transform>()) {
                  GameObject.Destroy(child.gameObject);
              }
          }
      }
      */
    
}
