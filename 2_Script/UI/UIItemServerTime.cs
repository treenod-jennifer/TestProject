using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemServerTime : MonoBehaviour
{
    [SerializeField] private UILabel labelTime;

    private void Start()
    {
        transform.localPosition = new Vector3(0, -110f, 0);

        if (IsActiveServerTime())
            StartCoroutine(CoServerTime());
        else
            Destroy(this.gameObject);
    }

    private IEnumerator CoServerTime()
    {
        while(true)
        {
            System.DateTime origin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            origin = origin.AddHours(9);
            origin = origin.AddSeconds(ServerRepos.GetServerTime());
            labelTime.text = origin.ToString("yyyy - MM - dd HH:mm:ss");

            yield return new WaitForSeconds(1f);
        }
    }

    private bool IsActiveServerTime()
    {
        if (Mathf.Abs(ServerRepos.GetServerTime() - ServerRepos.LocalNowTime()) > 300)
            return true;

        return false;
    }

    private void OnEnable()
    {
        if (IsActiveServerTime())
            StartCoroutine(CoServerTime());
        else
            Destroy(this.gameObject);
    }
}
