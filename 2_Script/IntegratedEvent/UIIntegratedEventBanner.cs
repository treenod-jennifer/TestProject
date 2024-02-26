using DG.Tweening;
using UnityEngine;

public class UIIntegratedEventBanner : MonoBehaviour
{
    private readonly Vector2 TWEEN_SCALE = new Vector2(1.3f, 1.3f);
    private readonly float   DURATION    = 0.15f;
    
    [SerializeField] private UIUrlTexture texture;
    [SerializeField] private UILabelPlus  label;
    [SerializeField] private GameObject   endTsObj;
    [SerializeField] private GameObject   newObj;

    private IntegratedEventInfo info;

    public void Init(IntegratedEventInfo info)
    {
        this.info = info;

        texture.SettingTextureScale(texture.width, texture.height);
        texture.LoadCDN(Global.gameImageDirectory, "BannerEvent/", $"{info.icon}.png");

        if (info.endTs < 0)
        {
            endTsObj.SetActive(false);
        }
        else
        {
            var type = info.GetIntegratedEventType();

            if (type != ManagerIntegratedEvent.IntegratedEventType.LOGIN_AD_BONUS_CBU &&
                type != ManagerIntegratedEvent.IntegratedEventType.LOGIN_AD_BONUS_NRU)
            {
                label.text = Global.GetTimeText_MMDDHHMM_Plus1(info.endTs);
            }
            else
            {
                label.text = Global.GetTimeText_MMDDHHMM(info.endTs);
            }
        }

        newObj.SetActive(info.isNew);
    }

    public void OnClickBanner()
    {
        info.OpenEventPopup();
    }

    public void StartNewTween()
    {
        if (info.isNew != true) return;

        var sequence = DOTween.Sequence()
                              .Append(newObj.transform.DOScale(TWEEN_SCALE, DURATION))
                              .Append(newObj.transform.DOScale(Vector2.one, DURATION));
        sequence.Play();
    }
    
    private void OnDestroy()
    {
        texture.mainTexture = null;
    }
}