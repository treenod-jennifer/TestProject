using System.Collections;
using System.Collections.Generic;

public static class GenericHelper {
    static System.Random rng = new System.Random();
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void Shuffle<T>(this List<T> list, System.Random rand)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static string MembersToString<T>(this List<T> list)
    {
        string outString = "";
        for(int i = 0; i < list.Count; ++i)
        {
            outString += list[i].ToString() + ((i == list.Count - 1) ? "" :  ", ");
        }
        return outString;
    }
}
