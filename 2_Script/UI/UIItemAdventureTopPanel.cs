using System.Collections;
using UnityEngine;

public abstract class UIItemAdventureTopPanel : MonoBehaviour
{
    [System.Serializable]
    protected struct AnimalInfo{
        public UIUrlTexture animalTextures;
        public UIItemWeightIcon weightIcon;
        [System.NonSerialized] public int animalIndex;
        [System.NonSerialized] public int attribute;
    }

    [System.Serializable]
    protected struct BossInfo
    {
        public UIUrlTexture bossTextures;
        [System.NonSerialized] public int attribute;
    }

    [Header("Animal and Boss")]
    [SerializeField] protected AnimalInfo[] animals;
    [SerializeField] protected BossInfo boss;

    [Header("Object")]
    [SerializeField] private UISprite bossAttributeIcon;
    [SerializeField] protected GameObject bossAttributeButton;
    [SerializeField] private GameObject helpBox;

    public void RepaintAnimal(int animalIdx)
    {
        int slotIndex = -1;
        for(int i=0; i<animals.Length; i++)
        {
            if (animals[i].animalIndex == animalIdx) slotIndex = i;
        }

        if (slotIndex == -1) return;

        SetAnimal(slotIndex, animalIdx);
    }

    public void SetAnimal(int slotIndex, int animalIdx)
    {
        if (slotIndex >= animals.Length)
            return;

        animals[slotIndex].animalIndex = animalIdx;

        var animalData = ManagerAdventure.Animal.GetAnimal(animalIdx);
        animals[slotIndex].attribute = animalData.attr;
        int lookId = ManagerAdventure.User.GetAnimalInstance(animalIdx)?.lookId ?? 0;

        int idx = slotIndex;    //람다캡쳐
        if (animals[slotIndex].animalTextures == null)
        {
            OnVsTextureLoaded(idx);
            return;
        }
        animals[slotIndex].animalTextures.SuccessEvent += () => OnVsTextureLoaded(idx);
        animals[slotIndex].animalTextures.LoadCDN(Global.adventureDirectory, "Animal/", ManagerAdventure.GetAnimalTextureFilename(animalIdx, lookId));
    }

    public void SetBoss(int bossId)
    {
        boss.bossTextures.SuccessEvent += () => OnVsTextureLoaded(-1);
        boss.bossTextures.LoadCDN(Global.adventureDirectory, "Animal/", string.Format("m_{0:D4}", bossId));
    }

    protected virtual void OnVsTextureLoaded(int idx)
    {
        if (idx == -1)
        {
            if(boss.bossTextures != null)
            {
                boss.bossTextures.MakePixelPerfect();
                boss.bossTextures.width = (int)(boss.bossTextures.width * 1.0f);
                boss.bossTextures.height = (int)(boss.bossTextures.height * 1.0f);
            }
        }
        else
        {
            if(animals[idx].animalTextures != null)
            {
                animals[idx].animalTextures.MakePixelPerfect();
                animals[idx].animalTextures.width = (int)(animals[idx].animalTextures.width * 0.8f);
                animals[idx].animalTextures.height = (int)(animals[idx].animalTextures.height * 0.8f);
            }
        }
    }

    protected virtual void SetBossAttribute(int value)
    {
        boss.attribute = value;

        if(value == 0)
        {
            bossAttributeButton.gameObject.SetActive(false);
        }
        else
        {
            bossAttributeButton.gameObject.SetActive(true);
            bossAttributeIcon.spriteName = "animal_attr_" + value.ToString();
        }
    }

    public virtual void SetWeight()
    {
        StageInfo stageInfo = StageUtility.StageInfoDecryption(StageUtility.lastLoadedStageMapData);
        SetBossAttribute(stageInfo.bossInfo.attribute);

        for (int i = 0; i < animals.Length; i++)
        {
            int weight = AttributeCalculator.Calculate(boss.attribute, animals[i].attribute, stageInfo.bossInfo.attrSize);

            animals[i].weightIcon.InitWeigth();
            animals[i].weightIcon.AddWeigth
            (
                new UIItemWeightIcon.WeightInfo()
                {
                    weightType = (UIItemWeightIcon.WeightType)animals[i].attribute,
                    weight = weight
                }
            ); 

            animals[i].weightIcon.OnIcon();
        }
    }

    public void OnClickBossAttribute()
    {
        if (!helpBox.activeSelf)
            StartCoroutine("OnBossAttributeAni");
        else
        {
            StopCoroutine("OnBossAttributeAni");
            helpBox.SetActive(false);
        }
    }

    private IEnumerator OnBossAttributeAni()
    {
        helpBox.SetActive(true);

        yield return new WaitForSeconds(2.0f);

        helpBox.SetActive(false);
    }
}

public static class AttributeCalculator
{
    public enum Attribute
    {
        NoAttribute = 0,
        Scissors    = 1,
        Rock        = 2,
        Paper       = 3
    }

    public static int Calculate(Attribute bossAttr, Attribute animalAttr, int bossAttributeSize = 10)
    {
        if (bossAttr == Attribute.NoAttribute)
            return 0;

        int result = bossAttr - animalAttr;
        result = result % 2 != 0 ? result : result * -1;

        if (result == 0)    //비김
            return 0;
        else if (result > 0) //동물이 짐
            return bossAttributeSize * -1;
        else                //동물이 이김
            return bossAttributeSize;
    }

    public static int Calculate(int bossAttr, int animalAttr, int bossAttributeSize = 10)
    {
        return Calculate((Attribute)bossAttr, (Attribute)animalAttr, bossAttributeSize);
    }
}
