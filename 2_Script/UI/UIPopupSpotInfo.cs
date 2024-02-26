using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupSpotInfo : UIPopupBase
{
    public static UIPopupSpotInfo _instance = null;

    [SerializeField] private UIReuseGrid_Generic gridScrollRoot;

    static public List<ServerUserMail> spotMessageData = new List<ServerUserMail>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    protected override void OnDestroy()
    {
        if (_instance == this)
            _instance = null;

        spotMessageData.Clear();

        base.OnDestroy();
    }

    public void InitData()
    {
        List<MessageData> listMessageData = ListMessageData();

        gridScrollRoot.InItGrid(listMessageData.Count, (go, index) =>
        {
            go.GetComponent<UIItemSpotMessage>().UpdateData(listMessageData[index]);
        });
    }

    List<MessageData> ListMessageData()
    {
        List<MessageData> listMessageData = new List<MessageData>();

        MessageData test = new MessageData();

        for (int i = 0; i < spotMessageData.Count; i++)
        {
            listMessageData.Add(
            new MessageData
            (
                spotMessageData[i].index,
                spotMessageData[i].fuid,
                spotMessageData[i].fUserKey,
                spotMessageData[i].ts,
                (RewardType)spotMessageData[i].type,
                spotMessageData[i].mtype,
                spotMessageData[i].value,
                spotMessageData[i].textKey,
                spotMessageData[i].text
                )
            );
        }
        return listMessageData;
    }

}
