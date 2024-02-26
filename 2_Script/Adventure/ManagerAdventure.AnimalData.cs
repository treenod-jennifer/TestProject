using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerAdventure : MonoBehaviour {

    [SerializeField] private TextAsset animalExpTable;

    public class ManagerAnimalInfo
    {
        SortedDictionary<int, AnimalData> animalDic = new SortedDictionary<int, AnimalData>();

        public AnimalData GetAnimal(int animalIdx)
        {
            AnimalData animalData = null;
            animalDic.TryGetValue(animalIdx, out animalData);
            return animalData;
        }

        /// <summary>
        /// 로비 캐릭터 인덱스로 AnimalData 가져오는 함수.
        /// </summary>
        /// <param name="lobbyCharIdx"></param>
        /// <returns></returns>
        public AnimalData GetAnimal(TypeCharacterType lobbyCharIdx)
        {
            AnimalData animalData = null;

            foreach (var animal in animalDic)
            {
                if (animal.Value.lobbyCharIdx == (int)lobbyCharIdx)
                    animalData = animal.Value;
            }
            return animalData;
        }

        public void OnReboot()
        {
            animalDic.Clear();
        }

        public void SetTestData()
        {
            animalDic.Add(1, new AnimalData() { idx = 1, atkType = 0, attr = 1, defAtk = 10, defHp = 100, grade = 2, maxLv = 100, maxOverlap = 15, skill = 1 });
            animalDic.Add(2, new AnimalData() { idx = 2, atkType = 0, attr = 1, defAtk = 10, defHp = 100, grade = 2, maxLv = 100, maxOverlap = 15, skill = 1 });
            animalDic.Add(3, new AnimalData() { idx = 3, atkType = 0, attr = 1, defAtk = 10, defHp = 100, grade = 3, maxLv = 100, maxOverlap = 15, skill = 1 });
            animalDic.Add(4, new AnimalData() { idx = 4, atkType = 0, attr = 1, defAtk = 10, defHp = 100, grade = 4, maxLv = 100, maxOverlap = 15, skill = 1 });
            animalDic.Add(5, new AnimalData() { idx = 5, atkType = 0, attr = 1, defAtk = 10, defHp = 100, grade = 4, maxLv = 100, maxOverlap = 15, skill = 1 });
        }

        public void AddAnimalData(int index, AnimalData aData)
        {
            animalDic.Add(index, aData);
        }

        public SortedDictionary<int, AnimalData>.KeyCollection GetAnimalKeyList()
        {
            return animalDic.Keys;
        }

        public static int GetMaxOverlap(int animalIdx)
        {
            var animalBase = ManagerAdventure.Animal.GetAnimal(animalIdx);
            if (animalBase != null)
                return animalBase.maxOverlap;

            return 20;
        }

        public static int GetMaxLevel(int animalIdx)
        {
            var animalBase = ManagerAdventure.Animal.GetAnimal(animalIdx);

            //이렇게 처리하는게 맞지만 일단 임시로 밑에 코드가 돌도록
            //if (animalBase != null)
            //{
            //    return animalBase.maxLv;
            //}
            
            if (animalBase != null)
            {
                int maxLevel = 0;
                foreach(var level in GetExpTable(animalBase.grade))
                {
                    maxLevel++;
                }

                return maxLevel;
            }

            return 20;
        }

        private static AnimalExpTable expTable = null;

        public static Dictionary<int, AnimalExpInfo> GetExpTable(int grade)
        {
            if (expTable == null)
            {
                expTable = Newtonsoft.Json.JsonConvert.DeserializeObject<AnimalExpTable>(instance.animalExpTable.text);
            }

            switch (grade)
            {
                case 1: return expTable.grade1;
                case 2: return expTable.grade2;
                case 3: return expTable.grade3;
                case 4: return expTable.grade4;
                case 5: return expTable.grade5;
                default: return null;
            }
        }
    }

    public class AnimalData
    {
        internal int idx;
        internal int grade;
        internal int limited;
        internal int attr;
        internal int maxOverlap;
        internal int maxLv;

        internal int defHp;
        internal int defAtk;
        internal int atkType;
        internal int animalSize;

        internal int skill = 1;
        internal int skillGrade = 1;

        internal int lobbyCharIdx = -1; // 0 : 포코타, 0보다 작은 경우 연결없음
        internal int specialLobby = 0; // 로비 스페셜 배치 (최대 중첩으로 외형이 변경 될 때 로비 배치되는 외형도 변경)
        internal bool protectedFromMelee;   // 근접공격으로부터의 보호
        internal int output_jp;   // JP 환경 출력 여부
        internal int output_tw;   // TW 환경 출력 여부

        internal string bulletImageName;

        internal string animalHitSoundName;
        internal string animalDamageSoundName;
        internal string enemyHitSoundName;
        internal string enemyDamageSoundName;

        internal string damageEffectName_1;
        internal string damageEffectName_2;
        internal string hitEffectName_1;
        internal string hitEffectName_2;

        internal string tags;

        internal long endTs;
    }
}
