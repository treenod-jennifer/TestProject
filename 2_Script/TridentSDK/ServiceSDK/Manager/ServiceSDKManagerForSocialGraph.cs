using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Trident;
using JsonFx.Json;

namespace ServiceSDK
{
    public class UserProfileData
    {
        
        public int recommendOrderNo = 0;    //게임등록 안한 유저중 추천유저는 recommendOrderNo 순으로 정렬
        public string userKey;              // 게임에 등록한 유저일경우 userKey가존재함, providerKey는 존재하지 않음
        public string name;
        public string providerKey;          // 게임에 등록하지 않은유저일경우 userKey존재하지 않음, providerKey 존재함
        public string pictureUrl;

        public Trident.AuthProvider providerId;

        public UserProfileData()
        {
        }

        public UserProfileData( UserProfile data )
        {
            this.recommendOrderNo = 0;
            this.userKey = data.getUserKey();
            this.name = Global.ClipString(data.getDisplayName());
            this.pictureUrl = data.getPictureUrl();
            this.providerId = data.getProviderId();
            this.providerKey = data.getProviderKey();
        }

        public void InitProfileDummyData ()
        {
            this.userKey = "T0CG00000003";
            this.name = "김성훈  (에반)";
            this.pictureUrl = "http://dl.profile.line-cdn.net/0m07eda01a7251107eacfcc24797b67d7c44ea9e95001a";
            this.providerId = Trident.AuthProvider.AuthProviderFirst;
            this.providerKey = "u5511fda78f60dba3796f18bccdcd1768";
        }
    }

    public partial class ServiceSDKManager
    {
        private const int FRIEND_LIST_READ_COUNT = 50; // 친구리스트 가져올때 한번에 가져올수 있는 최대 갯수
        
        #region ======== Public Method ================================================================================================

        /// <summary>
        /// 유저 프로필 정보 가져오기
        /// </summary>
        /// <param name="onComplete"></param>
        public void GetMyProfile(Action<bool, UserProfileData> onComplete)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK
            return;
#endif

            GraphService graphService = ServiceManager.getInstance().getService<GraphService>();

            if (graphService == null)
            {
                Extension.PokoLog.Log("============Graph Service Instance is null");
                DelegateHelper.SafeCall(onComplete, false, null);
                return;
            }

            graphService.getProfile((isSuccess, userProfile, error) =>
            {
                Extension.PokoLog.Log("========get Profile result : " + isSuccess);
                if (!isSuccess)
                {
                    Extension.PokoLog.Log("========Error[" + error.getCode() + "]: " + error.getMessage());
                }

                UserProfileData profile = new UserProfileData(userProfile);
                DelegateHelper.SafeCall(onComplete, isSuccess, profile);
            });
        }

        /// <summary>
        /// 게임에 등록한 친구들 프로필 정보 가져오기
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="onComplete"></param>
        public void GetGameFriends(int offset, Action<bool, int, List<UserProfileData>> onComplete)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK
            return;
#endif

            GraphService graphService = ServiceManager.getInstance().getService<GraphService>();

            if (graphService == null)
            {
                Extension.PokoLog.Log("============Graph Service Instance is null");
                DelegateHelper.SafeCall(onComplete, false, 0, null);
                return;
            }

            graphService.getGameFriends((isSuccess, graphProfileResponse, error) =>
                {
                    Extension.PokoLog.Log("========get game friend result : " + isSuccess);
                    if (!isSuccess)
                    {
                        Extension.PokoLog.Log("========Error[" + error.getCode() + "]: " + error.getMessage());
                    }

                    List<UserProfileData> profileList = this.ConvertProfileList(graphProfileResponse.userProfiles);
                    DelegateHelper.SafeCall(onComplete, isSuccess, graphProfileResponse.total, profileList);
                },
                offset,
                FRIEND_LIST_READ_COUNT
            );
        }

        /// <summary>
        /// 게임에 등록하지 않은 친구들 프로필 정보 가져오기
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="onComplete"></param>
        public void GetNonGameFriends(int offset, Action<bool, int, List<UserProfileData>> onComplete)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK
            return;
