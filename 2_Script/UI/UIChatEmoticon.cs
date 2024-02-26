using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using PokoAddressable;

public class UIChatEmoticon : MonoBehaviour
{
    public UITexture emoticonBubble;
    public UITexture emoticon;

    private TypeChatEmoticon emoticonType = TypeChatEmoticon.none;
    private bool bDestroy = false;

    public void MakeEmoticon(TypeChatEmoticon type, CHAT_SIDE chatSide, TypeCharacterType charType)
    {
        Vector3 position = SetPositionByType(charType);
        emoticonType = type;

        string path = string.Format("local_emoticon/{0}", emoticonType);
        gameObject.AddressableAssetLoad<Texture2D>(path, (texture) => emoticon.mainTexture = texture);

        if (chatSide == CHAT_SIDE.RIGHT)
        {
            emoticonBubble.flip = UIBasicSprite.Flip.Horizontally;
            position = new Vector3(position.x * -1, position.y, position.z);
        }
        
        gameObject.AddressableAssetLoad<Texture2D>("local_emoticon/emoticon_bubble", (texture) => emoticonBubble.mainTexture = texture);
        emoticonBubble.color = new Color(1f, 1f, 1f, 0f);
        DOTween.ToAlpha(() => emoticonBubble.color, x => emoticonBubble.color = x, 1f, 0.2f);
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        transform.localPosition = position;

        StartCoroutine(ChatEmoticonIdleAction());
    }

    public IEnumerator DestroyEmoticon()
    {
        bDestroy = true;
        transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.05f);
        DOTween.ToAlpha(() => emoticonBubble.color, x => emoticonBubble.color = x, 0f, 0.15f);
        yield return new WaitForSeconds(0.15f);
        Destroy(gameObject);
    }

    public TypeChatEmoticon GetEmoticonType()
    {
        return emoticonType;
    }

    private Vector3 SetPositionByType(TypeCharacterType charType)
    {
        Vector3 position = new Vector3(-115f, 200f, 0f);
        var charData = ManagerCharacter._instance.GetLive2DCharacter((int)charType);
        if( charData != null )
        {
            position = charData.emoticonOffset;
        }

        return position;
    }

    private IEnumerator ChatEmoticonIdleAction()
    {
        while(bDestroy == false)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 5f) * 8f);
            yield return null;
        }
        yield return null;
    }
}
