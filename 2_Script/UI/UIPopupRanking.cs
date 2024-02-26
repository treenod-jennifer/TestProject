using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#region 랭킹계산.
public class GameStarComparer : IComparer<UserBase>
{
    public int Compare(UserBase a, UserBase b)
    {
        if (a.Flower < b.Flower)
            return 1;
        else if (a.Flower > b.Flower)
            return -1;
        else if (a.stage > b.stage)
            return -1;
        else if (a.stage < b.stage)
            return 1;
        else
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
    public UILabel[] title;
    public UILabel btnGradeInfoLabel;
    public GameObject cloverTimeEvnetRoot;

    protected List<UserBase> _listUserRanks = new List<UserBase>();
    protected List<RankingUIData> _listRankingDatas = new List<RankingUIData>();

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
        InitText();
        InitCloverTimeEvent();
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

    protected virtual void InitText()
    {
        string titleText = Global._instance.GetString("p_rk_1");
        for (int i = 0; i < title.Length; i++)
        {
            title[i].text = titleText;
        }
        btnGradeInfoLabel.text = Global._instance.GetString("p_e_rk_8");
    }

    private void InitCloverTimeEvent()
    {
        if (ServerRepos.LoginCdn.sendCloverEventVer != 0)
        {
            cloverTimeEvnetRoot.SetActive(true);
        }
        else
        {
            cloverTimeEvnetRoot.SetActive(false);
        }
    }

    bool isComplete = false;

    IEnumerator GetFriendData()
    {
        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        this._listRankingDatas.Clear();
        this._listUserRanks.Clear();

        bool isReceivedData = false;

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            MakeFriend();
            isReceivedData = true;
        }
        else
        {
            this._listUserRanks = new List<UserBase>(SDKGameProfileManager._instance.GetAllPlayingFriendList());
            this._listUserRanks.Sort(starComparer);
            
            isReceivedData = true;
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
            List<string> userKeys = SDKGameProfileManager._instance.GetAllPlayingFriendKeys();
            userKeys.Add( myProfile._userKey );
            yield return SDKGameProfileManager._instance.GetAllProfileList
            ( 
                (List<Profile_PION> profileData) => 
                {
                    int length = profileData.Count;

                    for ( int i = 0; i < length; i++ )
                    {
                        SDKGameProfileManager._instance.AddUsersProfile(profileData[i].userKey, profileData[i]);
                    }

                    this.InitDeviceRankingData(myProfile);

                    OnDataComplete();
                 },  
                userKeys.ToArray() 
            );
                
            yield return null;
        }
    }

    protected virtual void OnDataComplete()
    {
        if (_listRankingDatas.Count == 0) return;

        callbackDataComplete?.Invoke();
        loadingText.gameObject.SetActive(false);
    }
    
    public int GetMyIndx()
    {
        return nMyIndex;
    }

