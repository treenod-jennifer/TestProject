using System.Collections;
using UnityEngine;

public class UIPopupDecoCollectionList : UIPopupBase
{
    [SerializeField] private UITexture texture_TitleBg;
    [SerializeField] private UITexture texture_Bg;
    [SerializeField] private UITexture texture_Info;
    [SerializeField] private UILabel text_Timer;
    [SerializeField] private UIPanel panel_Scroll;

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

    public void Init(long endTs, Texture titleTexture)
    {
        texture_TitleBg.mainTexture = titleTexture;

        text_Timer.text = Global.GetTimeText_MMDDHHMM_Plus1(endTs);
        
        float sizeX = 0;
        float sizeY = 0;
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "DecoCollection/", $"declDecoList.png", (texture) =>
        {
            if (texture != null)
            {            
                sizeX = texture_Bg.width;
                sizeY = texture.height * ((float)texture_Bg.width / (float)texture.width);
            
                texture_Bg.mainTexture = texture;
            }
            
            texture_Bg.width = (int) sizeX;
            texture_Bg.height = (int) sizeY;

            panel_Scroll.depth = uiPanel.depth + 1;
            
            Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "DecoCollection/", $"decocollection_info.png", (texture2) =>
            {
                if (texture2 != null)
                {
                    texture_Info.mainTexture = texture2;
                    texture_Info.transform.localPosition = new Vector3(0, texture_Bg.transform.localPosition.y - sizeY - 10f, 0);
                }
            });
        });
        
        // 팝업 진입 관련 그로시 로그 전송
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.DECO_COLLECTION,
            string.Format("DECOCOLLECTION_SCROLLPOPUP_OPEN_{0}", ManagerDecoCollectionEvent.DecoCollectionEventIndex),
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
        );
        var d = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
    }
}
