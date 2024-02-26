using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupShowAPNG : UIPopupBase
{
    [SerializeField]
    private UIUrlTexture popupTexture;

    [SerializeField]
    private GameObject closeButtonObj;

    [SerializeField]
    private UILabel[] title;

    private const int MIN_WIDTH = 150;
    private const int MIN_HEIGHT = 150;

    public void InitPopup(string fileName, float? textureSizeOffset = null, Vector2? mainSpritePos = null, Vector2Int? popupSize = null)
    {
        //팝업 위치 설정
        if (mainSpritePos != null)
        {
            mainSprite.transform.localPosition = mainSpritePos.Value;
        }

        //타이틀
        string titleText = Global._instance.GetString("p_t_10");
        for (int i = 0; i < title.Length; i++)
        {
            title[i].text = titleText;
        }

        //팝업 사이즈 설정
        if (popupSize != null)
        {
            mainSprite.width = (popupSize.Value.x < MIN_WIDTH) ? MIN_WIDTH : popupSize.Value.x;
            mainSprite.height = (popupSize.Value.y < MIN_HEIGHT) ? MIN_HEIGHT : popupSize.Value.y;

            float offset_posX = (popupSize.Value.x - MIN_WIDTH) * 0.5f;
            float offset_posY = (popupSize.Value.y - MIN_HEIGHT) * 0.5f;

            //닫기 버튼 위치 설정.
            closeButtonObj.transform.localPosition = new Vector3(15 + offset_posX, 13 + offset_posY, closeButtonObj.transform.localPosition.z);

            //타이틀 위치 설정.
            if (title.Length > 0)
            {
                Vector3 tr = title[0].transform.localPosition;
                title[0].transform.localPosition = new Vector3(tr.x, 15 + offset_posY, tr.z);
            }
        }

        //APNG 로드.
        popupTexture.enabled = false;
        popupTexture.SuccessEvent += () =>
        {
            popupTexture.enabled = true;
            if (textureSizeOffset != null)
            {
                popupTexture.transform.localScale = Vector3.one * textureSizeOffset.Value;
            }
        };
        popupTexture.LoadStreaming($"APNG/{fileName}");
    }
}
