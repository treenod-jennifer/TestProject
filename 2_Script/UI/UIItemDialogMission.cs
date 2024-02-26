using System.Collections;
using DG.Tweening;
using UnityEngine;

public class UIItemDialogMission : UIDialog
{
    public UIUrlTexture missionIcon;
    public float _fstartTime = 0.2f;
    private int _nWidth = 412;

    public void InitMissionSprite(string missionName, int index)
    {
        _sprDialogBox.width = _nWidth;
        transform.localScale = new Vector3(1.0f, 0.0f, 1.0f);
        _labelChat.text = missionName;
        if (_labelChat.width > 310)
            _sprDialogBox.width = _labelChat.width + 80;
        string fileName = "m_" + index;

        missionIcon.SettingTextureScale(100, 100);
        missionIcon.LoadCDN(Global.gameImageDirectory, "IconMission/", fileName);
        //StartCoroutine(missionIcon.SetTextureScale(100, 100));
        StartCoroutine(CoShow());
    }

    private IEnumerator CoShow()
    {
        yield return new WaitForSeconds(_fstartTime);
        transform.DOScaleY(1f, 0.2f);
    }
}
