using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerAdventure : MonoBehaviour {

    public class ManagerAnimalInfo
    {
        Dictionary<int, AnimalData> animalDic = new Dictionary<int, AnimalData>();
        Dictionary<int, AnimalEvoData> animalEvoDic = new Dictionary<int, AnimalEvoData>();


        public AnimalData GetAnimal(int animalIdx)
        {
            AnimalData animalData = null;
            animalDic.TryGetValue(animalIdx, out animalData);
            return animalData;
        }

        public void OnReboot()
        {

        }

        public void SetTestData()
        {

        }
    }

    
    public class AnimalData
    {
        int idx;
        string name;
        string script;
        int grade;
        int attr;
        int maxOverlap;
        int maxLv;

        int defHp;
        int defAtk;
        int atkType;

        int skill;        
    }

    class AnimalEvoData
    {
        int idx;
        int reqCoin;
        List<KeyValuePair<int, int>> reqMaterials;
    }

    class SkillData
    {
        int reqSp;
        int skillCooldown;
        List<int> skillValue;
    }

   
}
