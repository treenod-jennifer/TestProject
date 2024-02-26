using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PokoWeb : MonoBehaviour
{
    public enum Method {
        GET,
        POST,
        PUT
    }

    string uri;
    WWWForm form = null;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public static IEnumerator Execute(Method m, string uri, Dictionary<string, string> uriParam = null, Dictionary<string, string> headers = null)
    //{
    //    UnityWebRequest r = null;

    //    WWWForm form;
    //    form.
    //    switch ( m)
    //    {
    //        case Method.GET: r = UnityWebRequest.Get(uri); break;
    //        case Method.POST: r = UnityWebRequest.Post(uri, ); break;
    //        case Method.GET: r = UnityWebRequest.Put(uri, ); break;
    //    }
        
        


    //}


}

public static class UnityWebRequestExt
{
    public static bool IsError (this UnityWebRequest u)
    {
        if(u.isHttpError || u.isNetworkError )
        {
            return true;
        }
        return false;
    }
}