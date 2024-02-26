using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupLobbyGiftInfo : UIPopupBase {
    [SerializeField] private GenericReward reward;
    [SerializeField] private UILabel time;
    [SerializeField] private UIWidget time_BG;
    [SerializeField] private GameObject buttonOK;
    [SerializeField] private GameObject objSpecialLobbyText;

    public void Init(int animalIndex)
    {
        var animalData = ServerContents.AdvAnimals[animalIndex];
        reward.SetReward(animalData.lobby_rewards[0]);

        string temp = Global._instance.GetString("p_lob_c_10");
        temp = temp.Replace("[0]", TimeToString(animalData.lobby_reward_cool_time));
        temp = temp.Replace("[1]", animalData.lobby_rewards[0].value.ToString());
        
        time.text = temp;
        
        time_BG.width = (int)time.printedSize.x + 30;

        if (ManagerCharacter._instance.IsSpecialLobbyChar(animalIndex))
        {
            mainSprite.height = 795;
            buttonOK.transform.localPosition = new Vector3(0f, -710f, 0f);
            objSpecialLobbyText.SetActive(true);
        }
        else
        {
            mainSprite.height = 755;
            buttonOK.transform.localPosition = new Vector3(0f, -666f, 0f);
            objSpecialLobbyText.SetActive(false);
        }
    }

    private string TimeToString(int timeSecond)
    {
        int day = timeSecond / 86400;
        timeSecond = timeSecond % 86400;

        int hour = timeSecond / 3600;
        timeSecond = timeSecond % 3600;

        int minute = timeSecond / 60;
        timeSecond = timeSecond % 60;

        int second = timeSecond;

        string temp = "";

        if (day > 0)
            temp += day.ToString() + Global._instance.GetString("p_lob_c_4");

        if (hour > 0)
            temp += hour.ToString() + Global._instance.GetString("p_lob_c_5");

        if (minute > 0)
            temp += minute.ToString() + Global._instance.GetString("p_lob_c_6");

        if (second > 0)
            temp += second.ToString() + Global._instance.GetString("p_lob_c_7");

        return temp;
    }
}
