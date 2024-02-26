using UnityEngine;

/// <summary>
/// https://forum.unity.com/threads/gui-drop-down-menu.76623/
/// 사용법
/// 1. 클래스 생성 (GUIDropDown dropDown = new GUIDropDown();)
/// 2. GUI 시작 지점에 CheckBlockingShowMenuGUI 호출
/// 3. GUI를 그리는 지점에 OnGUI 호출
/// BeginScrollView 뎁스 등으로 인해 맨 마지막에 그리는 부분 호출
/// </summary>
public class GUIDropDown
{
    private const int height = 20;
    
    private Vector2 scrollViewVector;
    private Rect dropDownRect;
    private string[] menus;

    private int  indexNumber;
    private bool isShowMenu;

    private GUIStyle dropDownStyle;
    public GUIDropDown()
    {
        scrollViewVector = Vector2.zero;
        dropDownRect     = new Rect(125, 50, 125, 300);
        menus            = new[] {"Drop_Down_Menu"};
    }

    public void CheckBlockingShowMenuGUI()
    {
        GUI.enabled = !isShowMenu;
    }

    private void SetUp(Rect rect, int index, string[] texts)
    {
        dropDownRect = rect;
        menus = texts;
        indexNumber = index;
    }

    public int OnGUI(Rect rect, int index, string[] texts)
    {
        GUI.enabled = true;
        
        SetUp(rect, index, texts);
        
        if (GUI.Button(new Rect((dropDownRect.x), dropDownRect.y, dropDownRect.width, height), ""))
        {
            isShowMenu = !isShowMenu;
        }

        if (isShowMenu)
        {
            GUI.Label(new Rect((dropDownRect.x + 5), dropDownRect.y, dropDownRect.width, height), menus[indexNumber]);
            
            var scrollViewRect = new Rect(0, 0, dropDownRect.width, Mathf.Max(dropDownRect.height, (menus.Length * height)));
            scrollViewVector = GUI.BeginScrollView(
                new Rect((dropDownRect.x), (dropDownRect.y + height), dropDownRect.width + 20, dropDownRect.height),
                scrollViewVector,
                scrollViewRect);

            
            GUI.Box(scrollViewRect, "");
            ShowMenus();

            GUI.EndScrollView();
        }
        else
        {
            GUI.Label(new Rect((dropDownRect.x + 5), dropDownRect.y, 300, height), menus[indexNumber]);
        }
        
        CheckHideMenu();
        CheckBlockingShowMenuGUI();
            
        return indexNumber;
    }

    private void CheckHideMenu()
    {
        var e = Event.current;
        if (e.isMouse && e.type == EventType.MouseDown)
        {
            isShowMenu = false;
        }
    }

    private void ShowMenus()
    {
        for (int index = 0; index < menus.Length; index++)
        {
            if (GUI.Button(new Rect(0, (index * height), dropDownRect.width, height), ""))
            {
                isShowMenu  = false;
                indexNumber = index;
            }

            GUI.Label(new Rect(5, (index * height), dropDownRect.width, height), menus[index]);
        }
    }
}