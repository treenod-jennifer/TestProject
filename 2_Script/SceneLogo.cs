using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLogo : MonoBehaviour
{

    // 여기 텍스처는 리소스 폴더에서 읽어서 출력,, 씬 벗어날때 읽은 텍스처 꼭 해제
    // 추가로 이벤트에 따라 텍스처를 로딩해서 타이틀씬 바뀌게도..
    // 
    public UITexture _textureLogo;

    void Awake()
    {
        
    }

	// Use this for initialization
	IEnumerator Start () {

        yield return new WaitForSeconds(0.8f);

        //그로씨 플로우 로그
        //ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEvent(ServiceSDK.GROWTHY_INFLOW_VALUE.SPLASH);

        CameraEffect staticCamera = CameraEffect.MakeScreenEffect();
        staticCamera.ApplyScreenEffect(new Color(0f, 0f, 0f, 0f), Color.black, 0.2f, false);
  
        yield return new WaitForSeconds(0.3f);
        Application.LoadLevel("Title");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
