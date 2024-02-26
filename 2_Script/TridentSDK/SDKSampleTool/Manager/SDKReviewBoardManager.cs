using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Networking;

class AcceptAllCertificatesSignedWithASpecificKeyPublicKey : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        X509Certificate2 certificate = new X509Certificate2(certificateData);
        string pk = certificate.GetPublicKeyString();
        return true;
    }
}

[System.Serializable]
public class SDKErrorData
{
    public string code;
    public string message;

    public SDKErrorData ()
    {

    }
}

[System.Serializable]
public class EmotionCountData
{
    public int like = 0;
    public int dislike = 0;
    public int sorrow = 0;

    public EmotionCountData ()
    {
    }

    public EmotionCountData (bool isTest)
    {
        if(isTest)
        {
            this.like = 0;
            this.dislike = 0;
            this.sorrow = 0;
        }
    }
}

[System.Serializable]
public class SDKBoardArticleInfo
{
    public string id;
    public string boardId;
    public string title;
    public string userKey;
    public Profile_PIONCustom profile;
    
    public int viewCount;
    public int commentCount;
    public System.Int64 updatedAt;
    public System.Int64 createdAt;
    public List<string> images = new List<string>();
    public bool isHidden;

    public SDKBoardArticleInfo()
    {
        profile = new Profile_PIONCustom();
    }

    public SDKBoardArticleInfo (string id, string boardId)
    {
        this.id = id;
        this.boardId = boardId;
        this.title = id;
        this.userKey = "ADMIN";
        this.profile = new Profile_PIONCustom();
        this.viewCount = 0;
        this.commentCount = 0;
        this.updatedAt = 1505449812207;
        this.createdAt = 1505449812207;
        this.isHidden = false;
    }
}


[System.Serializable]
public class SDKBoardSpecificArticleInfo
{
    public bool isBookmarked;
    public bool isLiked;
    public bool isHidden;
    public int viewCount;
    public int commentCount;
    public System.Int64 likeCount;
    public System.Int64 updatedAt;
    public System.Int64 createdAt;
    public string body;
    public string emotionId;
    public string hiddenReason;
    public string[] tags;
    public string id;
    public string boardId;
    public string userKey;
    public string title;
    public List<string> images = new List<string>();
    public Profile_PIONCustom profile;
    public EmotionCountData emotionCounts;
}

[System.Serializable]
public class SDKBoardArticleCommentInfo
{
    public string id;
    public string userKey;
    public string body;
    public bool isBest;
    public bool isHidden;
    public bool isLiked;
    public System.Int64 likeCount;
    public System.Int64 createdAt;
    public Profile_PIONCustom profile;

    public SDKBoardArticleCommentInfo()
    {
        this.profile = new Profile_PIONCustom();
    }

    public SDKBoardArticleCommentInfo ( string id, string body, long createAt )
    {
        this.id = id;
        this.userKey = "T0CG0000002E";
        this.profile = new Profile_PIONCustom();
        this.body = body;
        this.isBest = false;
        this.isHidden = false;
        this.createdAt = createAt;
    }
}

[System.Serializable]
public class SDKMeanGrade
{
    public double grade;
}

[System.Serializable]
public class SDKEmotionData
{
    public string id;
    public string type;
    public string count;
    public System.Int64 createdAt;

    public SDKEmotionData ()
    {

    }

    public SDKEmotionData ( EmotionID id, EmotionTYPE type, int count, long createAt )
    {
        this.id = id.ToString();
        this.type = type.ToString();
        this.count = count.ToString();
        this.createdAt = createAt;
    }

}

[System.Serializable]
public class SDKEmotionUpdateData
{
    public string id;
    public string type;
}

[System.Serializable]
public class SDKEmotionDataList
{
    public List<SDKEmotionData> emotionData = new List<SDKEmotionData>();
}

public enum EmotionID
{
    like, 
    dislike,
    sorrow,
    emotionCount
}

public enum EmotionTYPE
{
    POSITIVE,
    NEGATIVE,
    NEUTRAL
}

[System.Serializable]
public class SDKEmotionAdminUpdateData 
{
    public string type;
}

/// <summary>
/// 리뷰보드 Sample 매니저
/// </summary>
public class SDKReviewBoardManager : MonoBehaviour 
{
    public static SDKReviewBoardManager _instance = null;

    private enum LineStatusCode
    {
        NetworkError      = 0,
        NGWordError       = 400,
        DeleteComment     = 404,
        DataAlreadyUpdate = 409,
    }

    void Awake ()
    {
        _instance = this;
    }
    
