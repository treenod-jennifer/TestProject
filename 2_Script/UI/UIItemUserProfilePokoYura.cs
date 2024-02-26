using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemUserProfilePokoYura : MonoBehaviour
{
    public UITexture pokoYuraImage;
    public int index = 0;

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

    public void OnLoadComplete(Texture2D r)
    {
        pokoYuraImage.mainTexture = r;
        pokoYuraImage.MakePixelPerfect();
    }

    public void SetPokoYuraItemImgae(int pokoyuraIndex, int selectIndex)
    {
        index = pokoyuraIndex;
        string fileName = string.Format("y_i_{0}", index);
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "Pokoyura", fileName, OnLoadComplete, true);
        //현재 유저의 프로필 정보창이고, 포코유라가 선택된 포코유라이면 체크표시.
        if (UIPopupUserProfile._instance.GetUser() == true && index == selectIndex)
        {
            UIPopupUserProfile._instance.SettingCheck(transform);
        }
    }

    private void OnClickPokoYura()
    {
        if (index == 0 || UIPopupUserProfile._instance.GetUser() == false)
            return;
        UIPopupUserProfile._instance.OnClickPokoYura(index);
        UIPopupUserProfile._instance.PokoYuraPosition(transform);
    }
}
