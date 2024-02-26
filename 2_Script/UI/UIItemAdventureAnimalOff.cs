using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemAdventureAnimalOff : MonoBehaviour {

    [SerializeField] private GameObject[] stars = new GameObject[5];
    [SerializeField] private UIUrlTexture animalFullShot;
    [SerializeField] private GameObject eventMark;
    [SerializeField] private UILabel endTs;

    private Coroutine CoEventTs = null;

    private int Stars
    {
        set
        {
            if (stars[0] != null)
            {
                for (int i = 0; i < stars.Length; i++)
                {
                    if (i < value)
                        stars[i].SetActive(true);
                    else
                        stars[i].SetActive(false);
                }
            }
        }
    }

    private bool EventMark
    {
        set
        {
            if (eventMark != null)
            {
                eventMark.SetActive(value);
            }
        }
    }

    private long EventTs
    {
        set
        {
            if(endTs != null)
            {
                if (Global.LeftTime(value) > 0)
                {
                    endTs.gameObject.SetActive(true);

                    CoEventTs = StartCoroutine(CoAdventureAnimalEndTs(value));
                }
            }
        }
    }

    public void SetAnimalSelect(ManagerAdventure.AnimalInstance aData)
    {
        endTs.gameObject.SetActive(false);

        if (CoEventTs != null)
        {
            StopCoroutine(CoEventTs);
            CoEventTs = null;
        }

        Stars = aData.grade;
        
        EventMark = ManagerAdventure.EventData.IsAdvEventBonusAnimal(aData.idx);

        EventTs = aData.endTs;

        if (animalFullShot != null)
            animalFullShot.LoadCDN(Global.adventureDirectory, "Animal/", ManagerAdventure.GetAnimalProfileFilename(aData.idx, aData.lookId));
    }

    IEnumerator CoAdventureAnimalEndTs(long time)
    {
        long leftTime = 0;
        
        while (gameObject.activeInHierarchy)
        {
            leftTime = Global.LeftTime(time);

            if (leftTime <= 0)
            {
                leftTime = 0;
                endTs.text = "00: 00: 00";
                yield break;
            }
            endTs.text = Global.GetTimeText_DDHHMM(time);
            yield return new WaitForSeconds(0.2f);
        }
    }

}