#endif

            GraphService graphService = ServiceManager.getInstance().getService<GraphService>();

            if (graphService == null)
            {
                Extension.PokoLog.Log("============Graph Service Instance is null");
                DelegateHelper.SafeCall(onComplete, false, 0, null);
                return;
            }

            graphService.getNonGameFriends((isSuccess, graphProfileResponse, error) =>
                {
                    Extension.PokoLog.Log("========get non game friend result : " + isSuccess);
                    if (!isSuccess)
                    {
                        Extension.PokoLog.Log("========Error[" + error.getCode() + "]: " + error.getMessage());
                    }

                    List<UserProfileData> profileList = this.ConvertProfileList(graphProfileResponse.userProfiles);
                    DelegateHelper.SafeCall(onComplete, isSuccess, graphProfileResponse.total, profileList);
                },
                offset,
                FRIEND_LIST_READ_COUNT
            );
        }

        /// <summary>
        /// 게임에 등록되지 않은 추천 친구가져오기 (API 호출)
        /// </summary>
        /// <param name="onComplete"></param>
        public void GetRecommendedNonGameFriends(Action<bool, List<string>> onComplete)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK
            DelegateHelper.SafeCall(onComplete, true, null);
            return;
#endif
            GetRecommendedNonGameFriendsProc(onComplete);
        }
        
        private void GetRecommendedNonGameFriendsProc(Action<bool, List<string>> onComplete)
        {
            ServerAPI.GetRecommendedNonGameFriendProc(ServiceSDKManager.instance.GetAccessToken(), resp =>
            {
                if (resp.IsSuccessTridentAPI)
                {
                    DelegateHelper.SafeCall(onComplete, true, resp.data);
                }
                else
                {
                    DelegateHelper.SafeCall(onComplete, false, null);
                }
            });
        }

        /// <summary>
        /// 친구에게 메세지 보내기
        /// </summary>
        /// <param name="receivers">받을 계정들의 provider key</param>
        /// <param name="content">json 포멧으로된 메세지 내용</param>
        /// <param name="onComplete">완료 콜백</param>
        public void SendMessage(Trident.StringList receivers, string content, Action<bool> onComplete)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK
            DelegateHelper.SafeCall(onComplete, true);
            return;
#endif
            GraphService graphService = ServiceManager.getInstance().getService<GraphService>();

            if (graphService == null)
            {
                Extension.PokoLog.Log("============Graph Service Instance is null");
                DelegateHelper.SafeCall(onComplete, false);
                return;
            }

            graphService.sendMessage(receivers, content, (bool isSuccess, Error error) =>
            {
                Extension.PokoLog.Log("========send Message result : " + isSuccess);
                if (!isSuccess)
                {
                    Extension.PokoLog.Log("========Error[" + error.getCode() + "]: " + error.getMessage());
                }

                DelegateHelper.SafeCall(onComplete, isSuccess);
            });
        }
        
        public string GetMessageContent(string templateId, string name, string message)
        {
            return "{\"templateId\":\"" + templateId + "\",\"textParams\":{\"name\":\"" + name + "\"},\"subTextParams\":{\"subtext\":\"" + message + "\"},\"altTextParams\":{\"alttext\":\"" + message + "\"},\"linkTextParams\":{\"lt_p\":\"link\"},\"aLinkUriParams\":{\"android_link_uri\":\"android_uri\"},\"iLinkUriParams\":{\"ios_link_uri\":\"ios_uri\"},\"linkUriParams\":{\"link_uri\": \"uri\"}}";
        }

        #endregion

        #region ======== Private Method ================================================================================================
        
        // Trident의 UserProfile값을 ServiceSDK의 ProfileData값으로 변경
        private List<ServiceSDK.UserProfileData> ConvertProfileList(Trident.UserProfileList tridentProfileList)
        {
            if (tridentProfileList == null)
            {
                return null;
            }
            List<ServiceSDK.UserProfileData> profileList = new List<ServiceSDK.UserProfileData>();

            int listCount = tridentProfileList.Count;

            for (int i = 0; i < listCount; i++)
            {
                profileList.Add(new UserProfileData(tridentProfileList[i]));
            }

            return profileList;
        }
        
        #endregion
    }
}