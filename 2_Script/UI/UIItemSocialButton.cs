using System;
using UnityEngine;

public class UIItemSocialButton : MonoBehaviour
{
    public static UIItemSocialButton Instance { get; private set; }
    private Action enableAction;

    private void Start()
    {
        Instance = this;

        LoadGameFriendRequest();

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += LoadGameFriendRequest;
    }

    private void OnDestroy()
    {
        Instance = null;

        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= LoadGameFriendRequest;
    }

    private void OnEnable()
    {
        enableAction?.Invoke();
        enableAction = null;
    }

    private void LoadGameFriendRequest()
    {
        if (ManagerUI._instance.iosScreeningEnvironment)
            return;
        
        if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
        {
            LobbyMenuUtility.newContentInfo[LobbyMenuUtility.MenuNewIconType.friend] = 0;
            ManagerUI._instance.UpdateUI();
            return;
        }

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine
            (
                SDKGameProfileManager.CheckIncomingFriendExist
                (
                    (request) =>
                    {
                        LobbyMenuUtility.newContentInfo[LobbyMenuUtility.MenuNewIconType.friend] = request? 1 : 0;
                        ManagerUI._instance.UpdateUI();
                    }
                )
            );
        }
        else
        {
            enableAction = LoadGameFriendRequest;
        }
    }

    private void LoadGameFriendRequest(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if(scene.name == "Lobby")
        {
            LoadGameFriendRequest();
        }
    }
}
