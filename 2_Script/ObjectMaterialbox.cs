using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMaterialbox : ObjectIcon
{
    [SerializeField] private UIItemADButton adButton;

    [SerializeField] private GameObject adObject;
    [SerializeField] private GameObject timeObject;

    static public List<ObjectMaterialbox> _materialboxList = new List<ObjectMaterialbox>();

    private void Awake()
    {
        base.Awake();

        _materialboxList.Add(this);
    }

    private void Start()
    {
        StartCoroutine(CoIconChangeAction());
    }

    private void OnDestroy()
    {
        _materialboxList.Remove(this);
    }

    public override void OnTap()
    {
        if (adButton != null)
            adButton.OpenADView();
    }

    IEnumerator CoIconChangeAction()
    {
        bool changeValue = false;

        while (this.gameObject.activeInHierarchy)
        {

            adObject.SetActive(changeValue);
            timeObject.SetActive(!changeValue);

            changeValue = !changeValue;

            yield return new WaitForSeconds(1f);
        }
    }
}
