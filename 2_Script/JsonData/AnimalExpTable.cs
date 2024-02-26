using System.Collections.Generic;

[System.Serializable]
public class AnimalExpInfo
{
    public int startExp;
    public int endExp;

    public AnimalExpInfo(int startExp, int endExp)
    {
        this.startExp = startExp;
        this.endExp = endExp;
    }
}

[System.Serializable]
public class AnimalExpTable
{
    public Dictionary<int, AnimalExpInfo> grade1 = new Dictionary<int, AnimalExpInfo>();
    public Dictionary<int, AnimalExpInfo> grade2 = new Dictionary<int, AnimalExpInfo>();
    public Dictionary<int, AnimalExpInfo> grade3 = new Dictionary<int, AnimalExpInfo>();
    public Dictionary<int, AnimalExpInfo> grade4 = new Dictionary<int, AnimalExpInfo>();
    public Dictionary<int, AnimalExpInfo> grade5 = new Dictionary<int, AnimalExpInfo>();
}