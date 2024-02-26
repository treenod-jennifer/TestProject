using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerObjectPool : MonoBehaviour {

    public static ManagerObjectPool _instance = null;


    ///////////////////////////////////////////////////////////////////////////////////////////
    [System.Serializable]
    public class CategoryData
    {
        public string category;
        public List<StartupPool> dataList = new List<StartupPool>();
    }

    [System.Serializable]
    public class StartupPool
    {
        public int size;
        public GameObject prefab;
    }

    [System.Serializable]
    public class SoundCategoryData
    {
        public string category;
        public List<AudioClip> dataList = new List<AudioClip>();
    }
    ///////////////////////////////////////////////////////////////////////////////////////////
    private static List<GameObject> tempList = new List<GameObject>();

    private readonly Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();
    private readonly Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();
    private readonly Dictionary<string, GameObject> nameToPrefabs = new Dictionary<string, GameObject>();

    private bool startupPoolsCreated;
    ///////////////////////////////////////////////////////////////////////////////////////////
    public List<CategoryData> startupPools = new List<CategoryData>();
    ///////////////////////////////////////////////////////////////////////////////////////////





    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            CreateStartupPools();
        }
    }
	// Use this for initialization
	void Start () {
		
	}
    ///////////////////////////////////////////////////////////////////////////////////////////
    public static void CreateStartupPools()
    {
        if (!_instance.startupPoolsCreated)
        {
            _instance.startupPoolsCreated = true;
            var pools = _instance.startupPools;
            if (pools != null && pools.Count > 0)
            {
                for (int i = 0; i < pools.Count; ++i)
                {
                    List<StartupPool> dataList = pools[i].dataList;
                    for (int j = 0; j < dataList.Count; j++)
                    {
                        CreatePool(dataList[j].prefab, dataList[j].size);
                    }
                }
            }
        }
    }

    public static void CreatePool<T>(T prefab, int initialPoolSize) where T : Component
    {
        CreatePool(prefab.gameObject, initialPoolSize);
    }
    public static void CreatePool(GameObject prefab, int initialPoolSize)
    {
        if (prefab != null)
        {
            if (!_instance.nameToPrefabs.ContainsKey(prefab.name))
            {
                _instance.nameToPrefabs.Add(prefab.name, prefab);
            }

            if (!_instance.pooledObjects.ContainsKey(prefab))
            {
                var list = new List<GameObject>();
                _instance.pooledObjects.Add(prefab, list);

                if (initialPoolSize > 0)
                {
                    bool active = prefab.activeSelf;
                    prefab.SetActive(false);
                    Transform parent = _instance.transform;
                    while (list.Count < initialPoolSize)
                    {
                        var obj = (GameObject)Object.Instantiate(prefab);
                        obj.transform.parent = parent;
                        list.Add(obj);
                    }
                    prefab.SetActive(active);
                }
            }
        }
    }
    ///////////////////////////////////////////////////////////////////////////////////////////
    //startupPools 프리팹 등록. (카테고리 없을때 카테고리 추가)
    public static void RegisterStartupPools(string category, StartupPool in_pool)
    {
        var pools = _instance.startupPools;
        if (pools != null)
        {
            //카테고리가 있을때
            for (int i = 0; i < pools.Count; ++i)
            {
                if (pools[i].category == category)
                {
                    if (pools[i].dataList.Contains(in_pool) == false)
                        pools[i].dataList.Add(in_pool);
                    return;
                }
            }

            //카테고리가 없을때
            CategoryData data = new CategoryData();
            data.category = category;
            data.dataList = new List<StartupPool>();
            data.dataList.Add(in_pool);
            pools.Add(data);
        }
    }

    //startupPools 에서 해당 카테고리와 프리팹들 모두 삭제.
    public static void RemoveStartupPools(string category)
    {
        var pools = _instance.startupPools;
        if (pools != null)
        {
            for (int i = 0; i < pools.Count; ++i)
            {
                if (pools[i].category == category)
                {
                    pools.Remove(pools[i]);
                    return;
                }
            }
        }
    }
    ///////////////////////////////////////////////////////////////////////////////////////////
    public static T Spawn<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
    {
        return Spawn(prefab.gameObject, parent, position, rotation).GetComponent<T>();
    }
    public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        return Spawn(prefab.gameObject, null, position, rotation).GetComponent<T>();
    }
    public static T Spawn<T>(T prefab, Transform parent, Vector3 position) where T : Component
    {
        return Spawn(prefab.gameObject, parent, position, Quaternion.identity).GetComponent<T>();
    }
    public static T Spawn<T>(T prefab, Vector3 position) where T : Component
    {
        return Spawn(prefab.gameObject, null, position, Quaternion.identity).GetComponent<T>();
    }
    public static T Spawn<T>(T prefab, Transform parent) where T : Component
    {
        return Spawn(prefab.gameObject, parent, Vector3.zero, Quaternion.identity).GetComponent<T>();
    }
    public static T Spawn<T>(T prefab) where T : Component
    {
        return Spawn(prefab.gameObject, null, Vector3.zero, Quaternion.identity).GetComponent<T>();
    }
    public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
    {
        List<GameObject> list;
        Transform trans;
        GameObject obj;
        if (_instance.pooledObjects.TryGetValue(prefab, out list))
        {
            obj = null;
            if (list.Count > 0)
            {
                while (obj == null && list.Count > 0)
                {
                    obj = list[0];
                    list.RemoveAt(0);
                }
                if (obj != null)
                {
                    trans = obj.transform;
                    trans.parent = parent;
                    trans.localPosition = position;
                    trans.localRotation = rotation;
                    obj.SetActive(true);
                    _instance.spawnedObjects.Add(obj, prefab);
                    return obj;
                }
            }
            obj = (GameObject)Object.Instantiate(prefab);
            trans = obj.transform;
            trans.parent = parent;
            trans.localPosition = position;
            trans.localRotation = rotation;
            obj.SetActive(true);
            _instance.spawnedObjects.Add(obj, prefab);
            return obj;
        }
        else
        {
            obj = (GameObject)Object.Instantiate(prefab);
            trans = obj.GetComponent<Transform>();
            trans.parent = parent;
            trans.localPosition = position;
            trans.localRotation = rotation;
            CreatePool(obj, 1);
            obj.SetActive(true);
            _instance.spawnedObjects.Add(obj, prefab);
            return obj;
        }
    }
    public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position)
    {
        return Spawn(prefab, parent, position, Quaternion.identity);
    }
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return Spawn(prefab, null, position, rotation);
    }
    public static GameObject Spawn(GameObject prefab, Transform parent)
    {
        return Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
    }
    public static GameObject Spawn(GameObject prefab, Vector3 position)
    {
        return Spawn(prefab, null, position, Quaternion.identity);
    }
    public static GameObject Spawn(GameObject prefab)
    {
        return Spawn(prefab, null, Vector3.zero, Quaternion.identity);
    }


    public static GameObject Spawn(string name, Transform parent, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = FindByName(name);
        if (prefab != null)
            return Spawn(prefab, parent, position, rotation);

        Debug.Log("not found : " + name);
        return null;
    }
    public static GameObject Spawn(string name, Transform parent, Vector3 position)
    {
        return Spawn(name, parent, position, Quaternion.identity);
    }
    public static GameObject Spawn(string name, Vector3 position, Quaternion rotation)
    {
        return Spawn(name, null, position, rotation);
    }
    public static GameObject Spawn(string name, Transform parent)
    {
        return Spawn(name, parent, Vector3.zero, Quaternion.identity);
    }
    public static GameObject Spawn(string name, Vector3 position)
    {
        return Spawn(name, null, position, Quaternion.identity);
    }
    public static GameObject Spawn(string name)
    {
        return Spawn(name, null, Vector3.zero, Quaternion.identity);
    }
    ///////////////////////////////////////////////////////////////////////////////////////////

    public static void Recycle<T>(T obj) where T : Component
    {
        Recycle(obj.gameObject);
    }
    public static void Recycle(GameObject obj)
    {
        GameObject prefab;
        if (_instance.spawnedObjects.TryGetValue(obj, out prefab))
            Recycle(obj, prefab);
        else
            Object.Destroy(obj);
    }
    static void Recycle(GameObject obj, GameObject prefab)
    {
        /*
        if (obj == null)
            return;
         * */
        _instance.pooledObjects[prefab].Add(obj);
        _instance.spawnedObjects.Remove(obj);
        obj.transform.parent = _instance.transform;
        obj.SetActive(false);
    }

    public static void RecycleAll<T>(T prefab) where T : Component
    {
        RecycleAll(prefab.gameObject);
    }
    public static void RecycleAll(GameObject prefab)
    {
        foreach (var item in _instance.spawnedObjects)
            if (item.Value == prefab)
                tempList.Add(item.Key);
        for (int i = 0; i < tempList.Count; ++i)
            Recycle(tempList[i]);
        tempList.Clear();
    }
    public static void RecycleAll()
    {
        tempList.AddRange(_instance.spawnedObjects.Keys);
        for (int i = 0; i < tempList.Count; ++i)
            Recycle(tempList[i]);
        tempList.Clear();
    }
    ///////////////////////////////////////////////////////////////////////////////////////////
    public static bool IsSpawned(GameObject obj)
    {
        return _instance.spawnedObjects.ContainsKey(obj);
    }

    public static int CountPooled<T>(T prefab) where T : Component
    {
        return CountPooled(prefab.gameObject);
    }
    public static int CountPooled(GameObject prefab)
    {
        List<GameObject> list;
        if (_instance.pooledObjects.TryGetValue(prefab, out list))
            return list.Count;
        return 0;
    }

    public static int CountSpawned<T>(T prefab) where T : Component
    {
        return CountSpawned(prefab.gameObject);
    }
    public static int CountSpawned(GameObject prefab)
    {
        int count = 0;
        foreach (var instancePrefab in _instance.spawnedObjects.Values)
            if (prefab == instancePrefab)
                ++count;
        return count;
    }

    public static int CountAllPooled()
    {
        int count = 0;
        foreach (var list in _instance.pooledObjects.Values)
            count += list.Count;
        return count;
    }

    public static List<GameObject> GetPooled(GameObject prefab, List<GameObject> list, bool appendList)
    {
        if (list == null)
            list = new List<GameObject>();
        if (!appendList)
            list.Clear();
        List<GameObject> pooled;
        if (_instance.pooledObjects.TryGetValue(prefab, out pooled))
            list.AddRange(pooled);
        return list;
    }
    public static List<T> GetPooled<T>(T prefab, List<T> list, bool appendList) where T : Component
    {
        if (list == null)
            list = new List<T>();
        if (!appendList)
            list.Clear();
        List<GameObject> pooled;
        if (_instance.pooledObjects.TryGetValue(prefab.gameObject, out pooled))
            for (int i = 0; i < pooled.Count; ++i)
                list.Add(pooled[i].GetComponent<T>());
        return list;
    }

    public static List<GameObject> GetSpawned(GameObject prefab, List<GameObject> list, bool appendList)
    {
        if (list == null)
            list = new List<GameObject>();
        if (!appendList)
            list.Clear();
        foreach (var item in _instance.spawnedObjects)
            if (item.Value == prefab)
                list.Add(item.Key);
        return list;
    }
    public static List<T> GetSpawned<T>(T prefab, List<T> list, bool appendList) where T : Component
    {
        if (list == null)
            list = new List<T>();
        if (!appendList)
            list.Clear();
        var prefabObj = prefab.gameObject;
        foreach (var item in _instance.spawnedObjects)
            if (item.Value == prefabObj)
                list.Add(item.Key.GetComponent<T>());
        return list;
    }
    ///////////////////////////////////////////////////////////////////////////////////////////
    public static GameObject FindByName(string name)
    {
        if (_instance.nameToPrefabs.ContainsKey(name))
            return _instance.nameToPrefabs[name];

        return null;
    }

    public static void DestroyPooled(GameObject prefab)
    {
        List<GameObject> pooled;
        if (_instance.pooledObjects.TryGetValue(prefab, out pooled))
        {
            for (int i = 0; i < pooled.Count; ++i)
                GameObject.Destroy(pooled[i]);
            pooled.Clear();
        }
    }
    public static void DestroyPooled<T>(T prefab) where T : Component
    {
        DestroyPooled(prefab.gameObject);
    }

    public static void DestroyAll(GameObject prefab)
    {
        RecycleAll(prefab);
        DestroyPooled(prefab);
    }
    public static void DestroyAll<T>(T prefab) where T : Component
    {
        DestroyAll(prefab.gameObject);
    }
}
public static class ObjectPoolExtensions
{
    public static void CreatePool<T>(this T prefab) where T : Component
    {
        ManagerObjectPool.CreatePool(prefab, 0);
    }
    public static void CreatePool<T>(this T prefab, int initialPoolSize) where T : Component
    {
        ManagerObjectPool.CreatePool(prefab, initialPoolSize);
    }
    public static void CreatePool(this GameObject prefab)
    {
        ManagerObjectPool.CreatePool(prefab, 0);
    }
    public static void CreatePool(this GameObject prefab, int initialPoolSize)
    {
        ManagerObjectPool.CreatePool(prefab, initialPoolSize);
    }

