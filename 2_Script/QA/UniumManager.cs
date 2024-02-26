using UnityEngine;

namespace treenod.qa
{
    public class UniumManager : MonoBehaviour
    {
#if !UNIUM_DISABLE && ( DEVELOPMENT_BUILD || UNITY_EDITOR || UNIUM_ENABLE )
        private static bool m_Initialized = false;
        private static object m_Lock = new object();
        private static UniumManager m_Instance;

        public static UniumManager Instance
        {
            get
            {
                lock (m_Lock)
                {
                    if (m_Instance == null)
                    {
                        m_Instance = FindObjectOfType(typeof(UniumManager)) as UniumManager;
                        if (m_Instance == null)
                        {
                            Debug.Log("UniumManager Instance Create");
                            var singletonObject = new GameObject("UniumManager");
                            m_Instance = singletonObject.AddComponent<UniumManager>();
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
                    return m_Instance;
                }
            }
        }

        public void Initialize()
        {
            Debug.Log("UniumManager Initialize");
            if (!m_Initialized)
            {
                this.gameObject.AddComponent(typeof(UniumComponent));
                this.gameObject.AddComponent(typeof(QAUtil));

                m_Initialized = true;
            }
        }
#endif
    }
}
