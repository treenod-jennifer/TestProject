using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetExpEventUI : MonoBehaviour
{
    [Header("Exp Object")]
    [SerializeField] private GameObject expEvent;
    [SerializeField] private UILabel[]    expLabel;

    private void Awake()
    {
        SetExpEvent(ManagerAdventure.GetActiveEvent_ExpUpMagnification());
    }

    public void SetExpEvent(int expMagnification)
    {
        if (expMagnification > 1)
        {
            expEvent.SetActive(true);
            for(int i = 0; i < expLabel.Length; i++)
                expLabel[i].text = $"X{expMagnification}";
        }
        else
        {
            expEvent.SetActive(false);
        }
    }
}
