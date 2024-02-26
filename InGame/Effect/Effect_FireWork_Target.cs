using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_FireWork_Target : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    public void EffectEnd()
    {
        animator.SetTrigger("EndTrigger");
        Destroy(gameObject, 3.0f);
    }
}
