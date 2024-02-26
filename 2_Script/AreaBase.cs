using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneStateInfo
{
    public TriggerScene sceneData = null;
    public TypeSceneState state = TypeSceneState.Wait;
}

[System.Serializable]
public class CharCostumePair
{
    public TypeCharacterType character;
    public int costumeIdx;
}

[System.Serializable]
public class UseCharacterData_AtScene
{
    public TypeCharacterType _type = TypeCharacterType.Boni;
    public List<int> listUseMission = new List<int>();
}

public abstract class AreaBase : MonoBehaviour
{
    public List<SceneStateInfo> _listSceneDatas = new List<SceneStateInfo>();

    public List<TypeCharacterType> _characters = new List<TypeCharacterType>();
    public List<TypeCharacterType> _live2dChars = new List<TypeCharacterType>();

    public List<CharCostumePair> _costumeCharacters = new List<CharCostumePair>();

    //현재 에리어가 활성화 되었을 때, 오픈할 에리어 인덱스.
    [SerializeField]
    public int openAreaIndex = -1;

    //씬에서 사용하는 캐릭터의 정보와 해당 캐릭터가 사용되는 미션정보.
    [SerializeField]
    public List<UseCharacterData_AtScene> listUseCharacterData_AtScene = new List<UseCharacterData_AtScene>();

    [SerializeField]
    public float sceneStartBgmOffset = 96f;

    [SerializeField]
    public bool sceneStartBgmOff = false;


    public virtual bool IsEventArea() { return false; }


    // 자신의 상위에 존재하는 중 가장 가까운 Area 객체를 찾기
    static public AreaBase ScanNearestAreaBase(Transform t)
    {
        if (t.parent == null)
            return null;

        AreaBase areaBase = t.GetComponentInParent<AreaBase>();
        if (areaBase == null)
        {
            return ScanNearestAreaBase(t.parent);
        }
        else return areaBase;
    }

    protected void InitSceneDatas()
    {
        for (int i = 0; i < _listSceneDatas.Count; i++)
        {
            if (_listSceneDatas[i].sceneData != null)
            {
                _listSceneDatas[i].sceneData._sceneIndex = i + 1;

                _listSceneDatas[i].sceneData._triggerWait.gameObject.SetActive(false, true);
                _listSceneDatas[i].sceneData._triggerWakeUp.gameObject.SetActive(false, true);
                _listSceneDatas[i].sceneData._triggerActive.gameObject.SetActive(false, true);
                _listSceneDatas[i].sceneData._triggerFinish.gameObject.SetActive(false, true);
            }
        }

        var entryFocus = this.gameObject.GetComponent<LobbyEntryFocus>();
        if (entryFocus != null)
            entryFocus.enabled = false;

        SetUseCharacterRegisterList();
    }

    //에리어에 등록돼있는 씬에서 사용할 캐릭터 정보 중, 현재 미션 상태가 완료되지 않은 목록들만 리스트에 추가.
    private void SetUseCharacterRegisterList()
    {
        for (int i = 0; i < listUseCharacterData_AtScene.Count; i++)
        {
            UseCharacterData_AtScene checkData = listUseCharacterData_AtScene[i];
            if (checkData.listUseMission != null && checkData.listUseMission.Count > 0)
            {
                List<int> listAddMission = new List<int>();
                for (int j = 0; j < checkData.listUseMission.Count; j++)
                {
                    if (ManagerData._instance._missionData[checkData.listUseMission[j]].state != TypeMissionState.Clear)
                    {
                        listAddMission.Add(checkData.listUseMission[j]);
                    }
                }

                if (listAddMission.Count > 0)
                {
                    ManagerLobby._instance.AddCharacter_UseCharacterRegisterList(checkData._type, listAddMission);
                }
            }
        }
    }

    protected void TriggerStart_Internal()
    {
        bool areaActivated = false;
        for (int i = 0; i < _listSceneDatas.Count; i++)
        {
            if (_listSceneDatas[i].sceneData != null)
            {
                if (_listSceneDatas[i].state == TypeSceneState.Wait)
                {
                    _listSceneDatas[i].sceneData._triggerWait.gameObject.SetActive(true);
                    _listSceneDatas[i].sceneData._triggerWait.StartCondition();
                }
                else if (_listSceneDatas[i].state == TypeSceneState.Active)
                {
                    areaActivated = true;
                    _listSceneDatas[i].sceneData._triggerActive.gameObject.SetActive(true);
                    _listSceneDatas[i].sceneData._triggerActive.StartCondition();
                }
                else if (_listSceneDatas[i].state == TypeSceneState.Finish)
                {
                    areaActivated = true;
                    _listSceneDatas[i].sceneData._triggerFinish.gameObject.SetActive(true);
                    _listSceneDatas[i].sceneData._triggerFinish.StartCondition();
                }
            }
        }

        if (areaActivated)
        {
            var entryFocus = this.gameObject.GetComponent<LobbyEntryFocus>();
            if (entryFocus != null)
                entryFocus.enabled = true;
        }
    }

    public void ScanCharacterUsageInChildrens()
    {
#if UNITY_EDITOR
        _characters.Clear();
        _costumeCharacters.Clear();

        var cs = gameObject.GetComponentsInChildren<ActionCharacterSpawn>();
        var bs = gameObject.GetComponentsInChildren<ActionBirdSpawn>();

        for(int i = 0; i < cs.Length; ++i)
        {
            var cosPair = new CharCostumePair() { character = cs[i]._type, costumeIdx = cs[i].costumeIdx };

            if( cosPair.costumeIdx == 0 && 
                cosPair.character < TypeCharacterType.Mai )
            {
                // 빌드 내장된 캐릭터는 패스
                continue;
            }

            if( cosPair.costumeIdx == 0)
            {
                bool alreadyExist = this._characters.Exists(x => x == cosPair.character);
                if (alreadyExist == false)
                {
                    _characters.Add(cosPair.character);
                }
            }
            else
            {
                bool alreadyExist = this._costumeCharacters.Exists(x => (x.character == cosPair.character && x.costumeIdx == cosPair.costumeIdx));
                if (alreadyExist == false)
                {
                    _costumeCharacters.Add(cosPair);
                }
            }
        }
        for(int i = 0; i < bs.Length; ++i)
        {
            if (bs[i].costumeIdx == 0)
                continue;   // 기본마유지는 빌드 내장...

            var cosPair = new CharCostumePair() { character = TypeCharacterType.BlueBird, costumeIdx = bs[i].costumeIdx };

            var alreadyExist = this._costumeCharacters.Find(x => (x.character == cosPair.character && x.costumeIdx == cosPair.costumeIdx));
            if (alreadyExist == null)
            {
                _costumeCharacters.Add(cosPair);
            }
        }
#endif
    }

    public abstract void TriggerStart();

    public AreaBase GetAreaBase()
    {
        return this;
    }
}
