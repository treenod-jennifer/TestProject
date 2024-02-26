using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemPokoYuraSelector : MonoBehaviour
{
    public UITexture pokoYuraImage;
    public UILabel labelNew;
    public UISprite sprCheck;
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

    public void SetPokoYuraItemImgae(int pokoyuraIndex)
    {

        index = pokoyuraIndex;

        sprCheck.gameObject.SetActive(Pokoyura.pokoyuraDeployCustom.Exists(x => x == index));
        labelNew.gameObject.SetActive(UIPopupPokoyuraSelector.prevPokoyuraList.Exists(x => x == index) == false);

        string fileName = string.Format("y_i_{0}", index);
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "Pokoyura", fileName, OnLoadComplete, true);
    }

    private void OnClickPokoYura()
    {
        if (index == 0)
            return;

        int idx = Pokoyura.pokoyuraDeployCustom.FindIndex(x => x == index);
        if ( idx != -1)
        {
            Pokoyura.pokoyuraDeployCustom[idx] = 0;
        }
        else
        {
            for(int i = 0; i < Pokoyura.pokoyuraDeployCustom.Count; ++i)
            {
                if( Pokoyura.pokoyuraDeployCustom[i] == 0 )
                {
                    Pokoyura.pokoyuraDeployCustom[i] = index;
                    break;
                }
            }
        }
        
        sprCheck.gameObject.SetActive(Pokoyura.pokoyuraDeployCustom.Exists(x => x == index));

        ManagerLobby._instance.ReMakePokoyura();
    }
}
