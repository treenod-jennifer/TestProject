using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupLandMove : UIPopupBase
{
    public class LandData
    {
        public int landIndex;
        public bool IsActiveLand;

        public LandData() { }

        public LandData(int landIndex, bool IsActiveLand)
        {
            this.landIndex = landIndex;
            this.IsActiveLand = IsActiveLand;
        }
    }

    public class LandDataComparer : IComparer<LandData>
    {
        // -1 : a를 앞으로, 1: a를 뒤로, 0: 그대로.
        public int Compare(LandData a, LandData b)
        {
            //랜드 인덱스 정렬
            if (a.landIndex < b.landIndex)
                return -1;
            else if (a.landIndex > b.landIndex)
                return 1;
            else
                return 0;
        }
    }

    [Header("ObjectLink")]
    [SerializeField] private UIItemLandItem pokotownLandItem;
    [SerializeField] private Transform episodeLand;
    [SerializeField] private Transform housingLand;
    [SerializeField] private GameObject UILandItem;
    [SerializeField] private GameObject objLandIcon;

    private List<LandData> LandHousing = new List<LandData>();
    private List<LandData> LandEpisode = new List<LandData>();
    private Dictionary<int, List<string>> outlandData = new Dictionary<int, List<string>>();

    LandDataComparer landDataComparer = new LandDataComparer();

    public static bool canClickMoveLand = false;

    protected virtual void Start()
    {
        outlandData = ServerContents.Day.outlands;

        SetLandIndex();
        SetLand();
        SetLandIconPosition();

        canClickMoveLand = true;
    }

    void SetLandIndex()
    {
        if (outlandData == null) return;

        foreach(var outlandItme in outlandData)
        {
            bool active = false;

            active = outlandItme.Value.Count > 0 ? true : false;

            if (outlandItme.Key.ToString().Length > 2)
            {
                LandHousing.Add(new LandData(outlandItme.Key, active));
            }
            else
            {
                LandEpisode.Add(new LandData(outlandItme.Key, active));
            }
        }
        LandHousing.Sort(landDataComparer);
        LandHousing.Reverse();
        LandEpisode.Sort(landDataComparer);
        LandEpisode.Reverse();
    }

    void SetLandIconPosition()
    {
        int selectLandIndex = ManagerLobby.landIndex;
        Transform selectLandPos = null;

        if(selectLandIndex > 99)
        {
            int index = selectLandIndex % 100;

            var housingLandArray = housingLand.GetComponentsInChildren<UIItemLandItem>();

            System.Array.Reverse(housingLandArray);

            selectLandPos = housingLandArray[index].transform;
        }
        else if(selectLandIndex == 0)
        {
            selectLandPos = pokotownLandItem.gameObject.transform;
        }
        else
        {
            var episodeLandArray = episodeLand.GetComponentsInChildren<UIItemLandItem>();

            System.Array.Reverse(episodeLandArray);

            selectLandPos = episodeLandArray[selectLandIndex - 1].transform;
        }
        

        objLandIcon.transform.SetParent(selectLandPos);

        objLandIcon.transform.localPosition = Vector3.zero;
    }

    void SetLand()
    {
        pokotownLandItem.UpdataData(new LandData(0, true));

        if(LandHousing.Count > 0)
        {
            for (int i = 0; i < LandHousing.Count; i++)
            {
                UIItemLandItem itemRoot = NGUITools.AddChild(housingLand, UILandItem).GetComponent<UIItemLandItem>();

                itemRoot.UpdataData(LandHousing[i]);
            }
        }

        if(LandEpisode.Count > 0)
        {
            for (int i = 0; i < LandEpisode.Count; i++)
            {
                UIItemLandItem itemRoot = NGUITools.AddChild(episodeLand, UILandItem).GetComponent<UIItemLandItem>();

                itemRoot.UpdataData(LandEpisode[i]);
            }
        }

        episodeLand.GetComponent<UIGrid>().Reposition();
        housingLand.GetComponent<UIGrid>().Reposition();

    }
}
