using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[InitializeOnLoad]
public class Editor_ValueApplier {

    static Editor_ValueApplier instance = new Editor_ValueApplier();

    static Editor_ValueApplier()
    {
        Editor_ValueApplier.instance.Init();
        
    }

    private void Init()
    {
        Editor_AreaRedirect.Init();
        EditorApplication.playmodeStateChanged += this.PlayModeChanged;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void PlayModeChanged()
    {
        Debug.Log("PlayMode Changed Called");

        if( Application.isPlaying == true)
        {
            if (Editor_AreaRedirect.redirect)
            {
                ManagerLobby.areaRedirect.Clear();

                ManagerLobby.areaRedirect = Editor_AreaRedirect.rules;
            }
        }
    }
}
#endif