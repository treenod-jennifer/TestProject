using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameHelpButton : MonoBehaviour {

    public void StageClear()
    {
        GameManager.instance.moveCount = 1;
        ManagerBlock.instance.score = 1000000;
        GameManager.instance.StageClear();
    }

    public void StageFail()
    {
        GameManager.instance.StageFail();
    }

    public void MakeBomb()
    {
        ManagerBlock.instance.MakeBombRandom();
    }

    public void ClearWave()
    {
        AdventureManager.instance.ClearWave();
    }

    public void PangAnimal()
    {
        AdventureManager.instance.PangAnimal();
    }

    public void PangEnemy()
    {
        AdventureManager.instance.PangEnemy();
    }

    public void HealAnimal()
    {
        AdventureManager.instance.HealAnimal();
    }
}
