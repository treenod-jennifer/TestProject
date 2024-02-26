using System.Collections.Generic;

public struct LobbyMenuUtility
{
    //메뉴 UI 상단의 이벤트를 온/오프하는 이벤트 데이터 입니다.
    public enum MenuEventIconType
    {
        invite,
        wakeup,
        ranking
    }

    //메뉴 UI 하단의 newIcon을 온/오프하는 데이터 입니다.
    public enum MenuNewIconType
    {
        mail,
        friend
    }

    public static Dictionary<MenuEventIconType, bool> eventContentInfo;
    public static Dictionary<MenuNewIconType, int> newContentInfo;

    public static void MakeInitializedData()
    {
        eventContentInfo = new Dictionary<MenuEventIconType, bool>();
        newContentInfo = new Dictionary<MenuNewIconType, int>();
        
        eventContentInfo.Add(MenuEventIconType.invite, false);
        eventContentInfo.Add(MenuEventIconType.wakeup, false);
        eventContentInfo.Add(MenuEventIconType.ranking, false);
        
        newContentInfo.Add(MenuNewIconType.mail, 0);
        newContentInfo.Add(MenuNewIconType.friend, 0);
    }
}