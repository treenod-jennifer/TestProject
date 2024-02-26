using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BannerTypeDatas : MonoBehaviour
{
    public class Package_RandomBox_ShowAD
    {
        public AdManager.AdType adType;
        public int resourceIdx;
        public long expiredTime;
        public Reward[] rewards;
    }
}
