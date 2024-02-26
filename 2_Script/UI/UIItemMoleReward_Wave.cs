using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class UIItemMoleReward_Wave : MonoBehaviour, IMoleReward
{
    [SerializeField]UITweener[] tweens;

    [SerializeField] UIUrlTexture texture;
    [SerializeField] UITexture[] twinkles;
    [SerializeField] Texture2D[] twinkleTex;
    [SerializeField] GameObject beamLightsRoot;
    public int waveIndex;

    private Coroutine backRoutine = null;

	// Use this for initialization
	void Start () {

        var rwFilename = string.Format("rw_mc_{0}_{1}", ManagerMoleCatch.instance.GetEventIndex(), waveIndex);
        texture.SuccessEvent += OnTextureLoaded;
        texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", rwFilename);
	}

    public void OnTextureLoaded()
    {
        texture.MakePixelPerfect();
        texture.color = new Color(0, 0, 0, 0.7f);
    }

    public IEnumerator CoAppear()
    {
        for (int i = 0; i < tweens.Length; ++i)
        {
            tweens[i].enabled = true;
        }
        var fromColor = new Color(0, 0, 0, 0.7f);
        var toColor = new Color(1, 1, 1, 1);

        float a = 0f;
        while (a <= 1.0f)
        {
            a += Time.deltaTime * 1.0f;

            texture.color = Color.Lerp(fromColor, toColor, a);
            yield return null;
        }
        a = 1.0f;
        texture.color = Color.Lerp(fromColor, toColor, a);

        beamLightsRoot.SetActive(true);
        backRoutine = StartCoroutine(CoBackDeco());
        for (int i = 0; i < twinkles.Length; ++i)
        {
            twinkles[i].gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(2.5f);

        ManagerSound.AudioPlay(AudioInGame.COMBO1);
    }

    public IEnumerator CoDisappear()
    {
        yield return new WaitForSeconds(1.0f);
        beamLightsRoot.SetActive(false);
        if (backRoutine != null)
        {
            StopCoroutine(backRoutine);
            backRoutine = null;
        }

        for (int i = 0; i < twinkles.Length; ++i)
        {
            twinkles[i].gameObject.SetActive(false);
        }

        var fromColor = new Color(1, 1, 1, 1f);
        var toColor = new Color(1, 1, 1, 0);

        float a = 0f;
        while (a <= 1.0f)
        {
            a += Time.deltaTime * 3.0f;

            texture.color = Color.Lerp(fromColor, toColor, a);
            yield return null;
        }
        a = 1.0f;
        texture.color = Color.Lerp(fromColor, toColor, a);

        Destroy(this.gameObject, 0.3f);
        yield break;
    }

    public void SelfDestroy()
    {
        Destroy(this.gameObject);
    }

    IEnumerator CoBackDeco()
    {
        List<int> twinkleFrame = new List<int>() { 0, 1, 2, 3 };
        while (true)
        {
            var scl = beamLightsRoot.transform.localScale;
            scl.y *= -1;
            beamLightsRoot.transform.localScale = scl;

            for(int i = 0; i < twinkles.Length; ++i)
            {
                twinkles[i].mainTexture = this.twinkleTex[twinkleFrame[i]];
                twinkleFrame[i] = (twinkleFrame[i] + 1) % twinkleTex.Length;
            }
            yield return new WaitForSeconds(0.4f);
        }
    }
}



public interface IMoleReward
{
    void SelfDestroy();
    IEnumerator CoAppear();
    IEnumerator CoDisappear();
}