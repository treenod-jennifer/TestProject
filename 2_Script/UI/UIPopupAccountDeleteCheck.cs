public class UIPopupAccountDeleteCheck : UIPopupAccountDeleteBase
{
    protected void OnClickAccountDelete() => base.OnClickAccountDelete<UIPopupAccountDeleteDiaDelete>();
}