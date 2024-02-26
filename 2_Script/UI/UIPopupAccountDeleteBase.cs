public class UIPopupAccountDeleteBase : UIPopupBase
{
    protected void OnClickAccountDelete<T>() where T : UIPopupAccountDeleteBase
    {
        _callbackClose = () => ManagerUI._instance.OpenPopup<T>();
        base.OnClickBtnClose();
    }

    protected void OnClickAccountDelete(System.Action callback)
    {
        _callbackClose = () => callback();
        base.OnClickBtnClose();
    }

    protected override void OnClickBtnClose()
    {
        _callbackClose = () => ManagerUI._instance.OpenPopup<UIPopupOption>();
        base.OnClickBtnClose();
    }
}