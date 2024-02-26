using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerAdventure : MonoBehaviour {

    static bool isLoaded = false;
    static public ManagerAnimalInfo Animal;
    static public ManagerStageInfo Stage;

    static public UserData User;
    static public UserSession Current;

    public class UserSession
    {
        public int chapterIndex = 0;
        public int stageIndex = 0;
    }



    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    static public void OnInit()
    {
        if(isLoaded)
        {
            return;
        }

        isLoaded = true;

        Stage = new ManagerStageInfo();
        Animal = new ManagerAnimalInfo();
        User = new UserData();
        Current = new UserSession();

        Stage.SetTestData();
        Animal.SetTestData();
        User.SetTestData();

    }

    static public void OnReboot()
    {
        User.OnReboot();
        Animal.OnReboot();
        Stage.OnReboot();

        User = null;
        Animal = null;
        Stage = null;
        isLoaded = false;

    }
}
