using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class AssetGlobal : MonoBehaviour
{
    [Header("문자열 파일들 더이상 여기서 제어하지 않습니다")]
    [System.Obsolete("더이상 사용안합니다")]
    public TextAsset _jpJsonData = null;
    [System.Obsolete("더이상 사용안합니다")]    
    public TextAsset _exJsonData = null;

    [System.Obsolete("더이상 사용안합니다")]
    [SerializeField] private string _jpFileName;
    [System.Obsolete("더이상 사용안합니다")]
    [SerializeField] private string _exFileName;
    [SerializeField]
    private bool useJsonFile = false;
    [Header("------------------------")]

    public ManagerEnvironment.EnvEffectType lobbyLeafType = ManagerEnvironment.EnvEffectType.ENV_EFFECT_DANDELION;
    public bool lobbyCloudEnabled = true;

    //에리어와 연결된 동물 데이터
    public List<ManagerAreaAnimal.AreaAnimalData_InputData> listAreaAnimalData = new List<ManagerAreaAnimal.AreaAnimalData_InputData>();

    //에리어 별 로밍 데이터
    public List<RoamingAreaData> listRoamingAreaData = new List<RoamingAreaData>();

    // Use this for initialization
    void Start ()
    {
        
        AddLoadListAreaAnimal();

        ManagerEnvironment._instance.cloudEnabled = this.lobbyCloudEnabled;
        ManagerEnvironment._instance.envEffectType = this.lobbyLeafType;
    }

    

    private void AddLoadListAreaAnimal()
    {
        if (ManagerAreaAnimal.IsActiveEvent())
        {
            ManagerAreaAnimal.SetAreaAnimalData(listAreaAnimalData, listRoamingAreaData);
        }
    }
}