    private void InitEditorRankingData(UserBase myProfile)
    {
        if (this._listUserRanks.Count > 0)
        {
            //이전 랭킹의 꽃, 스테이지.
            long flowerCnt = 0;
            int stagePos = 0;

            int rank = 1;

            for (int i = 0; i < this._listUserRanks.Count; i++)
            {
                UserBase user = this._listUserRanks[i];

                //현재 내 데이터와 비교.
                if ( user.Flower < myProfile.Flower ||
                        user.Flower == myProfile.Flower &&
                        user.stage <= myProfile.stage )
                {
                    if (nMyIndex == -1)
                    {
                        nMyIndex = i;
                        this._listUserRanks.Insert(i, myProfile);
                    }
                }

                //이전 랭킹 데이터와 비교.
                if (this._listUserRanks[i].Flower < flowerCnt ||
                    (this._listUserRanks[i].Flower == flowerCnt && this._listUserRanks[i].stage < stagePos))
                {
                    rank++;
                }
                flowerCnt = this._listUserRanks[i].Flower;
                stagePos = ( int ) user.stage;

                RankingUIData cell = new RankingUIData(i, rank, this._listUserRanks[i]);
                this._listRankingDatas.Add(cell);

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

        OnDataComplete();
    }

    private void InitDeviceRankingData(UserSelf myProfile)
    {
        //이전 랭킹의 꽃, 스테이지.
        long flowerCnt = 0;
        int stagePos = 0;
        int rank = 1;
        int stageBackup = myProfile.stage;
        Profile_PION myProfileData = myProfile.GetPionProfile();
        if (myProfileData != null )
        {
            myProfile.SetPionProfile(myProfileData);
            myProfileData.profile.SetSDKGameProfileRequestData(myProfile);
            myProfile.SetStage(stageBackup);
            myProfileData.profile.Log();
        }

        // 친구들만 있는 목록에 내 프로필 밀어넣기
        for (int i = 0; i < this._listUserRanks.Count; i++)
        {
            int friendFlower = this._listUserRanks[i].Flower;
            var userBase = _listUserRanks[i];

            //현재 내 데이터와 비교.
            if (userBase.Flower < myProfile.Flower ||
                userBase.Flower == myProfile.Flower &&
                userBase.stage <= myProfile.stage)
            {
                if (nMyIndex == -1)
                {
                    nMyIndex = i;
                    this._listUserRanks.Insert(i, myProfile);
                    break;
                }
            }
        }

        if (nMyIndex == -1) //친구 랭킹 다 돌았는데도 현재 내 데이터가 안들어갔다면(꼴찌) 맨 마지막에 내 데이터 추가.
        {
            nMyIndex = this._listUserRanks.Count;
            this._listUserRanks.Add(myProfile);
        }

        this._listUserRanks.Sort(starComparer);
        
        // 랭킹 프로필 데이터
        for (int i = 0; i < this._listUserRanks.Count; i++)
        {
            if(SDKGameProfileManager._instance.TryGetPIONProfile(this._listUserRanks[i]._userKey, out Profile_PION userProfileData))
            {
                UserBase user = this._listUserRanks[i];
                // 프로필 데이터 재설정
                user.SetPionProfile(userProfileData);
                userProfileData.profile.SetSDKGameProfileRequestData(user);

                //이전 랭킹 데이터와 비교.
                if (this._listUserRanks[i].Flower < flowerCnt ||
                    this._listUserRanks[i].Flower == flowerCnt && (int)userProfileData.profile.stage < stagePos)
                {
                    rank++;
                }
                flowerCnt = this._listUserRanks[i].Flower;
                stagePos = (int)userProfileData.profile.stage;

                RankingUIData cell = new RankingUIData(i, rank, _listUserRanks[i]);
                this._listRankingDatas.Add(cell);
            }
        }

        // 위에 루프중 DB에서 받은 스테이지가 트라이던트 프로필상 스테이지로 덮여버렸을 가능성이 높으므로 다시 롤백처리
        myProfile.SetStage(stageBackup);
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
        {
            int noCount = 1;
            //친구정보 생성
            for (int i = 1; i < 60; i++)
            {
                if (i % 2 == 0)
                    continue;
                for (int d = 0; d < 3; d++)
                {
                    UserBase fp = new UserBase();
                    var p = new ServiceSDK.UserProfileData();
                    //._profile.userKey = fp._profile.name;
                    p.name = "TOMATO" + i;
                    p.pictureUrl = "http://dl-hsp.profile.line.naver.jp/ch/p/u5511fda78f60dba3796f18bccdcd1768?r=0m07eda01a725&size=small";                    
                    p.userKey = "TF" + i;
                    fp.SetTridentProfile(p);
                    //게임을 하는 친구
                    _listUserRanks.Add(fp);
                }
            }
        }

        //친구
        {
            UserBase fp = new UserBase();
            //     fp.mid = "u16a9c7d9225b4e8d73c8bb38a572b664";
            var p = new ServiceSDK.UserProfileData();
            p.name = "석히";            
            p.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116";
            p.userKey = "TF" + fp.DefaultName;
            fp.SetTridentProfile(p);


            _listUserRanks.Add(fp);
        }
        {
            UserBase fp = new UserBase();
            //  fp.mid = "u39ae185351d8cc9e08fa68286f0ce910";
            var p = new ServiceSDK.UserProfileData();
            p.name = "정현천";
            p.pictureUrl = "http://dl.profile.line.naver.jp/0m0573a03972517cc149081cff808b0ce9347e3c54fe62";
            p.userKey = "TF" + fp.DefaultName;
            fp.SetTridentProfile(p);

            _listUserRanks.Add(fp);
        }
        {
            UserBase fp = new UserBase();
            // fp.mid = "u6a0b6f68de914cf9352ab0d54e97865e";
            var p = new ServiceSDK.UserProfileData();
            p.name = "Alice (Miju Lee)";
            p.pictureUrl = "http://dl.profile.line.naver.jp/0m049a36f772512f643e3393b600ec3139ec831dcf912c";
            p.userKey = "TF" + fp.DefaultName;
            fp.SetTridentProfile(p);

            _listUserRanks.Add(fp);
        }
        {
            UserBase fp = new UserBase();
            //  fp.mid = "u79b5aa7f6f88d3c2bb12fcf10ab75cbc";
            var p = new ServiceSDK.UserProfileData();
            p.name = "김재영";
            
            p.pictureUrl = "http://dl.profile.line.naver.jp/0m059fdf8672513cdafb2422af9fd7bf01a0a759ef4bce";
            p.userKey = "TF" + fp.DefaultName;
            fp.SetTridentProfile(p);

            _listUserRanks.Add(fp);
        }
        {
            UserBase fp = new UserBase();
            //  fp.mid = "u9ec3c16af9d7e3384a36e277076de81e";
            var p = new ServiceSDK.UserProfileData();
            p.name = "김경화 carrie";            
            p.pictureUrl = "http://dl.profile.line.naver.jp/0m05eb2c6f7251dbac9031b4f0af2d4e25be215435ff6c";
            p.userKey = "TF" + fp.DefaultName;
            fp.SetTridentProfile(p);

            _listUserRanks.Add(fp);
        }
        {
            UserBase fp = new UserBase();
            //  fp.mid = "ub0949d3bb38e47eb7c7759eb7ea8acf1";
            var p = new ServiceSDK.UserProfileData();
            p.name = "박톰";
            p.pictureUrl = "http://dl.profile.line.naver.jp/0m056d63ba7251f5fbcc60fea90bfe786405bd2df0bb83";
            p.userKey = "TF" + fp.DefaultName;
            fp.SetTridentProfile(p);

            _listUserRanks.Add(fp);
        }
        {
            UserBase fp = new UserBase();
            //   fp.mid = "uee645202e7cb6bf036654565b6b78870";
            var p = new ServiceSDK.UserProfileData();
            p.name = "Yuri Jeong";
            p.pictureUrl = "http://dl.profile.line.naver.jp/0m05f1d4e57251ab0b0359aaec27e532761933e5ef9f02";
            p.userKey = "TF" + fp.DefaultName;
            fp.SetTridentProfile(p);

            _listUserRanks.Add(fp);
        }
        {
            UserBase fp = new UserBase();
            // fp.mid = "ufbe7e86c65153b40b86972371e4c8e93";
            var p = new ServiceSDK.UserProfileData();
            p.name = "SunJu Kim Doreen";
            p.pictureUrl = "http://dl.profile.line.naver.jp/0m05ebef5d7251be88b93728327755bc7a2e614d1499e1";
            p.userKey = "TF" + fp.DefaultName;
            fp.SetTridentProfile(p);

            _listUserRanks.Add(fp);
        }
        _listUserRanks.Sort(starComparer);
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

    private void OnClickBtnUserGradeInfo()
    {
        ManagerUI._instance.OpenPopupUserGradeInfo();
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
