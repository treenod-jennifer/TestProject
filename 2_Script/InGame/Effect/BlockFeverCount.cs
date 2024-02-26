using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockFeverCount : MonoBehaviour
{
    public UILabel labelCount;
    public UISprite spriteIcon;

    private Transform _transform;
    private float delay = 0;

    private void Awake()
    {
        _transform = transform;
    }

    public void initFeverCount(Vector3 startPos, string score, float tempDelay = 0)
    {
        _transform.position = startPos;
        _transform.localPosition += Vector3.up * 37;
        labelCount.text = score;
        delay = tempDelay;
        SetIconPosition();
        StartCoroutine(CoAction());
    }

    private void SetIconPosition()
    {
        float iconPosX = spriteIcon.transform.localPosition.x + ((labelCount.text.Length - 1) * 8f);
        spriteIcon.transform.localPosition = new Vector3(iconPosX, spriteIcon.transform.localPosition.y, 0f);
    }

    IEnumerator CoAction()
    {
        if (delay > 0)
        {
            labelCount.gameObject.SetActive(false);
            spriteIcon.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(delay);
        labelCount.gameObject.SetActive(true);
        spriteIcon.gameObject.SetActive(true);

        float targetPosY = _transform.localPosition.y + 70f;
        _transform.DOLocalMoveY(targetPosY, 0.5f);

        _transform.DOScale(1.8f, 0.4f);
        yield return new WaitForSeconds(0.4f);
        _transform.DOScale(1f, 0.2f);
        yield return new WaitForSeconds(0.2f);
        DOTween.To(() => labelCount.color, x => labelCount.color = x, new Color(1f, 1f, 1f, 0f), 0.3f).SetOptions(true);
        DOTween.To(() => spriteIcon.color, x => spriteIcon.color = x, new Color(1f, 1f, 1f, 0f), 0.3f).SetOptions(true);
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
}
