using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PokoAddressable;

public static class RewardHelper
{
    private class RewardInfo
    {
        /// <summary>
        /// 아이템 이름의 키값 입니다.
        /// </summary>
        private readonly string nameKey;
        /// <summary>
        /// 아이템 아이콘의 스프라이트 이름 입니다.
        /// </summary>
        private readonly string spriteName;
        /// <summary>
        /// Resource 폴더 내부에 있는 아이템 아이콘의 경로 입니다.
        /// </summary>
        private readonly string resourcePath;

        /// <summary>
        /// CDN에서 다운 받은 아이템 아이콘의 로컬 저장 경로 입니다.
        /// </summary>
        public readonly string localPath;
        /// <summary>
        /// 아이템 아이콘의 CDN 경로 입니다.
        /// </summary>
        public readonly string cdnPath;
        /// <summary>
        /// CDN에 있는 아이템 아이콘의 이름 입니다.
        /// </summary>
        private readonly string fileName;

        private readonly bool isTimeLimitItem;
        private readonly bool isActiveCount;

        public bool HasSpriteName { get { return !string.IsNullOrEmpty(spriteName); } }
        public bool HasResourcePath { get { return !string.IsNullOrEmpty(resourcePath); } }
        public bool HasCDNPath { get { return !string.IsNullOrEmpty(localPath) && !string.IsNullOrEmpty(cdnPath) && !string.IsNullOrEmpty(fileName); } }
        public bool IsTimeLimitItem { get { return isTimeLimitItem; } }
        public bool IsActiveCount { get { return isActiveCount; } }

        public delegate void ImageAction(RewardInfo reward, int value, UISprite spr, UIUrlTexture tex, float scl, bool detailMode, bool autoResizing = true);
        public delegate void GradeAction(GameObject[] grade, int value, bool detailMode);
        public delegate void LabelAction(int value, UILabel[] lblCount, bool lblFormatIncludeX, bool detailMode);

        /// <summary>
        /// 아이템 아이콘을 처리하는 방식
        /// </summary>
        public ImageAction imageAction;
        /// <summary>
        /// 아이템의 단계를 처리하는 방식
        /// </summary>
        public GradeAction gradeAction;
        /// <summary>
        /// 아이템의 갯수 및 시간 등을 처리하는 방식
        /// </summary>
        public LabelAction labelAction;

        /// <summary>
        /// 아이템의 정보 입니다.
        /// </summary>
        /// <param name="nameKey">아이템 이름의 키값 입니다.</param>
        /// <param name="spriteName">아이템 아이콘의 스프라이트 이름 입니다.</param>
        /// <param name="resourcePath">Resource 폴더 내부에 있는 아이템 아이콘의 경로 입니다.</param>
        /// <param name="localPath">CDN에서 다운 받은 아이템 아이콘의 로컬 저장 경로 입니다.</param>
        /// <param name="cdnPath">아이템 아이콘의 CDN 경로 입니다.</param>
        /// <param name="fileName">CDN에 있는 아이템 아이콘의 이름 입니다.</param>
        /// <param name="imageAction">아이템 아이콘을 처리하는 방식</param>
        /// <param name="gradeAction">아이템의 단계를 처리하는 방식</param>
        /// <param name="labelAction">아이템의 갯수 및 시간 등을 처리하는 방식</param>
        public RewardInfo(string nameKey = null,
                          string spriteName = null,
                          string resourcePath = null,
                          string localPath = null,
                          string cdnPath = null,
                          string fileName = null,
                          bool isTimeLimit = false,
                          bool isActiveCount = true,
                          ImageAction imageAction = null,
                          GradeAction gradeAction = null,
                          LabelAction labelAction = null)
        {
            this.nameKey = nameKey;
            this.spriteName = spriteName;
            this.resourcePath = resourcePath;
            this.localPath = localPath;
            this.cdnPath = cdnPath;
            this.fileName = fileName;
            this.isTimeLimitItem = isTimeLimit;
            this.isActiveCount = isActiveCount;

            this.imageAction = imageAction ?? ImageTypeDefault;
            this.gradeAction = gradeAction ?? GradeTypeDefault;
            this.labelAction = labelAction ?? LabelTypeCount;
        }

