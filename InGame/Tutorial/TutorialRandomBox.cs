using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRandomBox : TutorialBase
{
    public List<DecoBase> GetDecoBase_RandomBox()
    {
        List<DecoBase> listDecoBase = new List<DecoBase>();

        foreach(var tempBoard in ManagerBlock.boards)
        {
            if(tempBoard != null && tempBoard.DecoOnBoard != null)
            {
                foreach(var tempDeco in tempBoard.DecoOnBoard)
                {
                    RandomBox box = tempDeco as RandomBox;

                    if(box != null)
                    {
                        listDecoBase.Add(tempDeco);
                        break;
                    }
                }
            }
        }

        return listDecoBase;
    }
    public List<CustomBlindData> GetListCustomBlindData_RandomBox()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();
        foreach (var tempBoard in ManagerBlock.boards)
        {
            if (tempBoard != null && tempBoard.DecoOnBoard != null)
            {
                foreach (var tempDeco in tempBoard.DecoOnBoard)
                {
                    RandomBox box = tempDeco as RandomBox;

                    if (box != null)
                    {
                        CustomBlindData data = new CustomBlindData(box.transform.localPosition + new Vector3(2f, 2f, 0f), 90, 85);
                        listCustomBlindData.Add(data);
                        break;
                    }

                }
            }
        }
        return listCustomBlindData;
    }

}
