using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public partial class ManagerAdventure : MonoBehaviour {

    private const string animalFilename = "Animal/Animal.xml";
    private const string animalFilename_enc = "Animal/Animal.exm";
    private const string animalTextFilename = "Adventure/c_1_1.json";
    private string stageFilename = "testData";
    private string userFilename = "testData";

    private static ResourceBox box;
    private static ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(instance.gameObject);
            }

            return box;
        }
    }

    static IEnumerator Load()
    {
        yield return CoLoadAnimalString(animalTextFilename);
        yield return COLoad_EncData(animalFilename_enc, SetAnimalData);
        //yield return COLoad_Data(animalFilename, SetAnimalData);

        yield break;
    }

    static private IEnumerator CoLoadAnimalString(string fileName)
    {
        bool isComplete = false;

        instance._stringData.Clear();
        StringHelper.LoadStringFromCDN(fileName, instance._stringData, complete: (c) => isComplete = true);

        yield return new WaitUntil(() => isComplete);
    }

    static private IEnumerator COLoad_Data(string fileName, System.Action<string> setData)
    {
        string loadPath = Global._cdnAddress + fileName;

        using(UnityWebRequest www = UnityWebRequest.Get(loadPath))
        {
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null)
            {
                if (www.downloadHandler.data.Length > 0)
                {
                    setData(www.downloadHandler.text);
                }
            }

        }
        
    }

    static private IEnumerator COLoad_EncData(string fileName, System.Action<string> setData)
    {
        string loadPath = Global._cdnAddress + fileName;

        using (UnityWebRequest www = UnityWebRequest.Get(loadPath) )
        {
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null)
            {
                if (www.downloadHandler.data?.Length > 0)
                {
                    MemoryStream memoryStream = new MemoryStream(www.downloadHandler.data);
                    MemoryStream outMemoryStream = new MemoryStream();

                    SharpAESCrypt.SharpAESCrypt.Decrypt("iGki2W12fM93h8UA", memoryStream, outMemoryStream);
                    outMemoryStream.Position = 0;

                    string text = new StreamReader(outMemoryStream).ReadToEnd();

                    setData(text);
                }
            }

        }
       
    }

    static private void SetAnimalData(string xmlData)
    {
        Animal = new ManagerAnimalInfo();

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlData);

        XmlNodeList animals = doc.GetElementsByTagName("Animal");

        for (int i = 0; i < animals.Count; i++)
        {
            AnimalData aData = new AnimalData()
            {
                idx = xmlhelper.GetInt(animals[i], "idx"),
                limited = xmlhelper.GetInt(animals[i], "limited", 0),
                atkType = xmlhelper.GetInt(animals[i], "atk_type"),
                attr = xmlhelper.GetInt(animals[i], "attr"),
                defAtk = xmlhelper.GetInt(animals[i], "atk"),
                defHp = xmlhelper.GetInt(animals[i], "hp"),
                grade = xmlhelper.GetInt(animals[i], "grade"),
                maxLv = xmlhelper.GetInt(animals[i], "max_level"),
                maxOverlap = xmlhelper.GetInt(animals[i], "max_overlap"),
                skill = xmlhelper.GetInt(animals[i], "skill"),
                animalSize = xmlhelper.GetInt(animals[i], "size"),
                skillGrade = xmlhelper.GetInt(animals[i], "skill_grade", 1),
                lobbyCharIdx = xmlhelper.GetInt(animals[i], "lobby_char", -1),
                specialLobby = xmlhelper.GetInt(animals[i], "Special_Lobby_char", 0),
                protectedFromMelee = xmlhelper.GetBool(animals[i], "protected", false),
                output_jp = xmlhelper.GetInt(animals[i], "output_jp", 0),
                output_tw = xmlhelper.GetInt(animals[i], "output_tw", 0),
                bulletImageName = xmlhelper.GetString(animals[i], "bullet_image"),

                animalHitSoundName = xmlhelper.GetString(animals[i], "animalhit_sound"),
                animalDamageSoundName = xmlhelper.GetString(animals[i], "animaldamage_sound"),
                enemyHitSoundName = xmlhelper.GetString(animals[i], "enemyhit_sound"),
                enemyDamageSoundName = xmlhelper.GetString(animals[i], "enemydamage_sound"),

                damageEffectName_1 = xmlhelper.GetString(animals[i], "damage_effect_1"),
                damageEffectName_2 = xmlhelper.GetString(animals[i], "damage_effect_2"),
                hitEffectName_1 = xmlhelper.GetString(animals[i], "hit_effect_1"),
                hitEffectName_2 = xmlhelper.GetString(animals[i], "hit_effect_2"),

                tags = xmlhelper.GetString(animals[i], "tags"),

                endTs = xmlhelper.GetInt(animals[i], "end_ts")
            };

            Animal.AddAnimalData(aData.idx, aData);
        }
    }
    
    private void SetUserData(string xmlData)
    {
        User = new UserData();

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlData);

        XmlNodeList user = doc.GetElementsByTagName("User");

        //for (int i = 0; i < user.Count; i++)
        //{
            

        //    User.AddUserData_Animal
        //    User.AddUserData_CProgress
        //    User.AddUserData_Deck
        //}
    }

    static public string GetAnimalTextureFilename(int aniID, int lookId)
    {
        if(lookId == 0)
        {
            return string.Format($"at_{aniID:D4}");
        }

        return string.Format($"at_{aniID:D4}_{lookId:D1}");        
    }

    static public string GetAnimalProfileFilename(int aniID, int lookId)
    {
        if (lookId == 0)
        {
            return string.Format($"ap_{aniID:D4}");
        }

        return string.Format($"ap_{aniID:D4}_{lookId:D1}");
    }
}
