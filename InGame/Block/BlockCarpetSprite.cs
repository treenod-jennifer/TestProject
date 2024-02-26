using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCarpetSprite : MonoBehaviour
{
    public UISprite carpetSprite_1;
    public UISprite carpetSprite_2;

    public void InitCarpetSprite(int depth)
    {
        carpetSprite_1.depth = depth;
        carpetSprite_2.depth = depth;
        MakePixelPerfect(carpetSprite_1);
        MakePixelPerfect(carpetSprite_2);
    }

    private void MakePixelPerfect(UISprite sprite, float offset = 1.25f)
    {
        sprite.MakePixelPerfect();
        sprite.width = Mathf.RoundToInt(sprite.width * offset);
        sprite.height = Mathf.RoundToInt(sprite.height * offset);
    }
}
