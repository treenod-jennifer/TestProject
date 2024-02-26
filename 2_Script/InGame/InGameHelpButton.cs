using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class InGameHelpButton : MonoBehaviour 
{
    private int MAX_ADDTURN_COUNT = 99999;

    [SerializeField]
    private GameObject helpButtonRoot;

    [SerializeField]
    private GameObject failDataButtonRoot;

    //접기
    [SerializeField]
    private GameObject showButtonObj;
    [SerializeField]
    private Text showButtonText;

    //버튼
    [SerializeField]
    private GameObject addTurnObj;
    [SerializeField]
    private GameObject minTurnObj;
    [SerializeField]
    private GameObject button_ExtObj;

    //턴추가텍스트
    [SerializeField]
    private Text addTurnText;
    
    // 점수 텍스트
    [SerializeField]
    private Text scoreText;
    
    // FailData 체크 텍스트
    [SerializeField]
    private Text failDataCheckText;

    //접기
    private float showButtonPos_Y = 0f;
    private float showButtonOffset_Y = 270f;
    private bool isFold = false;

    //턴 추가
    private int addTurnCount = 0;

    public void Start()
    {
        showButtonPos_Y = showButtonObj.transform.localPosition.y;
        InitHelpButton_LevelServer();
        failDataButtonRoot.SetActive(Global._instance.showFailDataBTN);
    }

    //레벨 서버에서는 레벨팀에서 실제 사용하는 치트만 남겨둠.
    private void InitHelpButton_LevelServer()
    {
        if (NetworkSettings.Instance.serverTarget != NetworkSettings.ServerTargets.LevelTeamServer)
            return;
        button_ExtObj.SetActive(false);
        minTurnObj.SetActive(false);
        addTurnObj.transform.localPosition = new Vector3(0f, 560f, 0f);
        showButtonObj.transform.localPosition += new Vector3(0f, 120f, 0f);
        showButtonPos_Y = showButtonObj.transform.localPosition.y;
        showButtonOffset_Y = 150f;
        MAX_ADDTURN_COUNT = 2;
    }

    public void StageClear()
    {
        int score = 0;
        bool isInputScore = int.TryParse(scoreText.text, out score);
        ManagerBlock.instance.score = isInputScore ? score : 1000000;
        GameManager.instance.StageClear();
    }

    public void StageFail()
    {
        GameManager.instance.StageFail();
    }

    public void MakeRandomBomb()
    {
        ManagerBlock.instance.MakeBombRandom(PosHelper.GetRandomBlock());
    }

    public void MakeBomb()
    {
        ManagerBlock.instance.MakeBombRandom(PosHelper.GetRandomBlock(), BlockBombType.BOMB);
    }

    public void MakeLineBomb()
    {
        BlockBombType bombType = BlockBombType.LINE_H;
        int randomLine = Random.Range(0, 2);

        if (randomLine == 0) bombType = BlockBombType.LINE_V;
        else bombType = BlockBombType.LINE_H;

        ManagerBlock.instance.MakeBombRandom(PosHelper.GetRandomBlock(), bombType);
    }
    public void MakeRainbowBomb()
    {
        ManagerBlock.instance.MakeBombRandom(PosHelper.GetRandomBlock(), BlockBombType.RAINBOW);
    }

    public void ClearWave()
    {
        AdventureManager.instance.ClearWave();
    }

    public void PangAnimal()
    {
        AdventureManager.instance.PangAnimal();
    }

    public void PangEnemy()
    {
        AdventureManager.instance.PangEnemy();
    }

    public void HealAnimal()
    {
        AdventureManager.instance.HealAnimal();
    }

    public void ChangeAdventureOrigin()
    {
        GameManager.instance.ChangeAdventureMode(AdventureMode.ORIGIN);
    }
    public void ChangeAdventureCurrent()
    {
        GameManager.instance.ChangeAdventureMode(AdventureMode.CURRENT);
    }
    public void ChangeAdventureNormal()
    {
        GameManager.instance.ChangeAdventureMode(AdventureMode.NORMAL);
    }

    public void ChangeBombMode()
    {
        AdventureManager.instance.ChangeBombMode(1);
    }

    public void ChangeRainbowMode()
    {
        AdventureManager.instance.ChangeBombMode(0);
    }

    public void SkipStageClear()
    {
        Global.timeScalePuzzle = 1000f;
        GameManager.instance.SkipIngameClear = true;

        GameManager.instance.SetClearBlind(false);
    }

    public void MixBlock()
    {
        ManagerBlock.instance.state = BlockManagrState.STOP;
        StartCoroutine(ManagerBlock.instance.CoMixBlockAtTool());
    }

    public void AddTurn()
    {
        addTurnCount++;
        GameManager.instance.moveCount++;
        GameUIManager.instance.RefreshMove();
        addTurnText.text = "Add\nTurn : "+addTurnCount;

        if (addTurnCount >= MAX_ADDTURN_COUNT)
        {
            addTurnObj.SetActive(false);
        }
    }

    public void MinTurn()
    {
        GameManager.instance.moveCount--;
        GameUIManager.instance.RefreshMove();
    }

    public void ShowButton()
    {
        isFold = !isFold;
        if (isFold == true)
        {
            helpButtonRoot.gameObject.SetActive(false);
            showButtonObj.transform.localPosition += Vector3.up * showButtonOffset_Y;
            showButtonText.text = "▼";
        }
        else
        {
            helpButtonRoot.gameObject.SetActive(true);
            showButtonObj.transform.localPosition += Vector3.down * showButtonOffset_Y;
            showButtonText.text = "▲";
        }
    }

    public void SkillFull()
    {
        for (int i = 0; i < 3; i++)
        {
            if (AdventureManager.instance.AnimalLIst[i].GetState() == ANIMAL_STATE.DEAD)
                continue;

            if (AdventureManager.instance.AnimalLIst[i].skillItem == null)
                continue;

            AdventureManager.instance.AnimalLIst[i].skillItem.AddSkillPointUsingPercent(1.0f);
        }
    }

    private class FailCountData
    {
        public List<string> failData;
        private const int MAX_DATA_SIZE = 300;

        public FailCountData()
        {
            failData = new List<string>(MAX_DATA_SIZE);
        }
    }

    public void CheckSpotDia()
    {
        if (ServerRepos.UserSpotDiaShow == null)
        {
            failDataCheckText.text = "FAIL";
            return;
        }
        
        if (ServerRepos.UserSpotDiaShow.isShowed == 1)
        {
            failDataCheckText.text = "SEEN";
            return;
        }

        var failCount = ServerRepos.UserSpotDiaShow.failCount;
        var failTargetCount = ServerRepos.UserSpotDiaShow.failTargetCount;
        // 컨티뉴 팝업이나 실패 팝업이 오픈되어 있을 때는 실패로 간주하고 +1 해서 보여줌
        if (UIPopupContinue._instance != null || (UIPopupFail._instance != null && UIPopupReady._instance == null))
        {
            failCount += 1;
        }

        failDataCheckText.text = $"{failCount}/{failTargetCount}";
    }

    public void CheckNPUSpot() => CheckFailData("FailData_NPU_Spot");
    public void CheckNPUContinue() => CheckFailData("FailData_NPU_AD");

    private void CheckFailData(string str)
    {
        var json = PlayerPrefs.GetString(str);
        var failCountData = JsonConvert.DeserializeObject<FailCountData>(json);

        var count = 0;
        if (json != String.Empty && failCountData != null)
        {
            foreach (var item in failCountData.failData)
                if (item == $"{Global.GameType}_{Global.eventIndex}_{Global.chapterIndex}_{Global.stageIndex}")
                {
                    count++;
                }
        }
        failDataCheckText.text = count.ToString();
    }
}
