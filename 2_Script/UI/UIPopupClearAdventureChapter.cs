using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class UIPopupClearAdventureChapter : UIPopupBase {

    static public UIPopupClearAdventureChapter _instance = null;
    
    public GenericReward reward;

    public GameObject particleObj;

    [SerializeField]
    UITexture titleTex;
    [SerializeField]
    UITexture bgTex;

    [SerializeField] UILabel charScript;
    [SerializeField] UILabel descText;

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        base.OnDestroy();
        if (_instance == this)
            _instance = null;
    }

    public void Init(int chapterIdx)
    {
        var chapData = ManagerAdventure.Stage.GetChapter(chapterIdx);
        if( chapData != null)
        {
            var clearRewards = chapData.chapterClearReward;
            if (clearRewards.Count > 0)
                reward.SetReward(clearRewards[0]);
            else
                reward.gameObject.SetActive(false);
        }
    }

    // Use this for initialization
    void Start () {
        bgTex.mainTexture = Box.LoadResource<Texture2D>("UI/chapter_clear_bg");
        titleTex.mainTexture = Box.LoadResource<Texture2D>("UI/chapter_clear_text");
        this.charScript.text = Global._instance.GetString("p_adc_1");
        this.descText.text = Global._instance.GetString("p_adc_2");
        this.particleObj.SetActive(true);

        ManagerSound.AudioPlay(AudioLobby.Mission_Finish);
    }
}
