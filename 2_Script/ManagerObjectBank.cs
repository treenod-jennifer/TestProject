using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ManagerObjectBank : MonoBehaviour
{
    public static ManagerObjectBank _instance = null;
    [System.NonSerialized]
    public bool _complete = false;
    // 생성할 빅 오브젝트 리스트
    public List<GameObject> _bigObject = new List<GameObject>();


    // 각각 생성해서 채워넣어서 필요할때 가져다 활성화 시켜 사용후 다시 반남
    [System.NonSerialized]
    public GameObject _liveBoni = null;
    [System.NonSerialized]
    public GameObject _liveBird = null;
    [System.NonSerialized]
    public GameObject _liveCoco = null;
    [System.NonSerialized]
    public GameObject _clearBoniBird = null;

    void Awake()
    {
        if (_instance == null)
            _instance = this;
    }
    // 타일틀 로딩때 한번 부르기, 프레임 정지를 방지하기 위해 내부 코루틴으로 분산해서 생성. _complete 가 true될때까지 타이틀에서 기다리기
    public void MakeStart()
    {
        if (_complete)
            return;

        StartCoroutine(CoMakeStart());    
    }

    IEnumerator CoMakeStart()
    {
        yield return null;

        {
            // 화면에서 멀리 떨어져서 생성
            // 이 오브젝트 하위에 생성
            // 나중에 필요할때 읽어서 자기에게 연결해서 썼다가 다시 이쪽하단에 붙이기,  오브젝트 풀이랑 비슷하게
            yield return null;
            // 한번 돌리고 비활성화
            yield return null;

        }
        {
            // 화면에서 멀리 떨어져서 생성
            yield return null;
            // 한번 돌리고 비활성화
            yield return null;

        }
        {
            // 화면에서 멀리 떨어져서 생성
            yield return null;
            // 한번 돌리고 비활성화
            yield return null;

        }
        {
            // 화면에서 멀리 떨어져서 생성
            yield return null;
            // 한번 돌리고 비활성화
            yield return null;

        }
        _complete = true;
    }
}
