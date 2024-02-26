using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class UIItemStampEdit : MonoBehaviour
{
    public UIItemStamp itemStamp;

    public Transform buttonEdit;
    public GameObject leftTimeObj;
    public GameObject newIConObj;
    public UILabel leftTimeText;
    // CaptureComplete SignindicateDraw --------
    private Rect drawScaleRect;
    private Texture drawTexture;
    //------------------------------------------
    // 파일 저장시 파일이름
    private byte[] byteTexture = null;

    private bool isSetUpStampData = false;
    private int dataIndex = -1;
    private int listIndex = -1;

    private Stamp originData;
    private Action<string, int, int> callbackHandler;

    public void InitData( Stamp in_data, int dataIndex, int in_listIndex, bool bNew, Action<string, int, int> callbackHandler )
    {
        this.itemStamp.InitData( in_data ); // 수정할 데이터 세팅
        // 리셋할 데이터 세팅
        this.originData = in_data;
        this.originData.transform.parent = this.gameObject.transform;

        this.dataIndex = dataIndex;
        this.listIndex = in_listIndex;
        this.callbackHandler = callbackHandler;
        this.isSetUpStampData = true;
        InitStampLeftTime();
        newIConObj.SetActive(bNew);
    }

    /// <summary>
    /// 텍스트 수정 이벤트
    /// </summary>
    public void OnClickBtnEdit ()
    {
        // 수정 가능한 에디터 창 오픈
        ManagerUI._instance.OpenPopopSendItemToSocial( this.itemStamp, this.originData, this.SetStampScreenShotData, this.SetStampResetData, ServerRepos.UserStamps[listIndex].index);
    }

    public GameObject GetBtnEdit()
    {
        return buttonEdit.gameObject;
    }
  
    public void SetStampScreenShotData (UIItemStamp stampItem)
    {
        this.itemStamp.InitData( stampItem.data );
        this.isSetUpStampData = true;

        StampJsonData saveData = new StampJsonData( stampItem.data );
        string jsonStr = JsonUtility.ToJson( saveData );
        PlayerPrefs.SetString( string.Format( "stamp_{0}", this.dataIndex ) , jsonStr );
    }

    public void SetStampResetData (Stamp data)
    {
        this.itemStamp.InitData( data );
        this.callbackHandler(string.Format("stamp_{0}", this.dataIndex), this.dataIndex, this.listIndex);
    }

    private void ClosePopup ()
    {
        // 팝업 UI 없앤다
        ManagerUI._instance.ClosePopUpUI();
    }

    private void InitStampLeftTime()
    {
        //이벤트 시간이 없으면 시간 부분 표시안함.
        if (originData.startTime == 0 && originData.endTime == 0)
        {
            leftTimeObj.SetActive(false);
        }
        else
        {
            if (originData.startTime < Global.GetTime() && originData.endTime > Global.GetTime())
            {
                leftTimeObj.SetActive(true);
                StartCoroutine(CoStampLeftTime());
            }
        }
    }

    private IEnumerator CoStampLeftTime()
    {
        while (true)
        {
            if (gameObject.activeInHierarchy == false)
                break;
            long leftTime = Global.LeftTime(originData.endTime);
            if (leftTime >= 60)
            {
                leftTimeText.text = Global.GetTimeText_HHMM(originData.endTime);
            }
            else
            {
                leftTimeText.text = Global.GetTimeText_SS(originData.endTime);
            }

            if (leftTime <= 0)
            {
                break;
            }
            yield return new WaitForSeconds(0.2f);
        }

        if (gameObject.activeInHierarchy == true)
        {
            leftTimeText.text = "00:00";
        }
    }
}
