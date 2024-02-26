using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//재료 정렬 관련 코드.
public class MaterialComparer : IComparer<ServerUserMaterial>
{
    // -1 : a를 앞으로, 1: a를 뒤로, 0: 그대로.
    public int Compare(ServerUserMaterial a, ServerUserMaterial b)
    {
        /*
        //a, b 두 개 다 이벤트 재료이거나 아닐경우는 정렬안함.
        //a 만 이벤트 재료라면 a를 앞으로 정렬.
        if (a.expire_ts > 0 && b.expire_ts == 0)
        {
            return -1;
        }
        //b 만 이벤트 재료라면 b를 앞으로 정렬.
        else if (a.expire_ts == 0 && b.expire_ts > 0)
            return 1;
        else*/
            return 0;
    }
}

public class UIPopupMaterial : UIPopupBase
{
    public UIPanel scrollPanel;
    public UILabel[] title;
    public UILabel emptyText;
    public UILabel materialInfo;
    public GameObject _objMaterial;

    private float xPos = -450f;
    private float yPos = 430f;

    MaterialComparer materialComparer = new MaterialComparer();

    public void InitMaterialPopup(long openTime)
    {
        scrollPanel.depth = uiPanel.depth + 1;
        InitPopupText();

        //재료 팝업 열릴 때, 처음 한번만 재료들 획득 데이터 받아옴.
        //UIDiaryController._instance.CheckMaterialGetInfo();

        List<ServerUserMaterial> materialDataList = ServerRepos.UserMaterials;
        //materialDataList.Sort(materialComparer);

        int materialCount = materialDataList.Count;
        for (int i = 0; i < materialCount; i++)
        {
            long checkTime = openTime;
            if (checkTime == 0)
            {
                checkTime = Global.GetTime();
            }
            if (ManagerData._instance._materialSpawnProgress.ContainsKey(materialDataList[i].index) == true)
            {
                //기간 한정 재료는 시간이 다 지나면 목록에서 보이지 않도록(하우징 탭을 누른 시간을 기점으로).
                if (ManagerData._instance._materialSpawnProgress[materialDataList[i].index] != 0 
                    && ManagerData._instance._materialSpawnProgress[materialDataList[i].index] < checkTime)
                    continue;
            }
            UIItemMaterial material = NGUITools.AddChild(scrollPanel.gameObject, _objMaterial).GetComponent<UIItemMaterial>();
            material.InitItemMaterial(materialDataList[i]);
            if (i >= 4 && i % 4 == 0)
            {
                xPos = -450f;
                yPos -= 200f;
            }
            xPos += 180f;
            material.transform.localPosition = new Vector3(xPos, yPos, 0);
        }

        if (materialDataList.Count > 0)
        {
            emptyText.gameObject.SetActive(false);
        }
        else
        {
            emptyText.gameObject.SetActive(true);
            emptyText.text = Global._instance.GetString("p_e_8");
        }
    }

    private void InitPopupText()
    {
        string titleText = Global._instance.GetString("p_mt_1");
        for (int i = 0; i < title.Length; i++)
        {
            title[i].text = titleText;
        }
        materialInfo.text = Global._instance.GetString("p_mt_2");
    }

    private void OnClickBtnMaterialInfo()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGPKV_material", Global._instance.GetString("p_mt_2"));
    }
}