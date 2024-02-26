using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EFFECT_TYPE
{
    NONE,
    PANGFIELD,
    END,
}

[System.Serializable]
public class InGameEffectData
{
    public GameObject effectObj;
    public EFFECT_TYPE effectType = EFFECT_TYPE.NONE;
    public int preEffectCount = 0; //미리 생성해 놓을 갯수
}

public class InGameObjectPoolManager : MonoSingletonOnlyScene<InGameObjectPoolManager>
{
    public List<InGameEffectData> effectDataList = new List<InGameEffectData>();
    public Dictionary<EFFECT_TYPE, List<ActiveHelperEffect>> objectPoolDic = new Dictionary<EFFECT_TYPE, List<ActiveHelperEffect>>();

    public void InitEffectDataList()
    {
        ClearObjectPoolDic();
        for (int i = 0; i < effectDataList.Count; i++)
        {
            List<ActiveHelperEffect> objectPool = new List<ActiveHelperEffect>();
            for (int j = 0; j < effectDataList[i].preEffectCount; j++)
            {
                ActiveHelperEffect effectObj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, effectDataList[i].effectObj).GetComponent<ActiveHelperEffect>();
                effectObj.gameObject.SetActive(false);
                objectPool.Add(effectObj);
            }
            objectPoolDic.Add(effectDataList[i].effectType, objectPool);
        }
    }

    public ActiveHelperEffect ObjectPoolSpawn(EFFECT_TYPE effectType)
    {
        if (objectPoolDic.ContainsKey(effectType) == false)
        {
            objectPoolDic.Add(effectType, new List<ActiveHelperEffect>());
        }

        GameObject obj = effectDataList.Find(x => x.effectType == effectType).effectObj;
        List<ActiveHelperEffect> spawnObjectPool = objectPoolDic[effectType];
        if (spawnObjectPool.Count == 0)
        {
            ActiveHelperEffect effectObj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, obj).GetComponent<ActiveHelperEffect>();
            effectObj.gameObject.SetActive(true);
            return effectObj;
        }
        else
        {
            ActiveHelperEffect effectObj = spawnObjectPool[spawnObjectPool.Count - 1];
            effectObj.gameObject.SetActive(true);
            spawnObjectPool.Remove(effectObj);
            return effectObj;
        }
    }

    public void ObjectPoolDeSpawn(EFFECT_TYPE effectType, ActiveHelperEffect deSpawnEffectObject)
    {
        if (objectPoolDic.ContainsKey(effectType))
        {
            int effectObjectMaxCount = effectDataList.Find(x => x.effectType == effectType).preEffectCount;
            List<ActiveHelperEffect> deSpawnObjectPool = objectPoolDic[effectType];

            if (deSpawnObjectPool.Count >= effectObjectMaxCount)
            {
                Destroy(deSpawnEffectObject.gameObject);
            }
            else
            {
                deSpawnEffectObject.gameObject.SetActive(false);
                deSpawnObjectPool.Add(deSpawnEffectObject);
            }
        }
    }

    public void ClearObjectPoolDic()
    {
        foreach (var iter in objectPoolDic)
        {
            for (int i = 0; i < iter.Value.Count; i++)
            {
                Destroy(iter.Value[i].gameObject);
            }
            iter.Value.Clear();
        }
        objectPoolDic.Clear();
    }
}
