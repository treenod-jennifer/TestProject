using UnityEngine;

namespace SideLeftIcon
{
    public static class Maker
    {
        public static T MakeIcon<T>(UILobbyButtonListManager SidebarLeft, System.Action<T> init) where T : PassIcon, new()
        {
            UIPassIcon originPrefab = Resources.Load<UIPassIcon>($"UIPrefab/UIPassIcon");
            UIPassIcon uiIcon = Object.Instantiate(originPrefab, SidebarLeft.transform);

            T icon = new T();

            icon.UIIcon = uiIcon;

            init(icon);

            if (icon.GameObject.transform.parent != SidebarLeft.transform)
            {
                icon.GameObject.transform.SetParent(SidebarLeft.transform, false);
            }

            SidebarLeft.AddLobbyButton(uiIcon.gameObject);

            return icon;
        }
    }

    public class UIPassIcon : MonoBehaviour
    {
        [SerializeField] private UIUrlTexture texture;
        [SerializeField] private UILabelPlus label;
        [SerializeField] public GameObject newIcon;

        public UIUrlTexture Texture
        {
            get { return texture; }
        }

        public UILabelPlus Label
        {
            get { return label; }
        }

        public GameObject NewIcon
        {
            get { return newIcon; }
        }

        public event System.Action OnClickEvent;
        public event System.Action OnEnableEvent;

        public void OnIconClick()
        {
            OnClickEvent?.Invoke();
        }

        private void OnEnable()
        {
            OnEnableEvent?.Invoke();
        }
    }

    public abstract class PassIcon
    {
        private UIPassIcon uiIcon;

        public UIPassIcon UIIcon
        {
            protected get { return uiIcon; }
            set
            {
                uiIcon = value;
                uiIcon.OnClickEvent += () =>
                {
                    UpdateNewIcon();
                    OnIconClick();
                };
                uiIcon.OnEnableEvent += () =>
                {
                    if (PlayerPrefs.HasKey(UniqueKey) == false)
                    {
                        uiIcon.newIcon.SetActive(true);
                        return;
                    }

                    UpdateNewIcon();
                };
            }
        }

        public GameObject GameObject
        {
            get { return UIIcon.gameObject; }
        }

        protected UIUrlTexture Texture
        {
            get { return UIIcon.Texture; }
        }

        protected UILabelPlus Label
        {
            get { return UIIcon.Label; }
        }

        public abstract void OnIconClick();
        public abstract string UniqueKey { get; }
        public abstract void UpdateNewIcon();
    }

    public abstract class PassIcon<T> : PassIcon
    {
        public abstract void Init(T bannerData);

        public void SetNewIcon(bool activeNewIcon)
        {
            UIIcon.NewIcon.SetActive(activeNewIcon);
        }
    }

    public class IconPremiumPass : PassIcon<CdnPremiumPass>
    {
        private CdnPremiumPass eventData;

        public override string UniqueKey => $"IconPremiumPass_{eventData.vsn}_{eventData.resourceIndex}";

        public override void Init(CdnPremiumPass eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_premium_pass_{eventData.resourceIndex}.png");

            EndTsTimer.Run(Label, ManagerPremiumPass.GetPremiumPassData().endTs);
        }

        public override void OnIconClick()
        {
            if (ManagerPremiumPass.CheckStartable())
            {
                ManagerUI._instance.OpenPopup<UIPopUpPremiumPass>((popup) =>
                {
                    popup.InitData(eventData.resourceIndex);
                    popup._callbackClose += () => UpdateNewIcon();
                });
            }
            else
            {
                UIPopupSystem systemPopup =
                    ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"),
                    Global._instance.GetString("n_s_41"), false);
                systemPopup.SortOrderSetting();
            }
        }

        public override void UpdateNewIcon()
        {
            PlayerPrefs.SetInt(UniqueKey, ManagerPremiumPass.IsGetRewardState());
            UIIcon.newIcon.SetActive(PlayerPrefs.GetInt(UniqueKey) != 0);
        }
    }

    public class IconAdventurePass : PassIcon<CdnAdventurePass>
    {
        private CdnAdventurePass eventData;

        public override string UniqueKey => $"IconAdventurePass_{eventData.vsn}_{eventData.resourceIndex}";

        public override void Init(CdnAdventurePass eventData)
        {
            this.eventData = eventData;

            Texture.SettingTextureScale(Texture.width, Texture.height);
            Texture.LoadCDN(Global.gameImageDirectory, "IconEvent/", $"e_adventure_pass_{eventData.resourceIndex}.png");

            EndTsTimer.Run(Label, ManagerAdventurePass.GetAdventurePassData().endTs);
        }

        public override void OnIconClick()
        {
            if (ManagerAdventurePass.CheckStartable())
            {
                ManagerUI._instance.OpenPopup<UIPopUpAdventurePass>((popup) =>
                {
                    popup.InitData();
                    popup._callbackClose += () => UpdateNewIcon();
                });
            }
            else
            {
                UIPopupSystem systemPopup =
                    ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"),
                    Global._instance.GetString("n_s_41"), false);
                systemPopup.SortOrderSetting();
            }
        }

        public override void UpdateNewIcon()
        {
            PlayerPrefs.SetInt(UniqueKey, IsGetRewardState());
            UIIcon.newIcon.SetActive(PlayerPrefs.GetInt(UniqueKey) != 0);
        }

        private static int IsGetRewardState()
        {
            var nRewardState = ServerRepos.UserAdventurePass.rewardState;
            var pRewardState = ServerRepos.UserAdventurePass.premiumRewardState;

            int Count = 0;

            for (int i = 0; i < ManagerAdventurePass.GetMissionProgress(); i++)
            {
                if (nRewardState[i] == 0)
                    Count++;

                if (ServerRepos.UserAdventurePass.premiumState > 1)
                {
                    if (pRewardState[i] == 0)
                        Count++;
                }
            }

            return Count;
        }
    }
}