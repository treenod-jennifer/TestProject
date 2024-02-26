using UnityEngine;

public class UIPopUpDecoInfo : UIPopupBase
{
    [SerializeField] private UIUrlTexture texture_Main;
    [SerializeField] private UILabel label_Main;
    
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
    
    public void InitPopup(string path, string fileName, string infoKey)     // 폴더 경로, CDN 이미지 파일 이름, 텍스트 키 전달
    {
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, path, fileName, (texture) =>
        {
            texture_Main.mainTexture = texture;
        });
        
        label_Main.text = Global._instance.GetString(infoKey);
    }
}
