using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class NewMarkUtility
{
    static List<int> newAnimalList = new List<int>();

    static public bool isView { get; set; } = false;
    
    static public void newAnimalListAdd(int newAnimalIndex)
    {
        newAnimalList.Add(newAnimalIndex);
    }

    static public void newAnimalListReset()
    {
        newAnimalList = new List<int>();
    }

    static public void newAnimalDataSave()
    {
        if (newAnimalList.Count > 0)
        {
            PlayerPrefs.SetString("NewAnimalInstance", JsonUtility.ToJson(newAnimalList));
        }
    }

    static public void newAnimalDataLoad()
    {
        if (PlayerPrefs.HasKey("NewAnimalInstance"))
        {   
            var tmpList = JsonUtility.FromJson<List<int>>(PlayerPrefs.GetString("NewAnimalInstance"));
            for (int i = 0; i < tmpList.Count; i++)
                newAnimalList.Add(tmpList[i]);
        }
        else
        {
            newAnimalListReset();
        }
    }

    static public void newAnimalDataDelete()
    {
        PlayerPrefs.DeleteKey("NewAnimalInstance");
        newAnimalListReset();
        isView = false;
    }

    static public bool CompareNewList(int animalIndex)
    {
        for (int i = 0; i < newAnimalList.Count; i++)
        {
            if (animalIndex == newAnimalList[i])
            {
                return true;
            }
        }
        return false;
    }
}
