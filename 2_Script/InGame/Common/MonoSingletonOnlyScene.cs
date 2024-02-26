using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingletonOnlyScene<T> : MonoBehaviour where T : MonoSingletonOnlyScene<T>
{
    private static T m_Instance;

    public static T instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType<T>();
                if (m_Instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    //DontDestroyOnLoad(go);
                    m_Instance = go.AddComponent<T>();
                }
            }
            return m_Instance;
        }
    }

    public static void DestroySelf()
    {
        Destroy(m_Instance.gameObject);
        m_Instance = null;
    }
}

public class MonoSingletonManager<T> : MonoBehaviour where T : MonoSingletonManager<T>
{
    private static T m_Instance;

    public static T instance
    {
        get
        {
            if ( m_Instance == null )
            {
                m_Instance = FindObjectOfType<T>();
                if ( m_Instance == null )
                {
                    GameObject go = new GameObject( typeof( T ).Name );
                    DontDestroyOnLoad(go);
                    m_Instance = go.AddComponent<T>();
                }
            }
            return m_Instance;
        }
    }

    public static void DestroySelf ()
    {
        Destroy( m_Instance.gameObject );
        m_Instance = null;
    }
}
