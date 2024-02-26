using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerUIAtlas : MonoBehaviour
{
    public static ManagerUIAtlas _instance = null;

    public enum AtlasType{
        INVALID = -1,
        TARGET = 0,
        
        BLOCK = 1,
        DECO,
        DECO_TOP,
        EFFECT,
        INGAME_END,
        END = INGAME_END
    }

    [System.Serializable]
    private struct IngameAtlasTypeData
    {
        public AtlasType atlasType;
        public List<NGUIAtlas> listAtlas;
    }

    public struct AtlasData
    {
        public NGUIAtlas atlas;
        public HashSet<string> internalShards;
        
        public int overrideAtlasIndex;
    }

    [SerializeField]
    List<IngameAtlasTypeData> originalAtlases = new List<IngameAtlasTypeData>();
    Dictionary<AtlasType, AtlasData> atlases = new Dictionary<AtlasType, AtlasData>();
    

    [System.Serializable]
    public class IngameBGAtlasSet
    {
        public NGUIAtlas bgAtlas;
        public int atlasIndex;
    }

    public List<IngameBGAtlasSet> atlasSet_BG = new List<IngameBGAtlasSet>();

    static public void OnReboot()
    {
        if (_instance != null)
        {
            foreach(var a in _instance.atlases)
            {
                Destroy(a.Value.atlas);
            }
            
            _instance.atlases = new Dictionary<AtlasType, AtlasData>();

        }
    }

    public void Awake()
    {
        _instance = this;
    }

    public void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    public static bool CheckAndApplyEventAtlas(UISprite sprite)
    {
        return false;
        if (ManagerUIAtlas._instance == null)
            return false;

        if (sprite == null || sprite.atlas == null)
            return false;

        AtlasType t = AtlasType.INVALID;
        NGUIAtlas targetAtlas = null;
        switch (sprite.atlas.texture.name)
        {
            case "InGameBlock":
            case "InGameBlock2":
                {
                    t = AtlasType.BLOCK;
                }
                break;
            case "InGameUI":
                {
                    t = AtlasType.TARGET;
                }
                break;
            case "Target":
                {
                    t = AtlasType.TARGET;
                }
                break;
            case "InGameEffect":
                {
                    t = AtlasType.EFFECT;
                }
                break;
        }

        if( ManagerUIAtlas._instance.atlases.ContainsKey(t) )
            targetAtlas = ManagerUIAtlas._instance.atlases[t].atlas;

        if (targetAtlas == null)
            return false;

        sprite.atlas = targetAtlas;
        return true;
    }

    void RetrieveAtlas(AtlasType t, GameObject atlasObj, int atlasIndex, List<INGUIAtlas> outAtlas)
    {
        if (atlasObj != null)
        {
            var eAtlas = atlasObj.GetComponent<CustomAtlasPack>();
            if(eAtlas == null)
                return;

            var atlas = eAtlas.GetAtlas(t);
            if (atlas != null)
                outAtlas.Add(atlas);
        }
    }

    public IEnumerator BuildAtlas(AtlasType t, int overrideAtlasIndex, List<string> atlasShardIndexes)
    {
        HashSet<string> shardIndexSet = new HashSet<string>(atlasShardIndexes);
        if (NeedRebuildAtlas(t, overrideAtlasIndex, shardIndexSet) == false)
        {
            int findIndex = originalAtlases.FindIndex(x => x.atlasType == t);
            if (findIndex > -1)
            {
                List<NGUIAtlas> listAtlas = originalAtlases[findIndex].listAtlas;
                for (int i = 0; i < listAtlas.Count; i++)
                {
                    listAtlas[i].replacement = atlases[t].atlas;
                }
            }
            yield break;
        }
        ClearTargetAtlas(t);

        List<INGUIAtlas> atlasShards = new List<INGUIAtlas>();
        if (overrideAtlasIndex != 0)
        {
            if (Global.LoadFromInternal)
                LoadFromInternal(t, overrideAtlasIndex, atlasShards);
            else
                yield return LoadFromBundle(t, overrideAtlasIndex, atlasShards);
        }

        List<NGUIAtlas> inBuildShards = new List<NGUIAtlas>();
        LoadAtlasShards(t, shardIndexSet, inBuildShards);

        atlasShards.AddRange(inBuildShards);

        var newAtlas = new AtlasData()
        {
            atlas = NGUIExtension.CompositeAtlas(atlasShards, t.ToString()),
            overrideAtlasIndex = overrideAtlasIndex,
            internalShards = shardIndexSet
        };

        atlases.Add(t, newAtlas);

        UnloadAtlasShards(inBuildShards);

        int findAtlasIndex = originalAtlases.FindIndex(x => x.atlasType == t);
        if (findAtlasIndex > -1)
        {
            List<NGUIAtlas> listAtlas = originalAtlases[findAtlasIndex].listAtlas;
            for (int i = 0; i < listAtlas.Count; i++)
            {
                listAtlas[i].replacement = atlases[t].atlas;
            }
        }
    }
    

    bool NeedRebuildAtlas(AtlasType at, int overrideAtlasIndex, HashSet<string> atlasShardIndexes)
    {
        if (this.atlases.ContainsKey(at) == false)
        {
            return true;
        }

        var atlas = atlases[at];

        if( atlas.overrideAtlasIndex != overrideAtlasIndex )
            return true;

        HashSet<string> tempShardList = new HashSet<string>(atlas.internalShards);
        tempShardList.SymmetricExceptWith(atlasShardIndexes);
        if (tempShardList.Count > 0)
            return true;

        return false;
    }

    void ClearTargetAtlas(AtlasType at)
    {
        if (this.atlases.ContainsKey(at) == false)
        {
            return;
        }

        Destroy( atlases[at].atlas );
        atlases.Remove(at);
    }

    void LoadFromInternal(AtlasType t, int atlasIndex, List<INGUIAtlas> outAtlas)
    {
        string path = "Assets/5_OutResource/eventStageAtlas/eventStageAtlas_" + atlasIndex.ToString() + "/EventAtlasPack"+ ".prefab";
#if UNITY_EDITOR
        var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (obj == null)
            return;
        RetrieveAtlas(t, obj, atlasIndex, outAtlas);
#endif
    }

    IEnumerator LoadFromBundle(AtlasType t, int atlasIndex, List<INGUIAtlas> outAtlas)
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        string name = "eventstageatlas_" + atlasIndex.ToString();
        IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(name);
        while (e.MoveNext())
            yield return e.Current;
        if (e.Current != null)
        {
            AssetBundle assetBundle = e.Current as AssetBundle;
            if (assetBundle != null)
            {
                GameObject obj = assetBundle.LoadAsset<GameObject>("EventAtlasPack");
                RetrieveAtlas(t, obj, atlasIndex, outAtlas);
            }
        }
        NetworkLoading.EndNetworkLoading();
    }

    public NGUIAtlas GetAtlas(AtlasType type)
    {
        if (atlases.ContainsKey(type) == false) return null;
        return atlases[type].atlas;
    }

    #region 인게임 BG 아틀라스
    //현재 변경하고자 하는 인게임 BG의 아틀라스 인덱스를 반환
    public static int GetCustomIngameBGAtlasIndex()
    {
        int bgIndex = GameManager.instance.ingameBGIndex;
        if (bgIndex == 0)
            return -1;

        return ManagerUIAtlas._instance.atlasSet_BG.FindIndex(x => x.atlasIndex == bgIndex);
    }

    public IEnumerator LoadIngameBGAtlas(int atlasIndex)
    {
        //이미 아틀라스 있는 경우는 제외.
        if (IsExist_IngameBGAtlasSet(atlasIndex) == true)
            yield break;

        if (Global.LoadFromInternal)
            LoadFromInternal_IngameBG(atlasIndex);
        else
            yield return LoadFromBundle_IngameBG(atlasIndex);
    }

    public void LoadFromInternal_IngameBG(int atlasIndex)
    {
        string folderName = "inGameBG_" + atlasIndex.ToString();
        string fileName = "InGameBG_" + atlasIndex.ToString();
        string path = "Assets/5_OutResource/inGameBGAtlas/" + folderName + "/" + fileName + ".asset";
#if UNITY_EDITOR
        var atlas = UnityEditor.AssetDatabase.LoadAssetAtPath<NGUIAtlas>(path);
        if (atlas == null)
        {
            Debug.LogError("Asset Not found: " + path + "(not in assetDataList)");
            return;
        }
        AddIngameBGAtlas(atlas, atlasIndex);
#endif
    }

    void LoadAtlasShards(AtlasType t, HashSet<string> indexes, List<NGUIAtlas> atlasShards )
    {
        foreach(var i in indexes)
        {
            string path = $"AtlasShards/as_{t.ToString()}_{i}";
            var shard = Resources.Load<NGUIAtlas>(path);
            if( shard != null )
                atlasShards.Add(shard);
        }
    }

    void UnloadAtlasShards(List<NGUIAtlas> atlasShards)
    {
        foreach (var i in atlasShards)
        {
            Resources.UnloadAsset(i);
        }
        Resources.UnloadUnusedAssets();
    }

    public IEnumerator LoadFromBundle_IngameBG(int atlasIndex)
    {
        NetworkLoading.MakeNetworkLoading(0.5f);
        string name = "ingamebg_" + atlasIndex;
        IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(name);
        while (e.MoveNext())
            yield return e.Current;
        if (e.Current != null)
        {
            AssetBundle assetBundle = e.Current as AssetBundle;
            if (assetBundle != null)
            {
                NGUIAtlas atlas = assetBundle.LoadAsset<NGUIAtlas>(name);
                AddIngameBGAtlas(atlas, atlasIndex);
            }
        }
        NetworkLoading.EndNetworkLoading();
    }

    public void AddIngameBGAtlas(NGUIAtlas atlas, int atlasIndex)
    {
        if (atlas != null)
        {
            IngameBGAtlasSet newAtlasSet = new IngameBGAtlasSet();
            newAtlasSet.atlasIndex = atlasIndex;
            newAtlasSet.bgAtlas = atlas;
            atlasSet_BG.Add(newAtlasSet);
        }
    }

    public bool IsExist_IngameBGAtlasSet(int atlasIndex)
    {
        if (atlasSet_BG.FindIndex(x => x.atlasIndex == atlasIndex) == -1)
            return false;
        else
            return true;
    }

    public static bool CheckAndApplyIngameBGAtlas(UISprite sprite)
    {
        if (ManagerUIAtlas._instance == null)
            return false;

        if (sprite == null)
            return false;

        int atlasIndex = GetCustomIngameBGAtlasIndex();
        if (atlasIndex == -1)
            return false;

        NGUIAtlas targetAtlas = ManagerUIAtlas._instance.atlasSet_BG[atlasIndex].bgAtlas;
        if (targetAtlas == null)
            return false;

        sprite.atlas = targetAtlas;
        return true;
    }

    public static NGUIAtlas CheckAndApplyIngameBGAtlas()
    {
        if (ManagerUIAtlas._instance == null)
            return null;

        int atlasIndex = GetCustomIngameBGAtlasIndex();
        if (atlasIndex == -1)
            return null;

        NGUIAtlas targetAtlas = ManagerUIAtlas._instance.atlasSet_BG[atlasIndex].bgAtlas;
        return targetAtlas;
    }
    #endregion
}
