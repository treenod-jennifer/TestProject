using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupUserProfile : UIPopupBase, IImageRequestable
{
    public static UIPopupUserProfile _instance = null;
    private const int pokoValue = 0;

    //포코고로 변경되고 나서 호출되는 콜백.
    public Method.FunctionVoid changeCallBack = null;

    public UIScrollView decoScroll;
    public UIScrollView pokoyuraScroll;

    //유저 프로필.
    public UILabel[]    title;
    public UITexture    profileImage;
    public UITexture    userImage;
    public UITexture    pokogoro;
    public UILabel      userName;
    public UILabel      flower;
    public UILabel      stage;
    public UILabel      day;

    //포코유라 체크.
    public GameObject check;

    //버튼(좌우 각각 2개씩)
    public GameObject[] decoButtons;
    public GameObject[] pokoyuraButtons;
    
    //데코,포코유라 리스트 오브젝트.
    public GameObject _objItemUserProfileDecoList;
    public GameObject _objItemUserProfilePokoYuraList;

    //데코, 포코유라 데이터 리스트.
    private List<int> pokoyuraList = new List<int>();
    
    //데코, 포코유라 화면 인덱스.
    private int decoIndex;
    private int pokoIndex;
    private int decoMaxIndex;
    private int pokoMaxIndex;

    //유저인지 정보 저장 & 대표유라.
    private int selectYura = 0;
    private long userRank = 0;
    private bool bUser = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    new void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        decoScroll.panel.depth = uiPanel.depth + 1;
        pokoyuraScroll.panel.depth = uiPanel.depth + 2;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는다면 안올림.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            decoScroll.panel.useSortingOrder = true;
            pokoyuraScroll.panel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            decoScroll.panel.sortingOrder = layer + 1;
            pokoyuraScroll.panel.sortingOrder = layer + 1;
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void OnLoadComplete(ImageRequestableResult r)
    {
        pokogoro.mainTexture = r.texture;
        pokogoro.MakePixelPerfect();
        pokogoro.gameObject.SetActive(true);
    }

    public void OnLoadFailed() { }

    public int GetWidth()
    {
        return 0;
    }

    public int GetHeight()
    {
        return 0;
    }

    public bool GetUser()
    {
        return bUser;
    }

    public void PokoYuraPosition(Transform tr)
    {
        //포코 유라 변경 후 check 위치 변경하는 콜백.
        changeCallBack += (() =>
        {
            SettingCheck(tr);
        });
    }

    public void SettingCheck(Transform tr)
    {
        if (selectYura != 0)
        {   
            check.transform.parent = tr;
            check.transform.localPosition = new Vector3(20f, 30f, 0f);
            check.SetActive(true);
        }
    }

    public void OnClickPokoYura(int index)
    {
        int selectIndex = 0;
        //현재 포코유라가 선택된 포코유라랑 다르다면 장착(같으면 해제).
        if (selectYura != index)
        {
            selectIndex = index;
        }

    }




    IEnumerator SetProfileImage ( string pictureUrl )
    {
        yield return new WaitForSeconds( Random.Range( 0.5f, 0.7f ) );
        {
            IEnumerator e = PhotoHelp.WWW_LoadImage( pictureUrl );
            while ( e.MoveNext() )
                yield return e.Current;

            if ( e.Current != null )
            {
                Texture _photoTexture = e.Current as Texture;
                this.userImage.mainTexture = ( Texture2D ) _photoTexture;
                this.userImage.material = null;
                this.userImage.color = Color.white;
            }
        }
    }

    private void SettingPokoYura(bool bSelect)
    {
        if (bSelect == true)
        {
            UIImageLoader.Instance.Load(Global.gameImageDirectory, "Pokoyura/", "y_i_" + selectYura, this);
        }
        else
        {
            int rankYura = (int)userRank / 10 + 1;
            string resoucePath = "Class/c" + rankYura;
            Texture texture = Resources.Load(resoucePath) as Texture;

            if (texture != null)
            {
                pokogoro.mainTexture = texture;
                pokogoro.MakePixelPerfect();
            }
        }
    }


    private void MakeDecoList()
    {
      
    }

    private void MakePokoYuraList()
    {
        pokoMaxIndex = pokoyuraList.Count / 10;
        if (pokoyuraList.Count % 10 > 0)
        {
            pokoMaxIndex += 1;
        }
        /*
        int nCount = pokoyuraList.Count;
        for (int i = 0; i < pokoMaxIndex; i++)
        {
            //현재 생성할 데코 아이템 데이터 리스트 생성.
            List<int> pList = new List<int>();
            for (int j = (i * 10); j < (i * 10) + 10; j++)
            {
                //리스트 끝까지 왔으면 종료.
                if (j >= nCount)
                    break;
                pList.Add(pokoyuraList[j]);
            }
            //리스트 오브젝트 생성 & 값 세팅.
            UIItemUserProfilePokoYuraList itemPokoYuraList =
              NGUITools.AddChild(pokoyuraScroll.gameObject, _objItemUserProfilePokoYuraList).GetComponent<UIItemUserProfilePokoYuraList>();
            itemPokoYuraList.transform.localPosition = new Vector3((620f * i), 20f, 0f);
            itemPokoYuraList.SettingPokoYuraList(pList, selectYura);
        }

        //리스트 없을 때 아무 데이터도 안 들어가 있는 리스트 생성.
        if (pokoMaxIndex == 0)
        {
            UIItemUserProfilePokoYuraList itemPokoYuraList =
              NGUITools.AddChild(pokoyuraScroll.gameObject, _objItemUserProfilePokoYuraList).GetComponent<UIItemUserProfilePokoYuraList>();
            itemPokoYuraList.transform.localPosition = new Vector3(0f, 25f, 0f);
        }*/
    }

    #region 좌/우 버튼 클릭.
    private void OnClickBtnDeco_L()
    {
        decoIndex -= 1;
        MoveDecoScroll();
        if (decoIndex <= 1)
        {
            decoButtons[0].SetActive(false);
        }
        if (decoButtons[1].activeInHierarchy == false)
        {
            decoButtons[1].SetActive(true);
        }
    }

    private void OnClickBtnDeco_R()
    {
        decoIndex += 1;
        MoveDecoScroll();
        if (decoIndex >= decoMaxIndex)
        {
            decoButtons[1].SetActive(false);
        }
        if (decoButtons[0].activeInHierarchy == false)
        {
            decoButtons[0].SetActive(true);
        }
    }

    private void MoveDecoScroll()
    {
        Vector3 pos = new Vector3(620f * (decoIndex - 1), 0f, 0f);
        SpringPanel.Begin(decoScroll.gameObject, -pos, 8f);
    }

    private void OnClickBtnPoko_L()
    {
        pokoIndex -= 1;
        MovePokoYuraScroll();
        if (pokoIndex <= 1)
        {
            pokoyuraButtons[0].SetActive(false);
        }
        if (pokoyuraButtons[1].activeInHierarchy == false)
        {
            pokoyuraButtons[1].SetActive(true);
        }
    }

    private void OnClickBtnPoko_R()
    {
        pokoIndex += 1;
        MovePokoYuraScroll();
        if (pokoIndex >= pokoMaxIndex)
        {
            pokoyuraButtons[1].SetActive(false);
        }
        if (pokoyuraButtons[0].activeInHierarchy == false)
        {
            pokoyuraButtons[0].SetActive(true);
        }
    }

    private void MovePokoYuraScroll()
    {
        Vector3 pos = new Vector3(620f * (pokoIndex - 1), 0f, 0f);
        SpringPanel.Begin(pokoyuraScroll.gameObject, -pos, 8f);
    }
    #endregion 좌/우 버튼 클릭.

    private void OnClickBtnInfo()
    {
        ManagerUI._instance.OpenPopupPokoYuraInfo();
    }
}
