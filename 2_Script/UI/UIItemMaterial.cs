using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemMaterial : MonoBehaviour
{
    public UIMaterialTexture materialTexture;
    public UILabel materialName;
    public UILabel materialCount;

    //기간한정 데코 관련.
    public GameObject objEventRoot;
    public GameObject eventSprite;
    public GameObject materialTimeBack;
    public UILabel materialTime;

    private ServerUserMaterial matData;

    public void InitItemMaterial(ServerUserMaterial data)
    {
        matData = data;
        materialTexture.InitMaterialTexture(matData.index, 100, 100);
        string name = string.Format("mt_" + (matData.index));
        materialName.text = Global._instance.GetString(name);
        materialCount.text = matData.count.ToString();

        //기간한정 재료 세팅.
        if (ManagerData._instance._materialSpawnProgress.ContainsKey(matData.index) == true 
            && ManagerData._instance._materialSpawnProgress[matData.index] != 0)
        {
            SettingEventDeco(ManagerData._instance._materialSpawnProgress[matData.index]);
        }
    }

    private void SettingEventDeco(long expireTs)
    {
        objEventRoot.SetActive(true);
        SettingTimePosition();
        StartCoroutine(CoEventTimer(expireTs));
    }

    private void SettingTimePosition()
    {
        int lineCount = (int)(materialName.printedSize.y / materialName.fontSize);
        if (lineCount <= 1)
        {
            materialTimeBack.transform.localPosition = new Vector3(0f, -90f, 0f);
        }
        else
        {
            materialTimeBack.transform.localPosition = new Vector3(0f, -110f, 0f);
        }
    }

    private IEnumerator CoEventTimer(long expireTs)
    {
        while (gameObject.activeInHierarchy == true)
        {
            materialTime.text = Global.GetTimeText_DDHHMM(expireTs);
            if (Global.LeftTime(expireTs) <= 0)
            {
                materialTime.text = "00:00:00";
                break;
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnClickBtnMaterial()
    {
        MaterialGetInfo info = UIDiaryController._instance.GetMaterialGetInfo(matData.index);
        if (info != null)
        {
            ManagerUI._instance.OpenPopupMaterialInfo(materialName.text, info);
        }
    }
}
