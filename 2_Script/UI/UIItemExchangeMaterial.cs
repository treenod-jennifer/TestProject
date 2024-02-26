using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIItemExchangeMaterial : MonoBehaviour
{
    private enum CountType
    {
        Non,
        Plus,
        Minus
    }

    [SerializeField] private bool isUpdate = true;
    [SerializeField] private CountType countType = CountType.Non;

    [SerializeField] private Transform scaleRoot;

    [SerializeField] private UIMaterialTexture materialTexture;
    [SerializeField] private UILabel materialCount;

    [SerializeField] private UISprite bgSprite01;
    [SerializeField] private UISprite bgSprite02;

    [SerializeField] private Color maxColor = Color.white;
    [SerializeField] private Color minColor = Color.white;

    private ServerUserMaterial materialData;

    public event Action<UIItemExchangeMaterial> CenterOnEvent;

    private bool isCenter = false;

    [SerializeField] private float maxScale = 2.0f;
    [SerializeField] private float minScale = 1.0f;

    private const int OUTLINE_SIZE = 5; //뒷 프레임의 테두리 사이즈
    private const float DISTANCE_RATIO = 5.0f;  //거리 1당 줄일 사이즈의 크기
    private const float CENTER_CHECK_RATIO = 0.9f;  //센터로 판단할 범위
    private Vector2 bgDefaultSize;

    private void Start()
    {
        bgDefaultSize = new Vector2(bgSprite01.width, bgSprite01.height);
    }

    private void Update()
    {
        if (!isUpdate) return;

        float distance = Mathf.Abs(transform.position.x);
        float size = Mathf.Clamp(maxScale - (distance * DISTANCE_RATIO), minScale, maxScale);
        scaleRoot.localScale = Vector3.one * size;

        int width = Mathf.RoundToInt(bgDefaultSize.x * size);
        int height = Mathf.RoundToInt(bgDefaultSize.y * size);

        bgSprite01.width = width;
        bgSprite01.height = height;
        bgSprite02.width = width;
        bgSprite02.height = height;

        bgSprite01.transform.localScale = new Vector3
        (
            (float)(width + OUTLINE_SIZE) / width * bgSprite02.transform.localScale.x,
            (float)(height + OUTLINE_SIZE) / height * bgSprite02.transform.localScale.y,
            1.0f
        );

        bgSprite02.color = Color.Lerp(minColor, maxColor, (size - minScale)/(maxScale - minScale));

        if (size > maxScale * CENTER_CHECK_RATIO)
        {
            if (!isCenter)
            {
                isCenter = true;
                CenterOnEvent?.Invoke(this);
            }
        }
        else
        {
            if (isCenter)
            {
                isCenter = false;
            }
        }
    }

    public void InitMaterial(ServerUserMaterial materialData)
    {
        this.materialData = materialData;
        materialTexture.InitMaterialTexture(materialData.index, 100, 100);
        SetCount(materialData.count);
    }

    public void SetCount(int count)
    {
        string countString = string.Empty;
        switch (countType)
        {
            case CountType.Non:
                break;
            case CountType.Plus:
                countString += "+";
                break;
            case CountType.Minus:
                countString += "-";
                break;
            default:
                break;
        }

        countString += count.ToString();

        materialCount.text = countString;
    }

    public int GetCount()
    {
        return int.Parse(materialCount.text);
    }

    public ServerUserMaterial GetMaterial()
    {
        return materialData;
    }
}
