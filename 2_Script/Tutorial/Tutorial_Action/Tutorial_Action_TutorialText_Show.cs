using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 튜토리얼 말풍선 생성
/// </summary>
public class Tutorial_Action_TutorialText_Show : Tutorial_Action
{
    public string textKey = "";

    public Tutorial_Action_TutorialText_Show(string key)
    {
        this.textKey = key;
    }

    public void Init(string key)
    {
        this.textKey = key;
    }

    public override void StartAction(System.Action endAction = null)
    {
        ManagerTutorial._instance._current.textBox =
            ManagerTutorial._instance.MakeTextbox(ManagerTutorial._instance.birdType, Global._instance.GetString(textKey), 0.3f);

        //사운드 출력.
        ManagerTutorial._instance.PlayBirdSound();
        endAction.Invoke();
    }
}
