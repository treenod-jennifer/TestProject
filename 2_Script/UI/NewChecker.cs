using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class NewChecker
{
    private const int MAX_COUNT = 30;

    private Queue<string> checkList = null;

    private Queue<string> CheckList
    {
        get
        {
            if (checkList == null)
            {
                checkList = Load();
            }

            return checkList;
        }
    }

    private readonly string key;

    public NewChecker(string key)
    {
        this.key = key;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += (scene) =>
        {
            if (scene.name == "Lobby")
            {
                checkList = null;
            }
        };
    }

    private void Save(Queue<string> checkList)
    {
        int overCount = Mathf.Max(0, checkList.Count - MAX_COUNT);
        for (int i = 0; i < overCount; i++)
        {
            checkList.Dequeue();
        }

        string json = JsonConvert.SerializeObject(checkList);
        PlayerPrefs.SetString(key, json);
    }

    private Queue<string> Load()
    {
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            return JsonConvert.DeserializeObject<Queue<string>>(json);
        }
        else
        {
            return new Queue<string>();
        }
    }

    /// <summary>
    /// 확인 했다면, uniqueKey를 저장해서 New 표시를 보여주지 않도록 처리
    /// </summary>
    public void SetNew(string uniqueKey)
    {
        if (!CheckList.Contains(uniqueKey))
        {
            CheckList.Enqueue(uniqueKey);
            Save(CheckList);
        }
    }

    public bool IsNew(string uniqueKey)
    {
        return !CheckList.Contains(uniqueKey);
    }

    /// <summary>
    /// 하나라도 New가 있다면 true
    /// </summary>
    public bool IsNew(IEnumerable<string> uniqueKey)
    {
        foreach (var key in uniqueKey)
        {
            if (IsNew(key))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 배너를 확인했음에도 느낌표를 띄워야 하는 경우 관련 데이터 제거
    /// </summary>
    public void DeleteNew(string uniqueKey)
    {
        Queue<string> tempList = new Queue<string>();
        foreach (var str in CheckList)
        {
            if (str != uniqueKey)
                tempList.Enqueue(str);
        }
        checkList.Clear();
        checkList = tempList;
        Save(CheckList);
    }
}
