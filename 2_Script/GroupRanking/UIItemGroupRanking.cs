using UnityEngine;

public class UIItemGroupRanking : MonoBehaviour
{
    private readonly Rect _uvAnimalRect = new Rect(0.03f, 0.04f, 0.93f, 0.93f);

    [SerializeField] private int          _sizeProfile;
    [SerializeField] private UIUrlTexture _userAlterPicture;
    [SerializeField] private UILabel[]    _labelRank;     //현재 순위
    [SerializeField] private UILabel[]    _labelUserName; // 유저명
    [SerializeField] private UILabel      _labelScore;    //점수
    [SerializeField] private UISprite     _spriteIcon;
    [SerializeField] private GameObject   _checkObj;

    private ManagerGroupRanking.GroupRankingUserData _item;

    private void SetRank(string        rank)         => _labelRank.SetText(rank);
    private void SetScore(string       score)        => _labelScore.text = score;
    private bool IsAlterPicture(string alterPicture) => !string.IsNullOrEmpty(alterPicture) && !alterPicture.Equals("0");

    private bool UpdateData_Base(ManagerGroupRanking.GroupRankingUserData cellData)
    {
        if (cellData == null)
        {
            gameObject.SetActive(false);
            return false;
        }

        _item = cellData;
        if (gameObject.activeInHierarchy == false)
        {
            return false;
        }

        SetScore(_item.score.ToString());
        _labelUserName.SetText($"{Global.ClipString(_item.name, 12)}");
        SetProfile(_item.alterPicture);

        if (_item.score <= 0)
        {
            SetRank("-");
            SetRankingBoxIcon(0);
        }
        else
        {
            SetRank(_item.rank.ToString());
            SetRankingBoxIcon((int)_item.rank);
            SetCheck();
        }

        return true;
    }

    public void UpdateData(ManagerGroupRanking.GroupRankingUserData cellData)
    {
        if (UpdateData_Base(cellData))
        {
        }
    }

    private void SetRankingBoxIcon(int rank)
    {
        if (_spriteIcon == null)
        {
            return;
        }

        _spriteIcon.gameObject.SetActive(true);
        switch (rank)
        {
            case 0:
                _spriteIcon.gameObject.SetActive(false);
                break;
            case 1:
                _spriteIcon.spriteName = "box1";
                break;
            case 2:
                _spriteIcon.spriteName = "box2";
                break;
            case 3:
                _spriteIcon.spriteName = "box3";
                break;
            default:
                _spriteIcon.spriteName = "box_etc";
                break;
        }
    }

    private void SetCheck()
    {
        if (_checkObj == null)
        {
            return;
        }

        _checkObj.SetActive(_item.isGetRankReward);
    }

    private void SetProfile(string alterPicture)
    {
        var isPhotoActive = IsAlterPicture(alterPicture);

        if (isPhotoActive)
        {
            _userAlterPicture.SettingTextureScale(_sizeProfile, _sizeProfile);
            _userAlterPicture.LoadCDN(Global.adventureDirectory, "Animal/", $"ap_{alterPicture}.png");

            _userAlterPicture.uvRect = _uvAnimalRect;
        }
        else
        {
            SetProfileImage("UI/profile_2");
        }
    }

    private void SetProfileImage(string resourcePath)
    {
        _userAlterPicture.SettingTextureScale(_sizeProfile, _sizeProfile);
        _userAlterPicture.LoadResource(resourcePath);
    }
}