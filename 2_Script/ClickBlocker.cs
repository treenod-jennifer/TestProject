using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickBlocker : MonoBehaviour
{
    public UISprite _spriteBlind;
    public UIPanel _mainPanel;
    float alpha = 0f;

    private static ClickBlocker _tempInstance = null;
    private static ClickBlocker clickBlocker_obj
    {
        get
        {
            if(_tempInstance == null)
                _tempInstance = Instantiate(Global._instance._objClickBlocker).gameObject.GetComponent<ClickBlocker>();

            return _tempInstance;
        }
    }

    void Awake()
    {
        _tempInstance = this;
        SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        DontDestroyOnLoad(gameObject);
    }
    
    private void OnDestroy()
    {
        _tempInstance = null;
        SceneManager.sceneUnloaded -= SceneManager_sceneUnloaded;
    }

    private void SceneManager_sceneUnloaded(Scene unloadScene)
    {
        string activeSceneName = SceneManager.GetActiveScene().name;

        if (clickBlocker_obj.gameObject.activeSelf)
        {
            clickBlocker_obj.StopAllCoroutines();
            clickBlocker_obj.gameObject.SetActive(false);
            if (ManagerUI._instance != null)
                ManagerUI._instance.activeClickBlocker = false;
        }
    }

    static public void Make(float disabledTime = 0.5f)
    {
        string activeSceneName = SceneManager.GetActiveScene().name;

        if (activeSceneName == "Lobby" || activeSceneName == "InGame")
        {
            if (clickBlocker_obj.gameObject.activeSelf)
                clickBlocker_obj.StopAllCoroutines();

            clickBlocker_obj.gameObject.SetActive(true);
            if (ManagerUI._instance != null)
                ManagerUI._instance.activeClickBlocker = true;
            clickBlocker_obj.StartCoroutine(clickBlocker_obj.End(disabledTime));
        }
    }

    private IEnumerator End(float disabledTime)
    {
        yield return new WaitForSeconds(disabledTime);
        gameObject.SetActive(false);
        if (ManagerUI._instance != null)
            ManagerUI._instance.activeClickBlocker = false;
    }

    static public bool Exist()
    {
        return _tempInstance;
    }

    // Use this for initialization
    IEnumerator Start()
    {        
        _mainPanel.alpha = 1f;
        yield return null;
    }
}
