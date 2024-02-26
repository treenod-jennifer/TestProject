using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notification : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnClickRegisterNotification()
	{
//		LocalNotification.RegisterNotification(LOCAL_NOTIFICATION_TYPE.GIFT_BOX, 1, 30, "Test Gift_Box", "ic_notify");
	}

	public void OnClickCancelNotification()
	{
//		LocalNotification.CancelNotification(LOCAL_NOTIFICATION_TYPE.GIFT_BOX, 1);
	}

	public void OnClickRegisterNotificationWithIcon()
	{
//		LocalNotification.RegisterNotificationWithIcon(LOCAL_NOTIFICATION_TYPE.TIME_MISSION, 15, 15, "test Time Mission", "ic_notify", "app_icon");
	}

	public void OnClickCancelNotificationWIthIcon()
	{
//		LocalNotification.CancelNotificationWithIcon(LOCAL_NOTIFICATION_TYPE.TIME_MISSION, 15); 
	}
}
