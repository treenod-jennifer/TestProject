using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemMultiLineLabel : MonoBehaviour
{
    [SerializeField] private UILabelPlus DefaultLabel;

    private readonly List<UILabelPlus> labels = new List<UILabelPlus>();

    public int Height
    {
        get
        {
            int height = 0;

            foreach(var label in labels)
            {
                height += label.height;
            }

            return height;
        }
    }

    public void SetText(params string[] text)
    {
        ClearLabel();

        DefaultLabel.gameObject.SetActive(false);

        int height = 0;

        foreach (var t in text)
        {
            var label = MakeLabel(t);
            
            Vector3 pos = label.transform.localPosition;
            pos.y = height;
            label.transform.localPosition = pos;
            height -= label.height;

            labels.Add(label);
        }





        void ClearLabel()
        {
            if (labels.Count > 0)
            {
                foreach (var label in labels)
                {
                    Destroy(label.gameObject);
                }

                labels.Clear();
            }
        }

        UILabelPlus MakeLabel(string textString)
        {
            UILabelPlus label = Instantiate(DefaultLabel, transform);
            label.gameObject.SetActive(true);
            label.text = textString;

            return label;
        }
    }
}