        public string GetNameKey(params object[] value)
        {
            if (string.IsNullOrEmpty(nameKey)) 
            {
                return string.Empty;
            }
            else
            {
                return string.Format(nameKey, value);
            }
        }

        public string GetSpriteName(params object[] value)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                return string.Empty;
            }
            else
            {
                return string.Format(spriteName, value);
            }
        }

        public string GetResourcePath(params object[] value)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return string.Empty;
            }
            else
            {
                return string.Format(resourcePath, value);
            }
        }

        public string GetFileName(params object[] value)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }
            else
            {
                return string.Format(fileName, value);
            }
        }
    }

    private static Dictionary<RewardType, RewardInfo> rewardInfo = new Dictionary<RewardType, RewardInfo>()
    {
        {
            RewardType.clover,              new RewardInfo(nameKey:      "item_1",
                                                           spriteName:   "icon_clover_stroke_green",
                                                           resourcePath: "local_message/clover")
        },
        {
            RewardType.coin,                new RewardInfo(nameKey:      "item_4",
                                                           spriteName:   "icon_coin_stroke_yellow",
                                                           resourcePath: "local_message/coin")
        },
        {
            RewardType.jewel,               new RewardInfo(nameKey:      "item_2",
                                                           spriteName:   "icon_diamond_stroke_blue",
                                                           resourcePath: "local_message/jewel")
        },
        {
            RewardType.star,                new RewardInfo(nameKey:      "item_3",
                                                           spriteName:   "icon_star_stroke_pink",
                                                           resourcePath: "local_message/star")
        },
        {
            RewardType.flower,              new RewardInfo(nameKey:      "item_10",
                                                           spriteName:   "stage_icon_level_03",
                                                           resourcePath: "local_message/flower")
        },
        {
            RewardType.readyItem1,          new RewardInfo(nameKey:      "item_i_1",
                                                           spriteName:   "icon_apple_stroke",
                                                           resourcePath: "local_message/readyItem1")
        },
        {
            RewardType.readyItem2,          new RewardInfo(nameKey:      "item_i_2",
                                                           spriteName:   "icon_scoreUp_stroke",
                                                           resourcePath: "local_message/readyItem2")
        },
        {
            RewardType.readyItem3,          new RewardInfo(nameKey:      "item_i_3",
                                                           spriteName:   "icon_random_bomb_stroke",
                                                           resourcePath: "local_message/readyItem3")
        },
        {
            RewardType.readyItem4,          new RewardInfo(nameKey:      "item_i_4",
                                                           spriteName:   "icon_line_bomb_stroke",
                                                           resourcePath: "local_message/readyItem4")
        },
        {
            RewardType.readyItem5,          new RewardInfo(nameKey:      "item_i_5",
                                                           spriteName:   "icon_bomb_stroke",
                                                           resourcePath: "local_message/readyItem5")
        },
        {
            RewardType.readyItem6,          new RewardInfo(nameKey:      "item_i_6",
                                                           spriteName:   "icon_rainbow_stroke",
                                                           resourcePath: "local_message/readyItem6")
        },
        {
            RewardType.readyItem7,          new RewardInfo(nameKey:      "item_i_7")
        },
        {
            RewardType.readyItem8,          new RewardInfo(nameKey:      "item_i_8")
        },
        {
            RewardType.readyItem3_Time,     new RewardInfo(nameKey:         "item_i_15",
                                                           spriteName:      "icon_time_random_bomb_stroke",
                                                           resourcePath:    "local_message/readyItem3_time",
                                                           isTimeLimit:     true,
                                                           isActiveCount:   false,
                                                           labelAction:     LabelTypeTime)
        },
        {
            RewardType.readyItem4_Time,     new RewardInfo(nameKey:         "item_i_11",
                                                           spriteName:      "icon_time_line_bomb_stroke",
                                                           resourcePath:    "local_message/readyItem4_time",
                                                           isTimeLimit:     true,
                                                           isActiveCount:   false,
                                                           labelAction:     LabelTypeTime)
        },
        {
            RewardType.readyItem5_Time,     new RewardInfo(nameKey:         "item_i_12",
                                                           spriteName:      "icon_time_bomb_stroke",
                                                           resourcePath:    "local_message/readyItem5_time",
                                                           isTimeLimit:     true,
                                                           isActiveCount:   false,
                                                           labelAction:     LabelTypeTime)
        },
        {
            RewardType.readyItem6_Time,     new RewardInfo(nameKey:         "item_i_13",
                                                           spriteName:      "icon_time_rainbow_stroke",
                                                           resourcePath:    "local_message/readyItem6_time",
                                                           isTimeLimit:     true,
                                                           isActiveCount:   false,
                                                           labelAction:     LabelTypeTime)
        },
        {
            RewardType.readyItemBomb_Time,  new RewardInfo(nameKey:         "item_i_14",
                                                           spriteName:      "icon_time_allBomb_stroke",
                                                           resourcePath:    "local_message/readyItemBomb_Time",
                                                           isTimeLimit:     true,
                                                           isActiveCount:   false,
                                                           labelAction:     LabelTypeTime)
        },
        {
            RewardType.ingameItem1,         new RewardInfo(nameKey:      "item_i_7",
                                                           spriteName:   "icon_hammer_stroke",
                                                           resourcePath: "local_message/ingameItem1")
        },
        {
            RewardType.ingameItem2,         new RewardInfo(nameKey:      "item_i_8",
                                                           spriteName:   "icon_line_hammer_stroke",
                                                           resourcePath: "local_message/ingameItem2")
        },
        {
            RewardType.ingameItem3,         new RewardInfo(nameKey:      "item_i_9",
                                                           spriteName:   "icon_power_hammer_stroke",
                                                           resourcePath: "local_message/ingameItem3")
        },
        {
            RewardType.ingameItem4,         new RewardInfo(nameKey:      "item_i_10",
                                                           spriteName:   "icon_rainbow_bomb_hammer_stroke",
                                                           resourcePath: "local_message/ingameItem4")
        },
        {
            RewardType.ingameItem5,         new RewardInfo(nameKey:      "item_i_16",
                                                           spriteName:   "icon_color_brush",
                                                           resourcePath: "local_message/ingameItem5")
        },
        {
            RewardType.ingameContinue,      null
        },
        {
            RewardType.toy,                 new RewardInfo(nameKey:         "item_8",
                                                           localPath:       Global.gameImageDirectory,
                                                           cdnPath:         "Pokoyura",
                                                           fileName:        "y_{0}.png",
                                                           isActiveCount:   false,
                                                           labelAction:     LabelTypeDisable)
        },
        {
            RewardType.stamp,               new RewardInfo(nameKey:         "item_5",
                                                           resourcePath:    "local_message/stamps",
                                                           isActiveCount:   false,
                                                           labelAction:     LabelTypeDisable)
        },
        {
            RewardType.housing,             new RewardInfo(nameKey:         "item_24",
                                                           localPath:       Global.gameImageDirectory,
                                                           cdnPath:         "IconHousing",
                                                           fileName:        "{0}_{1}.png",
                                                           isActiveCount:   false,
                                                           imageAction:     ImageTypeHousing,
                                                           labelAction:     LabelTypeDisable)
        },
        {
            RewardType.cloverFreeTime,      new RewardInfo(nameKey:         "item_7",
                                                           spriteName:      "icon_cloverTime_stroke_green",
                                                           resourcePath:    "local_message/cloverTime",
                                                           isTimeLimit:     true,
                                                           isActiveCount:   false,
                                                           labelAction:     LabelTypeTime)
        },
        {
            RewardType.costume,             new RewardInfo(nameKey:         "item_9",
                                                           resourcePath:    "local_message/costume_stroke",
                                                           localPath:       Global.gameImageDirectory,
                                                           cdnPath:         "Costume",
                                                           fileName:        "{0}_{1}.png",
                                                           isActiveCount:   false,
                                                           imageAction:     ImageTypeCoutume,
                                                           labelAction:     LabelTypeDisable) 
        },                                  
        {                                   
            RewardType.rankPoint,           null
        },                                  
        {                                   
            RewardType.wing,                new RewardInfo(nameKey:         "item_11",
                                                           spriteName:      "adven_wing_icon",
                                                           resourcePath:    "local_message/wing") 
        },                                  
        {                                   
            RewardType.expBall,             new RewardInfo(nameKey:         "item_12",
                                                           spriteName:      "adven_exp_icon",
                                                           resourcePath:    "local_message/adven_exp_icon") 
        },                                  
        {                                   
            RewardType.gachaTicket,         new RewardInfo(nameKey:         "item_13",
                                                           localPath:       Global.adventureDirectory,
                                                           cdnPath:         "IconEvent",
                                                           fileName:        "gacha_t_{0:D4}.png",
                                                           isActiveCount:   false,
                                                           labelAction:     LabelTypeDisable) 
        },                                  
        {                                   
            RewardType.animal,              new RewardInfo(nameKey:         "item_25",
                                                           localPath:       Global.adventureDirectory,
                                                           cdnPath:         "Animal",
                                                           fileName:        "NoName",
                                                           isActiveCount:   false,
                                                           imageAction:     ImageTypeAnimal,
                                                           gradeAction:     GradeTypeAnimal,
                                                           labelAction:     LabelTypeDisable) 
        },                                  
        {                                   
            RewardType.animalOverlapTicket, new RewardInfo(nameKey:         "item_ot_{0}",
                                                           spriteName:      "item_overlap_{0}",
                                                           isActiveCount:   false,
                                                           labelAction:     LabelTypeDisable) 
        },                                  
        {                                   
            RewardType.wingExtend,          new RewardInfo(spriteName:   "adven_wing_icon",
                                                           resourcePath: "local_message/wing",
                                                           isActiveCount:   false) 
        },                                  
        {                                   
            RewardType.BonusTag,            null
        },                                  
        {                                   
            RewardType.FreeCoin,            null
        },                                  
        {                                   
            RewardType.wingFreetime,        new RewardInfo(nameKey:         "item_20",
                                                           spriteName:      "adven_wing_time_icon",
                                                           resourcePath:    "local_message/wingTime",
                                                           isTimeLimit:     true,
                                                           isActiveCount:   false,
                                                           labelAction:     LabelTypeTime) 
        },                                  
        {                                   
            RewardType.rankToken,           new RewardInfo(nameKey:      "item_23",
                                                           spriteName:   "worldrank_rankToken",
                                                           resourcePath: "local_message/rankToken") 
        },
        {
            RewardType.capsuleGachaToken,     new RewardInfo(nameKey:    "p_ct_4",
                                                           spriteName:   "icon_capsuleToy_Token",
                                                           resourcePath: "local_message/capsuleToy_Token")
        },
        {
            RewardType.endContentsToken,     new RewardInfo(nameKey:    "ec_col_101",
                                                            spriteName:   "icon_endContents_Token",
                                                            resourcePath: "local_message/endContents_Token")
        },
        {                                   
            RewardType.revivalAndHeal,      new RewardInfo(nameKey:      "item_ad_1",
                                                           spriteName:   "item_Resurrection2",
                                                           resourcePath: "local_message/item_Resurrection") 
        },                                  
        {                                   
            RewardType.skillCharge,         new RewardInfo(nameKey:      "item_ad_2",
                                                           spriteName:   "item_Skill_charge",
                                                           resourcePath: "local_message/item_Skill_charge") 
        },                                  
        {                                   
            RewardType.rainbowHammer,       new RewardInfo(nameKey:      "item_ad_3",
                                                           spriteName:   "item_Rainbow_Hammer",
                                                           resourcePath: "local_message/item_Rainbow_Hammer") 
        },                                  
        {                                   
            RewardType.boxSmall,            new RewardInfo(nameKey:      "item_g_1",
                                                           spriteName:   "icon_giftbox_blueStroke_01",
                                                           resourcePath: "local_message/giftbox1") 
        },                                  
        {                                   
            RewardType.boxMiddle,           new RewardInfo(nameKey:      "item_g_2",
                                                           spriteName:   "icon_giftbox_blueStroke_02",
                                                           resourcePath: "local_message/giftbox2") 
        },                                  
        {                                   
            RewardType.boxBig,              new RewardInfo(nameKey:      "item_g_3",
                                                           spriteName:   "icon_giftbox_blueStroke_03",
                                                           resourcePath: "local_message/giftbox3") 
        },                                  
        {                                   
            RewardType.material,            new RewardInfo(nameKey:      "item_6",
                                                           localPath:    Global.gameImageDirectory,
                                                           cdnPath:      "IconMaterial",
                                                           fileName:     "mt_{0}.png",
                                                           imageAction:  ImageTypeMaterial) 
        },
    };

    private static void SetSprite(UISprite spr, UIUrlTexture tex, float scl, string spriteName, bool autoResizing)
    {
        if(spr != null)
        {
            spr.enabled = true;
            spr.spriteName = spriteName;

            if (autoResizing)
            {
                SettingSpriteSize(spr, scl);
            }
        }

        if(tex != null)
        {
            tex.enabled = false;
        }
    }

    private static void SetTextureResource(UISprite spr, UIUrlTexture tex, float scl, string resourcePath, bool autoResizing)
    {
        if(spr != null)
        {
            spr.enabled = false;
        }

        if(tex != null)
        {
            tex.gameObject.AddressableAssetLoad<Texture2D>(resourcePath, (texture) =>
            {
                tex.enabled = true;
                tex.mainTexture = texture;
                if (autoResizing)
                {
                    SettingTextureSize(tex, scl);
                }
            });

        }
    }

    private static void SetTextureCDN(UISprite spr, UIUrlTexture tex, float scl, string localPath, string cdnPath, string fileName, bool autoResizing)
    {
        if(spr != null)
        {
            spr.enabled = false;
        }

        if(tex != null)
        {
            tex.enabled = true;

            if (autoResizing)
            {
                SettingUrlTextureSize(tex, scl);
            }

            tex.LoadCDN(localPath, cdnPath, fileName);
        }
    }

    private static void ImageTypeDefault(RewardInfo reward, int value, UISprite spr, UIUrlTexture tex, float scl, bool detailMode, bool autoResizing = true)
    {
        if (spr != null && reward.HasSpriteName)
        {
            string spriteName= reward.GetSpriteName(value);
            SetSprite(spr, tex, scl, spriteName, autoResizing);
        }
        else if (tex != null && reward.HasResourcePath)
        {
            string resourcePath = reward.GetResourcePath(value);
            SetTextureResource(spr, tex, scl, resourcePath, autoResizing);
        }
        else if (tex != null && reward.HasCDNPath)
        {
            string fileName = reward.GetFileName(value);
            SetTextureCDN(spr, tex, scl, reward.localPath, reward.cdnPath, fileName, autoResizing);
        }
    }

    private static void ImageTypeAnimal(RewardInfo reward, int value, UISprite spr, UIUrlTexture tex, float scl, bool detailMode, bool autoResizing = true)
    {
        if (reward.HasCDNPath)
        {
            string fileName;

            if (detailMode)
            {
                fileName = ManagerAdventure.GetAnimalTextureFilename(value, 0);
                scl *= 1.925f;
            }
            else
            {
                fileName = ManagerAdventure.GetAnimalProfileFilename(value, 0);
                scl *= 0.9f;
            }

            SetTextureCDN(spr, tex, scl, reward.localPath, reward.cdnPath, fileName, autoResizing);
        }
    }

    private static void ImageTypeHousing(RewardInfo reward, int value, UISprite spr, UIUrlTexture tex, float scl, bool detailMode, bool autoResizing = true)
    {
        if (reward.HasCDNPath)
        {
            if (detailMode)
            {
                scl *= 1.375f;
            }

            int housingIdx = (int)(value / 10000);
            int modelIdx = (int)(value % 10000);
            string fileName = reward.GetFileName(housingIdx, modelIdx);

            SetTextureCDN(spr, tex, scl, reward.localPath, reward.cdnPath, fileName, autoResizing);
        }
    }

    private static void ImageTypeCoutume(RewardInfo reward, int value, UISprite spr, UIUrlTexture tex, float scl, bool detailMode, bool autoResizing = true)
    {
        if (detailMode)
        {
            if (reward.HasCDNPath)
            {
                string fileName = reward.GetFileName(0, value);
                scl *= 0.67f;

                SetTextureCDN(spr, tex, scl, reward.localPath, reward.cdnPath, fileName, autoResizing);
            }
        }
        else
        {
            if (reward.HasResourcePath)
            {
                SetTextureResource(spr, tex, scl, reward.GetResourcePath(), autoResizing);
            }
        }
    }

    private static void ImageTypeMaterial(RewardInfo reward, int value, UISprite spr, UIUrlTexture tex, float scl, bool detailMode, bool autoResizing = true)
    {
        if (reward.HasCDNPath)
        {
            int matNum = value % (int)RewardType.material;
            string fileName = reward.GetFileName(matNum);

            SetTextureCDN(spr, tex, scl, reward.localPath, reward.cdnPath, fileName, autoResizing);
        }
    }


    private static void GradeTypeDefault(GameObject[] grade, int value, bool detailMode)
    {
        if (grade != null)
        {
            for (int i = 0; i < grade.Length; i++)
            {
                grade[i].SetActive(false);
            }
        }
    }

    private static void GradeTypeAnimal(GameObject[] grade, int value, bool detailMode)
    {
        if (grade != null)
        {
            int gradeValue = detailMode ? value / 1000 : 0;

            for (int i = 0; i < grade.Length; i++)
            {
                grade[i].SetActive(i < gradeValue);
            }
        }
    }


    private static void LabelTypeCount(int value, UILabel[] lblCount, bool lblFormatIncludeX, bool detailMode)
    {
        if (lblCount != null && lblCount.Length != 0)
        {
            lblCount[0].enabled = true;
            lblCount[0].text = string.Format(lblFormatIncludeX ? "x{0}" : "{0}", value);
            if (lblCount.Length > 1)
            {
                lblCount[1].enabled = true;
                lblCount[1].text = string.Format(lblFormatIncludeX ? "x{0}" : "{0}", value);
            }
        }
    }

    private static void LabelTypeTime(int value, UILabel[] lblCount, bool lblFormatIncludeX, bool detailMode)
    {
        if (lblCount != null && lblCount.Length != 0)
        {
            lblCount[0].enabled = true;
            lblCount[0].text = $"{value / 60}{Global._instance.GetString("time_3")}";
            if (lblCount.Length > 1)
            {
                lblCount[1].enabled = true;
                lblCount[1].text = $"{value / 60}{Global._instance.GetString("time_3")}";
            }
        }
    }

    private static void LabelTypeDisable(int value, UILabel[] lblCount, bool lblFormatIncludeX, bool detailMode)
    {
        if (lblCount != null && lblCount.Length != 0)
        {
            lblCount.SetText("");
            lblCount[0].enabled = false;
            if (lblCount.Length > 1)
                lblCount[1].enabled = false;
        }
    }



    public static void SetRewardImage(Reward reward, UISprite spr, UIUrlTexture tex, UILabel[] lblCount, float scl, bool lblFormatIncludeX = true, bool detailMode = false, GameObject[] grade = null)
    {
        RewardType type = (RewardType)reward.type;
        int value = reward.value;

        if (detailMode) scl *= 1.5f;

        if (rewardInfo.TryGetValue(type, out RewardInfo info))
        {
            //이미지
            info?.imageAction?.Invoke(info, value, spr, tex, scl, detailMode);
        }
        else if ((int)type > (int)RewardType.material)
        {
            //재료인 경우 (재료는 처리방식이 다름 1001 부터 시작)
            info = rewardInfo[RewardType.material];

            //이미지
            info?.imageAction?.Invoke(info, (int)type, spr, tex, scl, detailMode);
        }
        else
        {
            return;
        }

        //등급
        info?.gradeAction?.Invoke(grade, value, detailMode);

        //레이블
        info?.labelAction?.Invoke(value, lblCount, lblFormatIncludeX, detailMode);
    }



    private static void SettingSpriteSize(UISprite sprite, float scl)
    {
        sprite.keepAspectRatio = UIWidget.AspectRatioSource.Free;
        sprite.MakePixelPerfect();
        sprite.keepAspectRatio = UIWidget.AspectRatioSource.BasedOnWidth;

        sprite.width = (int)(sprite.width * scl);
        sprite.height = (int)(sprite.height * scl);
    }

    private static void SettingUrlTextureSize(UIUrlTexture texture, float scl)
    {
        texture.SuccessEvent += () => SettingTextureSize(texture, scl);
    }

    private static void SettingTextureSize(UIUrlTexture tex, float scl)
    {
        tex.MakePixelPerfect();

        if( tex.width == tex.height)
        {
            tex.width = (int)(70f * scl);
            tex.height = (int)(70f * scl);
        }
        else
        {
            tex.width = (int)(tex.width * scl);
            tex.height = (int)(tex.height * scl);
        }
    }

    public static string GetRewardTextureResourcePath(RewardType type)
    {
        string path = string.Empty;

        if(rewardInfo.TryGetValue(type, out RewardInfo reward))
        {
            path = reward?.GetResourcePath() ?? string.Empty;
        }

        return path;
    }

    public static string GetOverlapTextureResourcePath(int grade)
    {
        return $"Message/adven_overlap_icon_{grade}";
    }
    
    public static string GetOverlapAddressablePath(int grade)
    {
        return $"local_message/item_overlap_{grade}";
    }

    public static void SetTexture(UIUrlTexture target, RewardType type, int value = 0)
    {
        if(rewardInfo.TryGetValue(type, out RewardInfo info))
        {
            info?.imageAction?.Invoke(info, value, null, target, 1.0f, false, false);
        }
        else if ((int)type > (int)RewardType.material)
        {
            info = rewardInfo[RewardType.material];

            info?.imageAction?.Invoke(info, (int)type, null, target, 1.0f, false, false);
        }
    }

    public static string GetRewardName(RewardType type, int val = 0)
    {
        const string DefaultKey = "item_6";

        if(rewardInfo.TryGetValue(type, out RewardInfo reward))
        {

        }
        else if ((int)type > (int)RewardType.material)
        {
            reward = rewardInfo[RewardType.material];
        }

        string key = reward?.GetNameKey(val) ?? string.Empty;

        if (Global._instance.HasString(key))
        {
            if(reward.IsTimeLimitItem)
                return Global._instance.GetString(key).Replace("[n]", (val / 60).ToString());
            else
                return Global._instance.GetString(key);
        }
        else
        {
            return Global._instance.GetString(DefaultKey);
        }
    }

    static public void ProcessTopUIFakeupdate(int type, int value)
    {
        switch ((RewardType)type)
        {
            case RewardType.clover:
                Global.clover += value;
                break;
            case RewardType.coin:
                Global.coin += value;
                break;
            case RewardType.jewel:
                Global.jewel += value;
                break;
            case RewardType.star:
                Global.star += value;
                break;
            case RewardType.wing:
                Global.wing += value;
                break;
            case RewardType.expBall:
                Global.exp += value;
                break;
            default:
                return;
        }

        ManagerUI._instance.UpdateUI();
    }

    public static bool IsActiveRewardCount(RewardType reward)
    {
        if ((int)reward > (int)RewardType.material) reward = RewardType.material;

        if (rewardInfo.ContainsKey(reward))
        {
            if (rewardInfo[reward].IsActiveCount)
                return true;
        }

        return false;
    }

    public static bool IsRewardTypeTimeLimit(RewardType reward)
    {
        if ((int)reward > (int)RewardType.material) reward = RewardType.material;

        if(rewardInfo.ContainsKey(reward))
        {
            if (rewardInfo[reward].IsTimeLimitItem)
                return true;
        }

        return false;
    }
}
