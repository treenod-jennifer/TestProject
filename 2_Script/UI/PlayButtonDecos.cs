using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayButtonDecos : MonoBehaviour
{
    public enum DecoFlagIter
    {
        CoinEvent,
        CloverSupply,
        Count
    }
    public enum DecoFlag
    {
        CoinEvent       = 1 << DecoFlagIter.CoinEvent,
        CloverSupply    = 1 << DecoFlagIter.CloverSupply,
    }
    [SerializeField] GameObject[] gameObjects;
    [SerializeField] List<GameObject> activeObj;

    bool stopCoroutine = true;
    Coroutine activeCoroutine = null;
    int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDecoState(int decoFlag)
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        activeObj.Clear();
        for (int i = 0; i < (int)DecoFlagIter.Count; ++i)
        {
            bool a = ((1 << i) & decoFlag) != 0;
            //gameObjects[i]?.SetActive(a);
            gameObjects[i]?.SetActive(false);
            if ( a )
                this.activeObj.Add(gameObjects[i]);
        }

        if( this.gameObject.activeInHierarchy )
            activeCoroutine = StartCoroutine(CoDecoAction());
    }

    IEnumerator CoDecoAction()
    {
        while (true)
        {
            if (activeObj.Count == 0)
                break;
            if(ManagerUI._instance == null)
                yield break;
                
            //메뉴가 열려있다면 코인, 클로버 이벤트 버블 알람이 뜨지 않습니다.
            if (ManagerUI._instance.IsMenuOpened())
            {
                yield return null;
            }
            else
            {
                index = index % this.activeObj.Count;

                activeObj[index].gameObject.SetActive(true);

                var spr = activeObj[index].GetComponentInChildren<UISprite>();
                DOTween.ToAlpha(() => spr.color, color => spr.color = color, 1f, 0.4f);

                //코인, 클로버 이벤트 버블 알림이 켜져있을 때 메뉴 UI가 오픈되면 바로 꺼집니다.
                float time = 0;
                while (time < 6)
                {
                    if(ManagerUI._instance.IsMenuOpened())
                        break;
                    yield return new WaitForSeconds(0.1f);
                    time+= 0.1f;
                }
                
                DOTween.ToAlpha(() => spr.color, color => spr.color = color, 0f, 0.5f);
                activeObj[index].transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                yield return new WaitForSeconds(2.0f);
            
                activeObj[index].gameObject.SetActive(false);
                index++;
            }
        }
        yield break;
    }
}
