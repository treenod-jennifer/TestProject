using System.Collections;
using UnityEngine;

public class APNGPlayer : MonoBehaviour
{
    private UITexture targetTexture;
    private APNGInfo apngInfo;
    private Coroutine coroutine;

    private static bool APNGPlayCheck(UITexture targetTexture, APNGInfo apngInfo)
    {
        if (targetTexture == null) return false;

        if (targetTexture.gameObject == null) return false;

        if (apngInfo == null) return false;

        return true;
    }

    public static APNGPlayer Play(UITexture targetTexture, APNGInfo apngInfo)
    {
        if (!APNGPlayCheck(targetTexture, apngInfo)) return null;

        APNGPlayer player = targetTexture.gameObject.GetComponent<APNGPlayer>();

        if(player == null)
        {
            player = targetTexture.gameObject.AddComponent<APNGPlayer>();
        }
        
        player.Init(targetTexture, apngInfo);
        return player;
    }

    public static void Stop(UITexture targetTexture)
    {
        APNGPlayer player = targetTexture.gameObject.GetComponent<APNGPlayer>();

        if (player == null) return;

        player.APNGStop();
    }

    public void Init(UITexture targetTexture, APNGInfo apngInfo)
    {
        this.targetTexture = targetTexture;
        this.apngInfo = apngInfo;

        APNGPlay();
    }

    private void OnEnable()
    {
        APNGPlay();
    }

    private void APNGPlay()
    {
        APNGStop();

        if (!APNGPlayCheck(targetTexture, apngInfo))
        {
            return;
        }

        if (apngInfo.IsSimplePNG)
        {
            targetTexture.mainTexture = apngInfo[0].tex;
            return;
        }

        if (gameObject.activeInHierarchy)
        {
            coroutine = StartCoroutine(APNGUpdate());
        }
    }

    private void APNGStop()
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    private IEnumerator APNGUpdate()
    {
        int frameIdx = 0;
        while (true)
        {
            if(apngInfo[frameIdx].tex == null)
            {
                APNGStop();
                yield break;
            }

            targetTexture.mainTexture = apngInfo[frameIdx].tex;
            yield return new WaitForSeconds(apngInfo[frameIdx].interval);

            frameIdx++;
            frameIdx %= apngInfo.Length;
        }
    }
}
