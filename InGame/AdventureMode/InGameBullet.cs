using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameBullet : MonoBehaviour
{
    [SerializeField] private AnimationCurve moveController_X;
    [SerializeField] private AnimationCurve moveController_Y;
    [SerializeField] private Transform startRoot;
    [SerializeField] private UITexture bullet;

    private string fileName;

    private ResourceBox box;

    private void Awake()
    {
        box = ResourceBox.Make(gameObject);
    }

    public void SetCDNTexture(string fileName)
    {
        if (this.fileName == fileName)
            return;

        this.fileName = fileName;

        box.LoadCDN(
            "Effect",
            fileName, 
            (Texture2D texture) => {
                bullet.mainTexture = texture;
                bullet.MakePixelPerfect();
            }
        );
    }

    private string resourcesPath;
    public void SetLocalTexture(string resourcesPath)
    {
        if (this.resourcesPath == resourcesPath)
            return;

        this.resourcesPath = resourcesPath;
        
        bullet.mainTexture = box.LoadResource<Texture2D>(resourcesPath);
        bullet.MakePixelPerfect();
    }

    public void Shot(Transform target, System.Action shotEndEven = null)
    {
        StartCoroutine(ShotAni(target, shotEndEven));
    }

    private IEnumerator ShotAni(Transform target, System.Action shotEndEvent)
    {
        Vector3 startPos = startRoot.position + Vector3.up * 0.2f;

        float movePosX = target.transform.position.x - startPos.x;
        float endPosY = target.transform.position.y;

        float posX;
        float posY;
        float totalTime = 0.0f;
        float endTime = Mathf.Max
                        (
                            moveController_X.keys[moveController_X.length - 1].time,
                            moveController_Y.keys[moveController_Y.length - 1].time
                        );

        bullet.gameObject.SetActive(true);

        while (totalTime < endTime)
        {
            totalTime += Global.deltaTimePuzzle;

            posX = startPos.x + moveController_X.Evaluate(totalTime) * movePosX;
            posY = Mathf.Lerp(startPos.y, endPosY, totalTime / endTime) + moveController_Y.Evaluate(totalTime);

            bullet.transform.position = new Vector3(posX, posY, bullet.transform.position.z);

            yield return null;
        }

        bullet.gameObject.SetActive(false);

        if (shotEndEvent != null)
            shotEndEvent();
    }
}
