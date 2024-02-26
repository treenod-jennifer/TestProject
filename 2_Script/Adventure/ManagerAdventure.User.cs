using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerAdventure : MonoBehaviour {

    public class UserData
    {
        Dictionary<int, UserDataAnimal> animalDic = new Dictionary<int, UserDataAnimal>();
        Dictionary<int, UserDataChapterProgress> chapterProgress = new Dictionary<int, UserDataChapterProgress>();

        List<Deck> decks = new List<Deck>(3);

        public void SyncFromServerData()
        {

        }

        public int GetAnimalIdxFromDeck(int deckIdx, int posInDeck)
        {
            return decks[deckIdx].party[posInDeck];
        }

        public UserDataAnimal GetAnimalFromDeck(int deckIdx, int posInDeck)
        {
            return animalDic[decks[deckIdx].party[posInDeck]];
        }

        public bool SetAnimalToDeck(int deckIdx, int posInDeck, int animalIdx)
        {
            decks[deckIdx].party[animalIdx] = animalIdx;
            return true;
        }

        public UserDataChapterProgress GetChapterProgress(int chapterIdx)
        {
            UserDataChapterProgress cp = null;
            chapterProgress.TryGetValue(chapterIdx, out cp);
            return cp;
        }

        public void OnReboot()
        {

        }

        public void SetTestData()
        {

        }
    }


    public class UserDataChapterProgress
    {
        public Dictionary<int, UserDataStageProgress> stageProgress = new Dictionary<int, UserDataStageProgress>();
        public UserDataStageProgress GetStageProgress(int stageIdx)
        {
            UserDataStageProgress sp = null;
            stageProgress.TryGetValue(stageIdx, out sp);
            return sp;
        }
    }

    public class UserDataStageProgress
    {
        public int playCount;
        public int clearLevel;
    }

    public class Deck
    {
        public List<int> party = new List<int>(3);
    }


    public class UserDataAnimal
    {
        int animalIdx;
        int grade;
        int level;
        int exp;
        int overlap;
    }

    public class AnimalInstance
    {
        internal int idx;
        internal long gettime;

        internal int grade;
        internal int level;
        internal int exp;
        internal int overlap;

        internal int attr;


        internal int hp;
        internal int atk;

        internal int atkType;
        internal int animalSize;

        internal int skill;
        internal int skillGrade;
    }

    public class AnimalGradeInfo
    {
        internal int maxLevel = 10;
        internal int maxOverlap = 20;
        internal int lifePerLevel = 100;
        internal int atkPerLevel = 10;

        internal int lifePerOverlap = 100;
        internal int atkPerOverlap = 10;
    }
}
