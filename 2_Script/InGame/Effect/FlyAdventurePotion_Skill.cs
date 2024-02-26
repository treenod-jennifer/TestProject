using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyAdventurePotion_Skill : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPos;
    private float timer = 0;

    private void Start()
    {
        startPos = transform.position;
        targetPos = GameUIManager.instance.adventureItemGuideRoot.transform.position;
    }

    void Update ()
    {
        timer += Global.deltaTimePuzzle * 1.7f;
        if (timer > 1f)
        {
            //턴추가
            if (AdventureManager.instance != null)
            {
                AdventureManager.instance.GetSkillPotion(0.20f);
            }
            timer = 0f;
            InGameEffectMaker.instance.RemoveEffect(gameObject);
        }
        transform.position = Vector3.Lerp(startPos, targetPos, 1f - Mathf.Cos(timer * ManagerBlock.PI90));
    }
}