    //---------------------------------------------------------------------------
    /// <summary>
    /// 전체 코맨트 보드 가져옴
    /// </summary>
    public void GetAllArticles(string boardID, string startArticleId, int limit, Action<SDKBoardArticleInfo[]> callbackHandler)
    {
        ServerAPI.GetAllArticle(boardID, startArticleId, limit, (resp) =>
        {
            if (resp.IsSuccessTridentAPI)
            {
                DelegateHelper.SafeCall(callbackHandler, resp.data);
            }
            else if (resp.IsFailTridentAPI && resp.lineStatusCode == (int)LineStatusCode.NetworkError)
            {
                UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_10"), false, null );
                popupWarning.SetButtonText( 1, Global._instance.GetString("btn_1"));
            }
            else
            {
                OpenPopupWarning(resp.Message);
            }
        });
    }

    /// <summary>
    /// 특정 코맨트 보드 가져옴(이모티콘 갱신할 떄 호출됨)
    /// </summary>
    public void GetArticleByIDOfSpecific (string boardID, string articleID, bool isCountUp, Action<SDKBoardSpecificArticleInfo> callbackHandler)
    {
        ServerAPI.GetArticleById(boardID, articleID, isCountUp, (resp) =>
        {
            if (resp.IsSuccessTridentAPI)
            {
                DelegateHelper.SafeCall(callbackHandler, resp.data);
            }
            else if (resp.IsFailTridentAPI && resp.lineStatusCode == (int)LineStatusCode.NetworkError)
            {
                UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_10"), false, null);
                popupWarning.SetButtonText(1, Global._instance.GetString("btn_1"));
            }
            else
            {
                OpenPopupWarning(resp.Message);
            }
        });
    }
    
    /// <summary>
    /// 특정 보드의 전체 코맨트들을 가져옴
    /// </summary>
    /// <param name="order"> like면 인기 많은 코맨트 순으로 정렬, none이면 최신순 </param>
    /// <param name="offset"> like일 경우 그 다음 코맨트들을 가져오려면 세팅 해주어야 함 </param>
    public void GetCommentsOfArticle (string boardId, string commentId, string articleId, int limit, bool includeBest, string order, string offset,
        Action<SDKBoardArticleCommentInfo[]> callbackSuccess, Action callbackFailedHandler)
    {
        ServerAPI.GetAllComments(boardId, commentId, articleId, limit, includeBest, order, offset, (resp) =>
        {
            if (resp.IsSuccessTridentAPI)
            {
                DelegateHelper.SafeCall(callbackSuccess, resp.data);
            }
            else if (resp.IsFailTridentAPI && resp.lineStatusCode == (int)LineStatusCode.NetworkError)
            {
                UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_10"), false, null);
                popupWarning.SetButtonText(1, Global._instance.GetString("btn_1"));
                DelegateHelper.SafeCall(callbackFailedHandler);
            }
            else
            {
                OpenPopupWarning(resp.Message);
                DelegateHelper.SafeCall(callbackFailedHandler);
            }
        });
    }

    /// <summary>
    /// 해당 보드에 남긴 특정 코멘트를 가져옴(내 코멘트 갱신할 때 사용 - 기존에 작성된 내 코멘트를 찾은 뒤 DeleteComment + AddComment 를 실행)
    /// </summary>
    public void GetSpecificCommentOfArticle(string boardId, string articleId, Action<SDKBoardArticleCommentInfo[]> callbackHandler)
    {
        ServerAPI.GetSpecificComment(boardId, articleId, (resp) =>
        {
            if (resp.IsSuccessTridentAPI)
            {
                DelegateHelper.SafeCall(callbackHandler, resp.data);
            }
            else if (resp.IsFailTridentAPI && resp.lineStatusCode == (int)LineStatusCode.NetworkError)
            {
                UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_10"), false, null);
                popupWarning.SetButtonText(1, Global._instance.GetString("btn_1"));
            }
            else
            {
                OpenPopupWarning(resp.Message);
            }
        });
    }

    /// <summary>
    /// 특정 보드에 코맨트 추가
    /// </summary>
    public void AddCommentOfArticle ( string boardId, string articleId, string comment, Action callbackCompleteHandler, Action callbackAlreadyEnrollReviewHandler, Action callbakNGWordReviewHandler )
    {
        if (ServerRepos.UserInfo.blockReview != 0)
        {   //밴당한 유저가 코멘트를 남긴다면 경고창을 띄우고 코멘트를 남기지 못하게 처리함.
            this.OpenPopupWriteBannedWarning();
            return;
        }
        
        ServerAPI.AddComment(boardId, articleId, comment, (resp) =>
        {
            if (resp.IsSuccessTridentAPI)
            {
                DelegateHelper.SafeCall(callbackCompleteHandler);
            }
            else
            {
                if (resp.lineStatusCode == (int)LineStatusCode.NGWordError)
                { // NgWord가 포함되어 있을 때 콜백(경고 메세지 출력)
                    DelegateHelper.SafeCall(callbakNGWordReviewHandler);
                }
                else if (resp.lineStatusCode == (int)LineStatusCode.DataAlreadyUpdate)
                { //이미 코멘트를 남긴 상태일 때 콜백(데이터를 덮어 씌울지 물어보고 덮어씌운다면 기존 데이터를 제거하고 현재 코멘트로 갱신)
                    DelegateHelper.SafeCall(callbackAlreadyEnrollReviewHandler);
                }
                else if (resp.IsFailTridentAPI && resp.lineStatusCode == (int)LineStatusCode.NetworkError)
                {
                    UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                    popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_10"), false, null);
                    popupWarning.SetButtonText(1, Global._instance.GetString("btn_1"));
                }
                else
                {
                    OpenPopupWarning(resp.Message);
                }
            }
        });
    }

