using UnityEngine;

public class UIItemAdventureVSPanel : UIItemAdventureTopPanel
{
    [Header("StageLabel")]
    [SerializeField] private UILabel stageLabel;
    [SerializeField] private UILabel stageLabel_Shadow;

    public string StageText
    {
        set
        {
            if (value == null || value == "")
            {
                stageLabel.transform.parent.gameObject.SetActive(false);
                return;
            }
            else
            {
                stageLabel.transform.parent.gameObject.SetActive(true);
            }

            if (stageLabel != null && stageLabel_Shadow != null)
            {
                stageLabel.text = value;
                stageLabel_Shadow.text = value;
            }
        }
    }
}
