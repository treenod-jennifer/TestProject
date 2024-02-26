using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemUserProfile_AnimalData : MonoBehaviour
{
    [SerializeField] private UITexture aniTexture;
    [SerializeField] private GameObject checkObj;

    private UserProfileData pData;

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

    public void UpdataData(UserProfileData profileData)
    {
        pData = profileData;

        if(pData == null)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }

        if(pData.selectionData)
        {
            checkObj.SetActive(true);
        }
        else
        {
            checkObj.SetActive(false);
        }

        aniTexture.mainTexture = null;

        Box.LoadCDN(
            "Adventure",
            "Animal",
            $"{pData.aniID}.png",
            (Texture2D texture) =>
            {
                aniTexture.mainTexture = texture;
            },
            true
        );
    }


    public void OnClickProfile()
    {
        UIPopUPUserProfileSelection._instance.SetCurrentProfileName(pData.aniID);
    }
}
