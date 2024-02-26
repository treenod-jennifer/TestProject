using System.Collections;
using UnityEngine;

public class UIButtonAdventureEvent : MonoBehaviour
{
    [SerializeField] private GameObject button;
    [SerializeField] private UIUrlTexture mainTexture;
    [SerializeField] private UILabel timeText;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => { return ManagerAdventure.Active; });
        SetButton();
    }

    public void OnClickAdventureEvent()
    {
        OpenEvent();
    }

    private void SetButton()
    {
        if (!ManagerAdventure.EventData.GetActiveEvent_AdventureEvent())
        {
            button.SetActive(false);
            return;
        }

        int eventIndex = ManagerAdventure.EventData.GetAdvEventIndex();
        long end_ts = ManagerAdventure.EventData.GetAdvEventEndTS();

        SetTime(end_ts);

        string fileName = ManagerAdventure.EventData.GetAdvEventButtonName(eventIndex);
        if (HashChecker.GetHash("IconEvent/", fileName + ".png") != null)
        {
            CoSetButton(fileName);
        }

        button.SetActive(true);
    }

    private void SetTime(long end_ts)
    {
        StartCoroutine(CoTime(end_ts));
    }

    private IEnumerator CoTime(long end_ts)
    {
        while (Global.LeftTime(end_ts) > 0)
        {
            timeText.text = Global.GetLeftTimeText(end_ts);
            yield return new WaitForSeconds(1.0f);
        }

        button.SetActive(false);
    }

    private void CoSetButton(string fileName)
    {
        mainTexture.SuccessEvent += () => mainTexture.MakePixelPerfect();
        mainTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", fileName);
        mainTexture.gameObject.SetActive(true);
    }

    public static void OpenEvent()
    {
        if (!ManagerAdventure.Active)
        {
            return;
        }

        if (!ManagerAdventure.User.IsCleared(1, 5))
        {
            OpenPopupNonPlay();
            return;
        }

        ManagerUI._instance.OpenPopup<UIPopUpStageAdventureEventReady>();
    }

    private static void OpenPopupNonPlay()
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();

        popupSystem.SetButtonText(0, Global._instance.GetString("btn_1"));

        popupSystem.InitSystemPopUp
        (
            Global._instance.GetString("p_t_4"),
            Global._instance.GetString("n_adv_2"),
            false,
            null
        );

        popupSystem.SortOrderSetting(ManagerUI._instance._popupList[ManagerUI._instance._popupList.Count - 1].uiPanel.useSortingOrder);
    }
}
