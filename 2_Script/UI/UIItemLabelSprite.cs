using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemLabelSprite : MonoBehaviour
{
    [SerializeField] private UILabel mainLabel;
    [SerializeField] private UISprite mainSprite;
    [SerializeField] private Transform root;

    [Tooltip("'@test' : Sprite\n'#test' : GlobalString\n'test' : String")]
    [SerializeField] private string mainText;
    [SerializeField] private bool isGlobalKey = false;

    private void Awake()
    {
        mainLabel.gameObject.SetActive(false);
        mainSprite.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (isGlobalKey)
            mainText = Global._instance.GetString(mainText);

        var texts = mainText.Split('/');
        SetText(texts);
    }

    private int startPos;
    public void SetText(params string[] texts)
    {
        if (texts.Length == 0)
            return;

        startPos = 0;

        foreach(var text in texts)
        {
            var type = CheckSprite(text);
            if (type == TextType.Sprite)
                GetSpriteText(text.Remove(0, 1));
            else if (type == TextType.GlobalLabel)
                GetLabelText(Global._instance.GetString(text.Remove(0, 1)));
            else
                GetLabelText(text);
        }

        root.localPosition = new Vector3(Mathf.RoundToInt(startPos * -0.5f), root.localPosition.y, root.localPosition.z);
    }

    private enum TextType
    {
        Sprite,
        Label,
        GlobalLabel
    }

    private TextType CheckSprite(string text)
    {
        switch (text[0])
        {
            case '@':
                return TextType.Sprite;
            case '#':
                return TextType.GlobalLabel;
            default:
                return TextType.Label;
        }
    }

    private UISprite GetSpriteText(string spriteName)
    {
        UISprite sprite = Instantiate(mainSprite, root);
        sprite.gameObject.SetActive(true);

        sprite.spriteName = spriteName;
        sprite.MakePixelPerfect();

        startPos += Mathf.CeilToInt(sprite.width * 0.5f);
        sprite.transform.localPosition = new Vector3(startPos, sprite.transform.localPosition.y, sprite.transform.localPosition.z);
        startPos += Mathf.FloorToInt(sprite.width * 0.5f);

        return sprite;
    }

    private UILabel GetLabelText(string text)
    {
        UILabel label = Instantiate(mainLabel, root);
        label.gameObject.SetActive(true);

        label.text = text;

        startPos += Mathf.CeilToInt(label.width * 0.5f);
        label.transform.localPosition = new Vector3(startPos, label.transform.localPosition.y, label.transform.localPosition.z);
        startPos += Mathf.FloorToInt(label.width * 0.5f);

        return label;
    }
}