    public static T Spawn<T>(this T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
    {
        return ManagerObjectPool.Spawn(prefab, parent, position, rotation);
    }
    public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        return ManagerObjectPool.Spawn(prefab, null, position, rotation);
    }
    public static T Spawn<T>(this T prefab, Transform parent, Vector3 position) where T : Component
    {
        return ManagerObjectPool.Spawn(prefab, parent, position, Quaternion.identity);
    }
    public static T Spawn<T>(this T prefab, Vector3 position) where T : Component
    {
        return ManagerObjectPool.Spawn(prefab, null, position, Quaternion.identity);
    }
    public static T Spawn<T>(this T prefab, Transform parent) where T : Component
    {
        return ManagerObjectPool.Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
    }
    public static T Spawn<T>(this T prefab) where T : Component
    {
        return ManagerObjectPool.Spawn(prefab, null, Vector3.zero, Quaternion.identity);
    }
    public static GameObject Spawn(this GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
    {
        return ManagerObjectPool.Spawn(prefab, parent, position, rotation);
    }
    public static GameObject Spawn(this GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return ManagerObjectPool.Spawn(prefab, null, position, rotation);
    }
    public static GameObject Spawn(this GameObject prefab, Transform parent, Vector3 position)
    {
        return ManagerObjectPool.Spawn(prefab, parent, position, Quaternion.identity);
    }
    public static GameObject Spawn(this GameObject prefab, Vector3 position)
    {
        return ManagerObjectPool.Spawn(prefab, null, position, Quaternion.identity);
    }
    public static GameObject Spawn(this GameObject prefab, Transform parent)
    {
        return ManagerObjectPool.Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
    }
    public static GameObject Spawn(this GameObject prefab)
    {
        return ManagerObjectPool.Spawn(prefab, null, Vector3.zero, Quaternion.identity);
    }

    public static void Recycle<T>(this T obj) where T : Component
    {
        ManagerObjectPool.Recycle(obj);
    }
    public static void Recycle(this GameObject obj)
    {
        ManagerObjectPool.Recycle(obj);
    }

    public static void RecycleAll<T>(this T prefab) where T : Component
    {
        ManagerObjectPool.RecycleAll(prefab);
    }
    public static void RecycleAll(this GameObject prefab)
    {
        ManagerObjectPool.RecycleAll(prefab);
    }

    public static int CountPooled<T>(this T prefab) where T : Component
    {
        return ManagerObjectPool.CountPooled(prefab);
    }
    public static int CountPooled(this GameObject prefab)
    {
        return ManagerObjectPool.CountPooled(prefab);
    }

    public static int CountSpawned<T>(this T prefab) where T : Component
    {
        return ManagerObjectPool.CountSpawned(prefab);
    }
    public static int CountSpawned(this GameObject prefab)
    {
        return ManagerObjectPool.CountSpawned(prefab);
    }

    public static List<GameObject> GetSpawned(this GameObject prefab, List<GameObject> list, bool appendList)
    {
        return ManagerObjectPool.GetSpawned(prefab, list, appendList);
    }
    public static List<GameObject> GetSpawned(this GameObject prefab, List<GameObject> list)
    {
        return ManagerObjectPool.GetSpawned(prefab, list, false);
    }
    public static List<GameObject> GetSpawned(this GameObject prefab)
    {
        return ManagerObjectPool.GetSpawned(prefab, null, false);
    }
    public static List<T> GetSpawned<T>(this T prefab, List<T> list, bool appendList) where T : Component
    {
        return ManagerObjectPool.GetSpawned(prefab, list, appendList);
    }
    public static List<T> GetSpawned<T>(this T prefab, List<T> list) where T : Component
    {
        return ManagerObjectPool.GetSpawned(prefab, list, false);
    }
    public static List<T> GetSpawned<T>(this T prefab) where T : Component
    {
        return ManagerObjectPool.GetSpawned(prefab, null, false);
    }

    public static List<GameObject> GetPooled(this GameObject prefab, List<GameObject> list, bool appendList)
    {
        return ManagerObjectPool.GetPooled(prefab, list, appendList);
    }
    public static List<GameObject> GetPooled(this GameObject prefab, List<GameObject> list)
    {
        return ManagerObjectPool.GetPooled(prefab, list, false);
    }
    public static List<GameObject> GetPooled(this GameObject prefab)
    {
        return ManagerObjectPool.GetPooled(prefab, null, false);
    }
    public static List<T> GetPooled<T>(this T prefab, List<T> list, bool appendList) where T : Component
    {
        return ManagerObjectPool.GetPooled(prefab, list, appendList);
    }
    public static List<T> GetPooled<T>(this T prefab, List<T> list) where T : Component
    {
        return ManagerObjectPool.GetPooled(prefab, list, false);
    }
    public static List<T> GetPooled<T>(this T prefab) where T : Component
    {
        return ManagerObjectPool.GetPooled(prefab, null, false);
    }

    public static void DestroyPooled(this GameObject prefab)
    {
        ManagerObjectPool.DestroyPooled(prefab);
    }
    public static void DestroyPooled<T>(this T prefab) where T : Component
    {
        ManagerObjectPool.DestroyPooled(prefab.gameObject);
    }

    public static void DestroyAll(this GameObject prefab)
    {
        ManagerObjectPool.DestroyAll(prefab);
    }
    public static void DestroyAll<T>(this T prefab) where T : Component
    {
        ManagerObjectPool.DestroyAll(prefab.gameObject);
    }


}
