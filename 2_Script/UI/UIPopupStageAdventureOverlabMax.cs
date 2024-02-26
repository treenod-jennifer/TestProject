using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupStageAdventureOverlabMax : UIPopupBase {
    [SerializeField] private UILabel[] title;
    [SerializeField] private UILabel main_1;
    [SerializeField] private UILabel main_2;

    private void Start()
    {
        for(int i=0; i<title.Length; i++)
        {
            title[i].text = Global._instance.GetString("p_t_8");
        }

        main_1.text = Global._instance.GetString("n_s_30");

        main_2.text = Global._instance.GetString("n_s_31");
    }
}