    /// <summary>
    /// 코맨트 좋아요 추가
    /// </summary>
    public void UpdateLikeComment(string boardId, string articleId, string commentId, Action<bool> callbackHandler)
    {
        bool isError = false;
        ServerAPI.LikeComment(boardId, articleId, commentId, (resp) =>
        {
            if (resp.IsSuccessTridentAPI == false)
            {
                isError = true;
                if (resp.lineStatusCode == (int)LineStatusCode.DataAlreadyUpdate)
                {   // 내가 남긴 리뷰에 대해 좋아요를 선택한 상황에서 콜백
                    if (UIPopupSystem._instance == null)
                    {
                        UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                        popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_rv_3"), false, null);
                        popupWarning.SortOrderSetting();
                    }
                }
                else if (resp.lineStatusCode == (int)LineStatusCode.DeleteComment)
                {   // 이미 삭제된 코맨트일 경우 경고 출력
                    if (UIPopupSystem._instance == null)
                    {
                        UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                        popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_rv_5"), false, null);
                        popupWarning.SortOrderSetting();
                    }
                }
                else if (resp.IsFailTridentAPI && resp.lineStatusCode == (int)LineStatusCode.NetworkError)
                {
                    UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_10"), false, null);
                    popupWarning.SetButtonText(1, Global._instance.GetString("btn_1"));
                }
                else
                {
                    OpenPopupWarning(resp.Message);
                }
            }
            
            //콜백 호출(통신에 실패했을 때 true를 반환)
            DelegateHelper.SafeCall(callbackHandler, isError);
        });
    }

    /// <summary>
    /// 내 코맨트를 삭제한다.
    /// </summary>
    public void DeleteMyComment(string boardId, string articleId, string commentId, Action<bool> callbackHandler)
    {
        ServerAPI.DeleteComment(boardId, articleId, commentId, (resp) =>
        {
            if (resp.IsSuccessTridentAPI)
            {
                DelegateHelper.SafeCall(callbackHandler, resp.IsNetworkError);
            }
            else if (resp.IsFailTridentAPI && resp.lineStatusCode == (int)LineStatusCode.NetworkError)
            {
                UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_10"), false, null);
                popupWarning.SetButtonText(1, Global._instance.GetString("btn_1"));
            }
            else
            {
                OpenPopupWarning(resp.Message);
            }
        });
    }

    public void AddReactionOfArticle ( string boardId, string articleId, string emotionId, Action callbackHandler )
    {
        ServerAPI.TakeEmotionalReactionToArticle(boardId, articleId, emotionId, (resp) =>
        {
            if (resp.IsSuccessTridentAPI)
            {
                DelegateHelper.SafeCall(callbackHandler);
            }
            else if (resp.IsFailTridentAPI && resp.lineStatusCode == (int)LineStatusCode.NetworkError)
            {
                UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_10"), false, null);
                popupWarning.SetButtonText(1, Global._instance.GetString("btn_1"));
            }
            else
            {
                OpenPopupWarning(resp.Message);
            }
        });
    }

    public void DeleteReactionOfArticle ( string boardId, string articleId, Action callbackHandler )
    {
        ServerAPI.UndoEmotionalReactionToArticle(boardId, articleId, (resp) =>
        {
            if (resp.IsSuccessTridentAPI)
            {
                DelegateHelper.SafeCall(callbackHandler);
            }
            else if (resp.IsFailTridentAPI && resp.lineStatusCode == (int)LineStatusCode.NetworkError)
            {
                UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_10"), false, null);
                popupWarning.SetButtonText(1, Global._instance.GetString("btn_1"));
            }
            else
            {
                OpenPopupWarning(resp.Message);
            }
        });
    }
    
    private void OpenPopupWarning (string message)
    {
        string textWarning = Global._instance.GetSystemErrorString(message, "");

        if (string.Equals(message, "BOARD_403_5000"))
        {
            OpenPopupWriteBannedWarning();
            return;
        }

        UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
        popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), textWarning, false, null, isError: true);
        popupWarning.SetButtonText(1, Global._instance.GetString("btn_1"));
    }

    private void OpenPopupWriteBannedWarning()
    {
        UIPopupSystem popupWarning = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
        popupWarning.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_er_11"), false, null, isError: true);
        popupWarning.SetButtonText(1, Global._instance.GetString("btn_1"));
    }
}
