using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventureEntry : AreaBase
{
    public static AdventureEntry _instance = null;

    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] public TextAsset _jpStringData = null;
    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] public TextAsset _exStringData = null;

    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] [SerializeField] private string _jpFileName;
    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] [SerializeField] private string _exFileName;

    [SerializeField] private string textFileName;

    public ObjectEvent _touchTarget;

    public override bool IsEventArea()
    {
        return true;
    }

    void Awake()
    {
        _instance = this;

        InitSceneDatas();
    }

    private void OnDestroy()
    {
        if(_instance == this)
            _instance = null;
    }

    public void OnActionUIActive(bool in_active)
    {
        //if (_uiEvent != null)
        //    _uiEvent.gameObject.SetActive(in_active);
    }

    public override void TriggerStart()
    {
        ManagerSound._instance.SetTimeBGM(sceneStartBgmOffset);

        if (sceneStartBgmOff)
            ManagerSound._instance?.PauseBGM();

        TriggerStart_Internal();
    }

    // Use this for initialization
    void Start()
    {
        if (_touchTarget != null)
            _touchTarget._onTouch = (() => onTouch());

        if (!string.IsNullOrEmpty(textFileName))
        {
            StringHelper.LoadStringFromCDN(textFileName, Global._instance._stringData);
        }
    }

    void onTouch()
    {
        ManagerUI._instance.OpenPopupStageAdventure();
    }

    static public IEnumerator OnLoadLobbyObject()
    {
        if (ManagerAdventure.CheckStartable() && ManagerLobby.landIndex == 0)
        {
            string adventureBundleName = "adv_ent";
            string prefabName = "adventure_entry";

            GameObject obj = null;
            if (Global.LoadFromInternal)
            {
#if UNITY_EDITOR
                string path = "Assets/5_OutResource/adventure_entry/adventure_entry.prefab";
                GameObject BundleObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (BundleObject)
                {
                    obj = ManagerLobby.NewObject(BundleObject);

                    AreaBase areaBase = BundleObject.GetComponent<AreaBase>();
                    if (areaBase)
                    {
                        ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);
                    }
                }
#endif
            }
            else
            {
                IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(adventureBundleName);
                while (e.MoveNext())
                    yield return e.Current;
                if (e.Current != null)
                {
                    AssetBundle assetBundle = e.Current as AssetBundle;
                    if (assetBundle != null)
                    {
                        var bundleReqAsync = assetBundle.LoadAssetAsync<GameObject>(prefabName);
                        yield return bundleReqAsync;
                        if (bundleReqAsync.isDone)
                        {
                            GameObject objn = bundleReqAsync.asset as GameObject;
                            obj = ManagerLobby.NewObject(objn);
                            AreaBase areaBase = obj.GetComponent<AreaBase>();
                            if (areaBase)
                            {
                                ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);
                            }
                        }
                    }
                }
            }

            yield return ManagerCharacter._instance.LoadCharacters();

            if (obj != null)
            {
                AdventureEntry adventureEntry = obj.GetComponent<AdventureEntry>();
                ManagerArea._instance._adventureEntry = adventureEntry;
            }

        }
    }
}
