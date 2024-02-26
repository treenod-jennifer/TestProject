using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectActionBase : MonoBehaviour 
{
    private System.Action onFinished;

    protected bool isEditComplete = false;

    protected GameObject actionObj;
    protected BoxCollider editCollider;

    protected abstract void InitActionData ();
    public abstract void StopActionEvent ();
    public abstract void StopActionEvent ( bool isCallback );
    protected abstract IEnumerator ProcCompuseActionEvent ( GameObject actionObj, BoxCollider editCollider );
    protected abstract IEnumerator OnActionEvent ();

    public void StartActionEvent ( System.Action onFinished , GameObject actionObj, BoxCollider editCollider )
    {
        this.onFinished = onFinished;
        this.InitActionData();
        this.StartCoroutine( this.ProcCompuseActionEvent( actionObj, editCollider ) );
    }

    public void StartOnFinishedHandler ()
    {
        if( this.onFinished != null )
        { 
            this.onFinished();
        }
    }

}
