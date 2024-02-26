using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIPopUpLandSelect: UIPopupBase
{
    public List<UIPokoButton> landButtons;


    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        
        landButtons[0].SetLabel("To mainland");

        

        for (int i = 1; i < landButtons.Count; ++i)
        {
            if( ServerContents.Day.outlands != null && ServerContents.Day.outlands.ContainsKey(i))
            {
                landButtons[i].gameObject.SetActive(true);
                landButtons[i].SetLabel($"To Land {i}");
            }
            else landButtons[i].gameObject.SetActive(false);
            
        }
    }

    void OnClickMoveMainland()
    {
        Global._instance.StartCoroutine(UIPopUpLandSelect.ToOutland(0));
        ClosePopUp();
        
    }

    void OnClickMoveOutland1()
    {
        Global._instance.StartCoroutine(UIPopUpLandSelect.ToOutland(1));
        ClosePopUp();

    }
    void OnClickMoveOutland2()
    {
        Global._instance.StartCoroutine(UIPopUpLandSelect.ToOutland(2));
        ClosePopUp();

    }
    void OnClickMoveOutland3()
    {
        Global._instance.StartCoroutine(UIPopUpLandSelect.ToOutland(3));
        ClosePopUp();

    }

    static IEnumerator ToMainland()
    {
        yield return new WaitForSeconds(0.2f);
        ManagerLobby.landIndex = 0;
        ManagerLobby._instance.EndDay();

    }

    static IEnumerator ToOutland(int i)
    {
        yield return new WaitForSeconds(0.2f);
        
        ManagerLobby._instance.MoveLand(i);

    }
}
