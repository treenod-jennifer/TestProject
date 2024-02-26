using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonAdventureSpeedUp : MonoBehaviour
{
    [SerializeField] private GameObject onRoot;
    [SerializeField] private GameObject offRoot;
    [SerializeField] private UILabel speedLabel;
    [SerializeField] private GameObject helpBox;
    [SerializeField][Range(1.5f, 5.0f)] private float timeSpeed = 1.5f;

#if UNITY_EDITOR
    [SerializeField] private bool isDebugMode = false;
#endif

    private bool Lock
    {
        set
        {
            onRoot.SetActive(!value);
            offRoot.SetActive(value);

#if UNITY_EDITOR
            if (isDebugMode)
            {
                onRoot.SetActive(true);
                offRoot.SetActive(false);
            }
#endif
        }
        get
        {
            return offRoot.activeSelf;
        }
    }
    private int Speed
    {
        set
        {
            if (!Lock)
            {
                speedLabel.text = "x" + value.ToString();

                switch (value)
                {
                    case 1:
                        PlayerPrefs.SetInt(SAVE_KEY, value);
                        GameSpeed.SetTimeScale(1.0f);
                        break;
                    case 2:
                        PlayerPrefs.SetInt(SAVE_KEY, value);
                        GameSpeed.SetTimeScale(timeSpeed);
                        break;
                }
            }
        }
        get
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                PlayerPrefs.SetInt(SAVE_KEY, 1);
            }

            return PlayerPrefs.GetInt(SAVE_KEY);
        }
    }

    private const string SAVE_KEY = "ADVENTURE_SPEED";

    private void Awake()
    {
        if (ManagerUI._instance != null)
        {
            ManagerUI._instance.FirstOpenCallback += Pause;
            ManagerUI._instance.AllCloseCallback += Play;
        }

        GameItemManager.OpenEvent += Pause;
        GameItemManager.CloseEvent += Play;
    }

    private void OnDestroy()
    {
        GameSpeed.ResetTimeScale();
       
        if( ManagerUI._instance != null)
        {
            ManagerUI._instance.FirstOpenCallback -= Pause;
            ManagerUI._instance.AllCloseCallback -= Play;
        }

        GameItemManager.OpenEvent -= Pause;
        GameItemManager.CloseEvent -= Play;
    }

    public void InitButton(bool isLock)
    {
#if UNITY_EDITOR
        if (isDebugMode) { isLock = false; };
#endif

        if (!isLock)
        {
            SetSpeed();
        }

        Lock = isLock;
    }

    public void OnClickSpeedUp()
    {
        if (Lock)
        {
            OnClickLock();
        }
        else if (Speed == 1)
        {
            Speed = 2;
        }
        else if(Speed == 2)
        {
            Speed = 1;
        }
    }

    public void OnClickLock()
    {
        if (!helpBox.activeSelf)
            StartCoroutine("OnLockAni");
        else
        {
            StopCoroutine("OnLockAni");
            helpBox.SetActive(false);
        }
    }

    private IEnumerator OnLockAni()
    {
        helpBox.SetActive(true);

        yield return new WaitForSeconds(2.0f);

        helpBox.SetActive(false);
    }

    private void SetSpeed()
    {
        Speed = Speed;
    }

    private void Pause()
    {
        GameSpeed.ResetTimeScale();
    }

    private void Play()
    {
        SetSpeed();
    }
}

public static class GameSpeed
{
    public static void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Global.timeScalePuzzle = scale;
    }

    public static void ResetTimeScale()
    {
        SetTimeScale(1.0f);
    }
}