using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseHeartEffect : MonoBehaviour 
{
    public Transform _Transform;
    public UISprite _MainSprite;

    public Vector3 startPos;
    public Vector3 EndPos;

    float effect_Timer = 0;

    public AnimationCurve curveSpeed;
    public AnimationCurve curveWidth;

    private float scaleWidth = 0;

   // public UIItemDiaryMission currentMission;

    public bool getType = false;
    /*
    public void Init(Vector3 tempStart, Vector3 tempEnd, UIItemDiaryMission tempMission, bool tempGetType = false)
    {
        ManagerUI._instance.bTouchTopUI = false;

        startPos = tempStart;
        EndPos = tempEnd;
        scaleWidth = EndPos.y - startPos.y;
        currentMission = tempMission;
        getType = tempGetType;
    }
    */
    void Update()
    {
        if (effect_Timer > 1)
        {
            if (getType == false)
            {
                //미션클리어
              //  currentMission.DestroyMission();
                Destroy(gameObject);
            }
            else //스테이클리어후 하트획득시
            {
           //     Global.star = (int)GameData.Asset.Star;
              //  Global.day = (int)GameData.User.day;
              //  Global.clover = (int)(GameData.Asset.AllClover);
              //  Global.coin = (int)(GameData.Asset.AllCoin);
              //  Global.jewel = (int)(GameData.Asset.AllJewel);
              //  ManagerData._instance.userData.stage = ( int ) ServerRepos.User.stage;

            //    Debug.Log("스테이지 " + ServerRepos.User.stage);
             //   if (ServerRepos.UserStages.Count > ManagerData._instance._stageData.Count)
                {
                 ////   Debug.Log(ServerRepos.UserStages[ServerRepos.UserStages.Count - 1].stage + " 처처처처 " + ServerRepos.UserStages[ServerRepos.UserStages.Count - 1].flowerLevel);
                }
                //Global.stageIndex = 

                //하트획득이펙트
                GameObject obj = NGUITools.AddChild(ManagerUI._instance.topCenterPanel, ManagerUI._instance._objHeartGetEffect);
                obj.transform.position = ManagerUI._instance._StarSprite.transform.position;
                ManagerUI._instance.ShakeHeart();


                ManagerSound.AudioPlay(AudioInGame.GET_HEART);
                ManagerUI._instance.UpdateUI();
                Destroy(gameObject);
            }
        }

        if (getType == false)
        {
            effect_Timer += Global.deltaTimeLobby * 1.2f;
        }
        else
        {
            effect_Timer += Global.deltaTimePuzzle;
        }
        float ratio = curveSpeed.Evaluate(effect_Timer);
        float ratio2 = curveWidth.Evaluate(effect_Timer);

        float tempheight = Mathf.Lerp(startPos.y, EndPos.y, ratio);
        float tempWeidth = Mathf.Lerp(0, scaleWidth, ratio2);
        float tempWeidth2 = Mathf.Lerp(startPos.x, EndPos.x, effect_Timer);


        _Transform.localScale = Vector3.one * (1f + ratio2 * 2f);
        _Transform.localEulerAngles = new Vector3(0, 0, 180 * (1 - Mathf.Sin(Mathf.PI*0.5f * ratio)));

        if (getType == false)
        _Transform.position = new Vector3(tempWeidth2 + tempWeidth * 0.5f, tempheight, 0);
        else
            _Transform.position = new Vector3(tempWeidth2 - tempWeidth * 0.5f, tempheight, 0);


        //_Transform.position = Vector3.Lerp(startPos, EndPos, effect_Timer);

        //스프라이트애니메이션
        //int spriteNumber = ((int)(ManagerBlock.instance.BlockTime * 20f) % 3 + 1);
        //_MainSprite.spriteName = "icon_heart_Effect"; //"Star0" + spriteNumber;
    }

}
