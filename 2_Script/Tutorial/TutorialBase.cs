using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBase : MonoBehaviour {

    [System.NonSerialized]
    public Transform _transform = null;
    public TutorialType _tutorialType;

    virtual public void Awake()
    {
        _transform = transform;

        //ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.TUTORIAL_S);


    }

	void OnDestroy()
    {
        if (ManagerTutorial._instance != null)
        {
            //ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.TUTORIAL_E);
            string growthyName = "TUTORIAL" + ((int)_tutorialType).ToString() + "_E";
            //ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEventTutorial(growthyName);
            Debug.Log(growthyName);


            ManagerTutorial._instance._current = null;
            ManagerTutorial._instance._playing = false;
        }
        //ManagerLobby._instance._state = TypeLobbyState.Wait;
    }
}
