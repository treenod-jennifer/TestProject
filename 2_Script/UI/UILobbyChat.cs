using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILobbyChat : UILobbyChat_Base
{
    static public UILobbyChat MakeLobbyChat(Transform in_obj, string in_strChat,float in_dulation = 1f, bool in_show = false)
    {
        if (SceneLoading.IsSceneLoading)
            return null;

        UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();

        ManagerSound.AudioPlay(AudioLobby.Button_02);
        UILobbyChat lobbyChat = NGUITools.AddChild(ManagerUI._instance.gameObject, ManagerUI._instance._objLobbyChat).GetComponent<UILobbyChat>();
        lobbyChat.targetObject = in_obj;
        lobbyChat.LobbyChat.text = in_strChat;
        lobbyChat.LobbyChat.text = in_strChat.Replace("[0]", myProfile.GameName);
        lobbyChat._dulation = in_dulation;
        lobbyChat.showOutScreen = in_show;
        return lobbyChat;
    }

    public override void SetDepthOffset( int offset )
    {
        var org = ManagerUI._instance._objLobbyChat.GetComponent<UILobbyChat>();
        this.ChatBubbleTail.depth = org.ChatBubbleTail.depth + offset;
        this.ChatBubbleBox.depth = org.ChatBubbleBox.depth + offset;
        this.LobbyChat.depth = org.LobbyChat.depth + offset;
    }
}
