using UnityEngine;

public class UIPopupAccountDeleteDiaDelete : UIPopupAccountDeleteBase
{
    #region Private
    [SerializeField] private UILabel labelDiaDeleteText;
    #endregion

    private void Start()
    {
        var text = Global._instance.GetString("a_d_5").Replace("[0]", ServerRepos.User.jewel.ToString());
        labelDiaDeleteText.text = text.Replace("[1]", ServerRepos.User.coin.ToString());
    }

    protected void OnClickAccountDelete() => base.OnClickAccountDelete<UIPopupAccountDeleteComplete>();
}