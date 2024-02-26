using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIContainerEmotionIcon : MonoBehaviour 
{
    public static UIContainerEmotionIcon _instance = null;

   // private List<UIItemEmotionIcon> curEmotionData = new List<UIItemEmotionIcon>();
     
    public GameObject objEmotionIcon;
    public GameObject rootEmotionIcon;

    private string idArticle;
    public void InitData ( string idArticle )
    {

    }



    private void RefreshEmotionData  ()
    {

    }


    #region
//    int count = 0;
//    void Update()
//    {
//#if !UNITY_EDITOR
//        if( Input.touchCount == 1 )
//        {
//            count += 1;
//            this.RefreshEmotionData();
//        }
//        else if (Input.touchCount == 2)
//        {
//            if(count > 999)
//            {`
//                count = 999;
//            }
//            count -= 1;
//            this.RefreshEmotionData();
//        }
//        else if (Input.touchCount == 3)
//        {
//            count = 100;
//            this.RefreshEmotionData();
//        }
//        else if ( Input.touchCount == 4 )
//        {
//            count = 9999;
//            this.RefreshEmotionData();
//        }

//#elif UNITY_EDITOR
//        if (Input.GetMouseButton(0))
//        {
//            count += 1;
//            this.RefreshEmotionData();
//        }
//        else if ( Input.GetMouseButtonDown( 1 ) )
//        {
//            if(count > 999)
//            {
//                count = 999;
//            }
//            count -= 1;
//            this.RefreshEmotionData();
//        }
//#endif
//    }
    #endregion


       

    public void SelectEmotionIconHandler ()
    {
        this.RefreshEmotionData ();
    }

    private void OnClickUpdateEmotionData ()
    {
        if( UIPopupPanelEmotionIcon._instance == null )
        {
            if ( UIPopupClear._instance.bCanTouch == false )
                return;
            UIPopupClear._instance.bCanTouch = false;

            ManagerUI._instance.OpenPopupPanelEmotionIcon( this.SelectEmotionIconHandler, this.gameObject );
        }
    }
}
