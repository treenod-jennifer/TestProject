using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class AndroidPermissionManager
{
    private List<string> permissions = new List<string>
    {
        "android.permission.POST_NOTIFICATIONS"
    };
    
    private int  permissionIndex       = 0;
    private bool isPermissionPopupShow = false;

    public IEnumerator CoCheckAndroidPermission()
    {
#if UNITY_ANDROID
        permissionIndex       = 0;
        isPermissionPopupShow = false;
        
        SetPermission();

        yield return new WaitUntil(() => isPermissionPopupShow);
#endif
        yield return null;
    }
    
    private bool IsCheckedAllPermission()
    {
        return permissionIndex > permissions.Count - 1;
    }
    
    private void SetPermission()
    {
        if (IsCheckedAllPermission())
        {
            isPermissionPopupShow = true;
        }
        else
        {
            string permission = permissions[permissionIndex++];

            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                Permission.RequestUserPermission(permission);
                isPermissionPopupShow = true;
            }
            else
            {
                SetPermission();
            }
        }
    }
}
