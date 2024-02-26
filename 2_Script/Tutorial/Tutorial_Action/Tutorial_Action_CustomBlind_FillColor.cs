using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 커스텀 블라인드 색 채우기
/// </summary>
public class Tutorial_Action_CustomBlind_FillColor : Tutorial_Action
{
    public List<int> listKey = new List<int>();
    public Color blindColor = Color.black;

    public override void StartAction(System.Action endAction = null)
    {
        if (listKey.Count == 0)
        {
            var enumerator = ManagerTutorial._instance._current.dicCustomBlindData.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ManagerTutorial._instance._current.blind.customBlind.FillColorAtCustomBlindTexture(enumerator.Current.Value, blindColor);
            }
        }
        else
        {
            for (int i = 0; i < listKey.Count; i++)
            {
                int key = listKey[i];
                if (ManagerTutorial._instance._current.dicCustomBlindData.ContainsKey(key) == true)
                {
                    ManagerTutorial._instance._current.blind.customBlind.FillColorAtCustomBlindTexture(ManagerTutorial._instance._current.dicCustomBlindData[key], blindColor);
                }
            }
        }
        endAction.Invoke();
    }
}
