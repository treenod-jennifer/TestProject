using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#region 랭킹계산.
public class GameStarComparer : IComparer<UserData>
{
    public int Compare(UserData a, UserData b)
    {

            return 0;
    }
}
#endregion

public class UIPopupRanking : UIPopupBase
{
    public static UIPopupRanking _instance = null;

    //데이터 전송 다 받았을 때의 콜백.
    public Method.FunctionVoid callbackDataComplete = null;

    public UIPanel scrollPanel;
    public UILabel loadingText;

    List<UserData> _listUserRanks = new List<UserData>();
    List<RankingUIData> _listRankingDatas = new List<RankingUIData>();

    int nMyIndex = -1;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
    new void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }
    public override void OpenPopUp(int _depth)
    {   
        this.StartCoroutine( this.GetFriendData() );
        base.OpenPopUp( _depth );
        scrollPanel.depth = uiPanel.depth + 1;
        CoLoadingText();
    }

    public override void ClosePopUp(float _mainTime = 0.3f, Method.FunctionVoid callback = null)
    {

        base.ClosePopUp(_mainTime, callback);
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는다면 안올림.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            scrollPanel.useSortingOrder = true;
            scrollPanel.sortingOrder = layer + 1;
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    bool isComplete = false;

    IEnumerator GetFriendData()
    {
        UserData myProfile = null;

        this._listRankingDatas.Clear();
        this._listUserRanks.Clear();

        bool isReceivedData = false;

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            MakeFriend();
            myProfile = ManagerData._instance.userData;

            isReceivedData = true;
        }
        else
        {


        }

        while (isReceivedData == false)
        {
            yield return null;
        }

        nMyIndex = -1;
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            this.InitEditorRankingData(myProfile);
        }
        else
        {

                
        yield return null;
        }
    }


    private void OnLoadFriendScore()
    {

    }
    public int GetMyIndx()
    {
        return nMyIndex;
    }

    private void InitEditorRankingData(UserData myProfile)
    {
        if (this._listUserRanks.Count > 0)
        {
            //이전 랭킹의 꽃, 스테이지.
            long flowerCnt = 0;
            int stagePos = 0;

            int rank = 1;

            for (int i = 0; i < this._listUserRanks.Count; i++)
            {
                UserData user = this._listUserRanks[i];

             

            }
            //친구 랭킹 다 돌았는데도 현재 내 데이터가 안들어갔다면(꼴찌) 맨 마지막에 내 데이터 추가.
            if (nMyIndex == -1)
            {
                rank++;
                nMyIndex = this._listUserRanks.Count;
                this._listUserRanks.Add(myProfile);
                RankingUIData cell = new RankingUIData(this._listUserRanks.Count, rank, myProfile);
                this._listRankingDatas.Add(cell);
            }
        }
        else
        {
            this._listUserRanks.Add(myProfile);
            RankingUIData cell = new RankingUIData(0, 0 + 1, myProfile);
            this._listRankingDatas.Add(cell);
        }

        if (callbackDataComplete != null && _listRankingDatas.Count > 0)
        {
            callbackDataComplete();
        }
    }

    private void InitDeviceRankingData(UserData myProfile)
    {


    }
    public RankingUIData GetRankData(int index)
    {
        if (_listRankingDatas.Count <= index || _listRankingDatas[index] == null)
            return null;

        return _listRankingDatas[index];
    }

    void MakeFriend()
    {
        _listUserRanks.Clear();

    }

    GameStarComparer starComparer = new GameStarComparer();

    public int GetUserRanksCount()
    {
        return (_listUserRanks.Count - 1);
    }

    IEnumerator CoLoadingText()
    {
        while (true)
        {
            if (loadingText.gameObject.activeInHierarchy == false)
                break;
            loadingText.spacingX = (int)Mathf.PingPong(Time.time * 10f, 3);
            yield return null;
        }
        yield return null;
    }

    //public void GetGameProfileDataHandler ( SDKGameProfileInfo[] profileData )
    //{
    //    int length = profileData.Length;

    //    for(int i = 0; i < length; i++)
    //    {
    //        Debug.Log( "profileData[i].userKey : " + profileData[i].userKey );
    //        ManagerData._instance._dicUserProfileInfo.Add( profileData[i].userKey, profileData[i] );
    //    }

    //    if( SDKGameProfileManager._instance != null )
    //    { 
    //        DestroyImmediate ( SDKGameProfileManager._instance.gameObject );
    //    }

    //    this.isComplete = true;
    //}
}
