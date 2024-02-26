using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyAIHints : MonoBehaviour {

    public Vector3 startPos;
    public List<LobbyAnimalAI.Circle> roamingArea = new List<LobbyAnimalAI.Circle>();

    public List<string> touchScripts = new List<string>();

    public List<string> monologs = new List<string>();

    public Range waitTimeRange = new Range() { min = 1.0f, max = 2.0f };
    public Range walkRange = new Range() { min = 2.0f, max = 5.0f };
    public Range runRange = new Range() { min = 10.0f, max = 20.0f };

    public Range monologInterval = new Range() { min = 3f, max = 10f };

    public BehaviourRatio behaviourRatio = new BehaviourRatio() { wait = 50, walk = 40, run = 10 };

    

    [System.Serializable]
    public struct BehaviourRatio
    {
        public int wait;
        public int walk;
        public int run;
    }

    [System.Serializable]
    public struct Range
    {
        public float min;
        public float max;

        public float GetInnerValue()
        {
            return Random.Range(min, max);
        }
    }

    public void SetRoamingArea(List<LobbyAnimalAI.Circle> roamingData)
    {
        if (roamingData.Count == 0)
            roamingArea.Clear();
        else
            roamingArea = new List<LobbyAnimalAI.Circle>(roamingData);
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
