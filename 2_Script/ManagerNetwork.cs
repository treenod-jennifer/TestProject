using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 임시로 로컬 네트워크 기능
public delegate void CommandDelegate(int in_result, string in_data);

public class ManagerNetwork : MonoBehaviour {
    public static ManagerNetwork _instance = null;


    void Awake()
    {
        if (_instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        _instance = this;

        //Debug.Log(FixSaveManager.GetString("kim"));
        //FixSaveManager.SetString("kim","sunghoon");

    }
	// Use this for initialization
    void Start()
    {
        
	}
	
	// Update is called once per frame
	void Update () {


   /*     if (Input.GetKeyDown(KeyCode.F1))
            SendMissionClear(1);
        if (Input.GetKeyDown(KeyCode.F2))
            SendMissionClear(2);
        if (Input.GetKeyDown(KeyCode.F3))
            SendMissionClear(3);
        if (Input.GetKeyDown(KeyCode.F4))
            SendMissionClear(4);
        if (Input.GetKeyDown(KeyCode.F5))
            SendMissionClear(5);
        if (Input.GetKeyDown(KeyCode.F6))
            SendMissionClear(6);
        if (Input.GetKeyDown(KeyCode.F7))
            SendMissionClear(7);
        if (Input.GetKeyDown(KeyCode.F8))
            SendMissionClear(8);
        if (Input.GetKeyDown(KeyCode.F9))
            SendMissionClear(9);*/
	}
    
    
    /*public bool SendStart(CommandDelegate in_callback = null)
    {
        if (PlayerPrefs.GetInt("SendStart") == 0)
        {
            Global.join = true;
            SendReset();

            PlayerPrefs.SetInt("Global.clover", 100);
            PlayerPrefs.SetInt("Global.coin", 0);
            PlayerPrefs.SetInt("Global.jewel", 0);
            PlayerPrefs.SetInt("Global.star", 0);
            PlayerPrefs.SetInt("Global.day", 1);
            PlayerPrefs.SetInt("Global.stageIndex", 1);
            PlayerPrefs.SetInt("SendStart", 1);
            PlayerPrefs.SetInt("missionData.state" + 1, (int)TypeMissionState.Active);
            PlayerPrefs.SetInt("missionCheck" + 1, 1);
            PlayerPrefs.SetInt("missionData.clearCount" + 1, 0);
            PlayerPrefs.SetFloat("missionData.clearTime" + 1, 0);

            // 임시 재료 부여
            ManagerData._instance._meterialData[0].count = 15;
            PlayerPrefs.SetInt("_meterialData1" , ManagerData._instance._meterialData[0].count);

            ManagerData._instance._meterialData[1].count = 10;
            PlayerPrefs.SetInt("_meterialData2" , ManagerData._instance._meterialData[1].count);

            ManagerData._instance._meterialData[2].count = 10;
            PlayerPrefs.SetInt("_meterialData3", ManagerData._instance._meterialData[2].count);
        }
        else
        {
            int count = 0;
            while (true)
            {
                count++;
                if (PlayerPrefs.HasKey("missionData.state" + count))
                {
                    ManagerData._instance.missionData[count - 1].state = (TypeMissionState)PlayerPrefs.GetInt("missionData.state" + count);
                    ManagerData._instance.missionData[count - 1].clearCount = PlayerPrefs.GetInt("missionData.clearCount" + count);
                    long.TryParse(PlayerPrefs.GetString("missionData.clearTime" + count), out ManagerData._instance.missionData[count - 1].clearTime);
                }

                if(count>100)
                    break;
            }

            //하우징 획득 상태
            for (int i = 0; i < ManagerData._instance._housingModelData.Count; i++)
            {
                if(PlayerPrefs.HasKey("_housingModelData.active" + (i + 1)))
                    ManagerData._instance._housingModelData[i].active = PlayerPrefs.GetInt("_housingModelData.active" + (i + 1), 0) > 0;
            }


            // 일단 임시로 10가지 찾아봄,,, 서버로 가면 필요없음
            for (int i = 0; i < 10; i++)
            {
                if (PlayerPrefs.HasKey("_housingData" + (i + 1)))
                {
                    HousingUserData data = new HousingUserData();
                    data.index = i + 1;
                    data.selectModel = PlayerPrefs.GetInt("_housingData" + (i + 1));
                    ManagerData._instance._housingData.Add(data);
                }
            }
            // 각 하우징 타입에 따라 선택된 상태
            for (int i = 0; i < ManagerData._instance._housingData.Count; i++)
                ManagerData._instance._housingData[i].selectModel = PlayerPrefs.GetInt("_housingData" + (i + 1),0);


            for (int i = 0; i < ManagerData._instance._stageData.Count; i++)
            {
                int index = i + 1;
                if (PlayerPrefs.HasKey("stageData._star" + index))
                {
                    ManagerData._instance._stageData[i]._flowerLevel = PlayerPrefs.GetInt("stageData._flowerLevel" + index);
                    ManagerData._instance._stageData[i]._score = PlayerPrefs.GetInt("stageData._score" + index);
                    ManagerData._instance._stageData[i]._missionProg1 = PlayerPrefs.GetInt("stageData._missionProg1" + index);
                    ManagerData._instance._stageData[i]._missionProg2 = PlayerPrefs.GetInt("stageData._missionProg2" + index);
                }
            }

            // 재료
            for (int i = 0; i < ManagerData._instance._meterialData.Count; i++)
            {
                int index = i + 1;
                if (PlayerPrefs.HasKey("_meterialData" + index))
                    ManagerData._instance._meterialData[i].count = PlayerPrefs.GetInt("_meterialData" + index);
            }
        }

        Global.day = PlayerPrefs.GetInt("Global.day");
        Global.clover = PlayerPrefs.GetInt("Global.clover");
        Global.coin = PlayerPrefs.GetInt("Global.coin");
        Global.jewel = PlayerPrefs.GetInt("Global.jewel");
        Global.star = PlayerPrefs.GetInt("Global.star");
        Global.stageIndex = PlayerPrefs.GetInt("Global.stageIndex");
        ManagerData._instance.userData.stagePos = Global.stageIndex;

        return true;
    }*/
    public bool SendLoadData(CommandDelegate in_callback = null)
    {


        return true;
    }
    public void SendReset()
    {
        PlayerPrefs.DeleteAll();
        //PlayerPrefs.DeleteKey("SendStart");

    }
  /*  public bool SendMissionClear(int in_index, ref bool in_newDay)
    {
        if (ManagerData._instance.missionData[in_index - 1].state != TypeMissionState.Active)
            return false;
        if (ManagerData._instance.missionData[in_index - 1].needStar > Global.star)
            return false;
        // 시간있는 미션이면 지났는지 어떤지 확인
        if (ManagerData._instance.missionData[in_index - 1].waitTime > 0 && ManagerData._instance.missionData[in_index - 1].clearTime > System.DateTime.Now.Ticks)
            return false;

        // 별까고 미션 클리어 상태로 설정
        Global.star -= ManagerData._instance.missionData[in_index - 1].needStar;
        ManagerUI._instance.UpdateUI();
        

        // 단계 미션 경우
        if (ManagerData._instance.missionData[in_index - 1].stepClear > 1)
        {
            ManagerData._instance.missionData[in_index - 1].clearCount++;
            if(ManagerData._instance.missionData[in_index - 1].clearCount>=ManagerData._instance.missionData[in_index - 1].stepClear)
                ManagerData._instance.missionData[in_index - 1].state = TypeMissionState.Clear;
        }// 시간 미션 경우
        else if (ManagerData._instance.missionData[in_index - 1].waitTime > 0)
        {
            if(ManagerData._instance.missionData[in_index - 1].clearTime == 0)
                ManagerData._instance.missionData[in_index - 1].clearTime = Global.GetTime() + ManagerData._instance.missionData[in_index - 1].waitTime;
            else if (Global.LeftTime(ManagerData._instance.missionData[in_index - 1].clearTime) <= 0)
                ManagerData._instance.missionData[in_index - 1].state = TypeMissionState.Clear;
        }else
            ManagerData._instance.missionData[in_index - 1].state = TypeMissionState.Clear;


        // 새로운 하우징 타입이 있으면 추가
        if (ManagerData._instance.missionData[in_index - 1].housingIndx > 0)
        {
            HousingUserData data = new HousingUserData();
            data.index = ManagerData._instance.missionData[in_index - 1].housingIndx;
            data.selectModel = 0;
            ManagerData._instance._housingData.Add(data);

            PlayerPrefs.SetInt("_housingData" + data.index, data.selectModel);
        }

        PlayerPrefs.SetInt("missionData.clearCount" + in_index, ManagerData._instance.missionData[in_index - 1].clearCount);
        PlayerPrefs.SetInt("missionData.state" + in_index, (int)ManagerData._instance.missionData[in_index - 1].state);
        PlayerPrefs.SetString("missionData.clearTime" + in_index, ManagerData._instance.missionData[in_index - 1].clearTime.ToString());
        PlayerPrefs.SetInt("Global.star", Global.star);

        ManagerData._instance.GetNewMission(Global.day);
        // 새로운 미션 받기 설정

        //Debug.Log("완료 미션 " + ManagerData._instance.missionData[in_index - 1].state + "    " + ManagerData._instance.missionData[in_index - 1].title);

        for (int i = 0; i < ManagerData._instance._newMissionIndex.Count; i++)
		{
            if (ManagerData._instance.missionData[ManagerData._instance._newMissionIndex[i] - 1].state == TypeMissionState.Inactive)
            {
                PlayerPrefs.SetInt("missionCheck" + ManagerData._instance._newMissionIndex[i], 1);
            }
          //  Debug.Log("받은 미션 " + ManagerData._instance._newMissionIndex[i] + "  " + ManagerData._instance.missionData[ManagerData._instance._newMissionIndex[i] - 1].title);
            ManagerData._instance.missionData[ManagerData._instance._newMissionIndex[i] - 1].state = TypeMissionState.Active;
            PlayerPrefs.SetInt("missionData.state" + ManagerData._instance._newMissionIndex[i], (int)ManagerData._instance.missionData[ManagerData._instance._newMissionIndex[i] - 1].state);
        }

        // 미션이 없으면 새로운 날
        if (ManagerData._instance._newMissionIndex.Count == 0)
        {
            Global.day = Global.day + 1;
            PlayerPrefs.SetInt("Global.day", Global.day);

            if (Global.day == 2)
            {
                PlayerPrefs.SetInt("missionData.state" + 12, (int)TypeMissionState.Active);
                PlayerPrefs.SetInt("missionCheck" + 12, 1);
            }

            in_newDay = true;
        }
        
        return true;
    }*/
    public bool SendBuyHousingCost(PlusHousingModelData housing, bool in_free = false)
    {
      /*  // 해당 하우징 찾기
        PlusHousingModelData housing = null;
        housing = ManagerData._instance._housingModelData[in_index];

        if (housing.costCoin <= Global.coin && housing.costJewel <= Global.jewel || in_free)
        {
            if (!in_free)
            {
                if (housing.costCoin > 0)
                    Global.coin -= housing.costCoin;
                else if (housing.costJewel > 0)
                    Global.jewel -= housing.costJewel;
                PlayerPrefs.SetInt("Global.jewel", Global.jewel);
                PlayerPrefs.SetInt("Global.coin", Global.coin);
            }
            housing.active = true;
            PlayerPrefs.SetInt("_housingModelData.active" + (in_index + 1), 1);

            return true;
        }*/
        return false;
    }
    public bool SendBuyHousingMaterial(int in_index)
    {
        // 해당 하우징 찾기
    /*    PlusHousingModelData housing = null;
        housing = ManagerData._instance._housingModelData[in_index];

        // 재료 갯수 확인
        for (int i = 0; i < housing.material.Count; i++)
        {
            for (int m = 0; m < ManagerData._instance._meterialData.Count; m++)
            {
                if (ManagerData._instance._meterialData[m].index == housing.material[i]._index)
                    if (ManagerData._instance._meterialData[m].count < housing.material[i]._count)
                        return false;
            }
        }

        // 재료 까기
        for (int i = 0; i < housing.material.Count; i++)
        {
            for (int m = 0; m < ManagerData._instance._meterialData.Count; m++)
            {
                if (ManagerData._instance._meterialData[m].index == housing.material[i]._index)
                {
                    ManagerData._instance._meterialData[m].count -= housing.material[i]._count;
                    PlayerPrefs.SetInt("_meterialData" + ManagerData._instance._meterialData[m].index, ManagerData._instance._meterialData[m].count);
                    break;
                }
            }
        }
        // 하우징 추가
        housing.active = true;
        PlayerPrefs.SetInt("_housingModelData.active" + (in_index + 1), 1);*/
        return false;
    }
    // 
    public bool SendStagePlay(int in_stage,List<int> in_useitemList = null)
    {
   /*     if (Global.clover <= 0)
            return false;

        Global.clover--;
        ManagerUI._instance.UpdateUI();
        PlayerPrefs.SetInt("Global.clover", Global.clover);*/
        return true;
    }
    public bool SendStageContinue(int in_stage,int in_cost)
    {
       /* if (in_cost > Global.jewel)
            return false;
        Global.jewel -= in_cost;
        ManagerUI._instance.UpdateUI();
        PlayerPrefs.SetInt("Global.jewel", Global.jewel);*/
        return true;
    }
    public bool SendStageFail(int in_stage, CommandDelegate in_callback = null)
    {

        return true;
    }
    public void SendStageClear(int in_stage, int in_star, int in_score, int in_coin, int in_missionProg1 = 0, int in_missionProg2 = 0)
    {
       /*if (in_coin > 0)
        {
            Global.coin += in_coin;
            ManagerUI._instance.UpdateUI();
            PlayerPrefs.SetInt("Global.coin", 0);
        }

        // 첫 클리어 리워드 보상 + 스테이지 진행
        if (ManagerData._instance._stageData[in_stage - 1]._flowerLevel == 0 && in_star > 0)
        {
            Global.stageIndex++;
            ManagerData._instance.userData.stagePos = Global.stageIndex;
            PlayerPrefs.SetInt("Global.stageIndex", Global.stageIndex);


            Global.clover++;
            ManagerUI._instance.UpdateUI();
            PlayerPrefs.SetInt("Global.clover", Global.clover);

            Global.star++;
            PlayerPrefs.SetInt("Global.star", Global.star);
        }

        if (in_star > ManagerData._instance._stageData[in_stage - 1]._flowerLevel)
        {
            ManagerData._instance._stageData[in_stage - 1]._flowerLevel = in_star;
            PlayerPrefs.SetInt("stageData._star" + in_stage, in_star);
        }
        if (in_score > ManagerData._instance._stageData[in_stage - 1]._score)
        {
            ManagerData._instance._stageData[in_stage - 1]._score = in_score;
            PlayerPrefs.SetInt("stageData._score" + in_stage, in_score);
        }
        if (in_missionProg1 > ManagerData._instance._stageData[in_stage - 1]._missionProg1)
        {
            ManagerData._instance._stageData[in_stage - 1]._missionProg1 = in_missionProg1;
            PlayerPrefs.SetInt("stageData._missionProg1" + in_stage, in_missionProg1);
        }
        if (in_missionProg2 > ManagerData._instance._stageData[in_stage - 1]._missionProg2)
        {
            ManagerData._instance._stageData[in_stage - 1]._missionProg2 = in_missionProg2;
            PlayerPrefs.SetInt("stageData._missionProg2" + in_stage, in_missionProg2);
        }*/
    }
    public bool SendHousingBuy(int in_housing,int in_model)
    {
    /*    ManagerData._instance._housingData[in_index - 1].selectModel
        for (int i = 0; i < ManagerData._instance._hao; i++)
        {
            
        }*/
        return true;
    }
    public bool SendHousingChange(int in_housing, int in_model)
    {
   /*     for (int i = 0; i < ManagerData._instance._housingData.Count; i++)
        {
            if (ManagerData._instance._housingData[i].index == in_housing)
            {
                ManagerData._instance._housingData[i].selectModel = in_model;
                break;
            }
        }
        PlayerPrefs.SetInt("_housingData" + in_housing, in_model);*/

        return true;
    }
}
