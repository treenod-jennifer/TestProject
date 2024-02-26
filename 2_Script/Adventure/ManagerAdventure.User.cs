using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Newtonsoft.Json;

public partial class ManagerAdventure : MonoBehaviour
{

    public enum AnimalFilter : int
    {
        AF_ALL,
        AF_MONSTER,
        AF_EVENT_BONUS,
        AF_OVERLAP_1,
        AF_OVERLAP_2,
        AF_OVERLAP_3,
        AF_OVERLAP_4,
        AF_OVERLAP_5
    }

    public enum AnimalSortOption
    {
        byGrade,
        byMuk,
        byChi,
        byBa,
        byAttr,
        byLevel
    }

    public class UserData : MonoBehaviour
    {
        Dictionary<int, UserDataAnimal> animalDic = new Dictionary<int, UserDataAnimal>();
        SortedDictionary<int, UserDataChapterProgress> chapterProgress = new SortedDictionary<int, UserDataChapterProgress>();

        Dictionary<int, Deck> decks = new Dictionary<int, Deck>();

        public HashSet<IUserAnimalListener> animalListeners = new HashSet<IUserAnimalListener>();
        public HashSet<IUserDeckListener> deckListeners = new HashSet<IUserDeckListener>();

        int chapterCursor = -1;
        int stageCursor = -1;
        
        bool stageAllCleared = false;

        bool firstSync = true;

        // 이벤트 스테이지 보상으로 받았는데, 아직 연출상 표시 안해줬던거
        public AnimalInstance noticePostponedAnimal = null;

        public bool SyncFromServerData()
        {
            SyncFromServer_Animal();
            SyncFromServer_Stage();

            return true;
        }

        private void Awake()
        {
            LoadAnimalGradeInfo();
        }

        public bool SyncFromServer_Stage()
        {
            chapterProgress.Clear();
            stageAllCleared = false;

            for (int i = 0; i < ServerRepos.UserAdventureStages.Count; ++i)
            {
                if (this.chapterProgress.ContainsKey(ServerRepos.UserAdventureStages[i].chapter) == false)
                {
                    UserDataChapterProgress prog = new UserDataChapterProgress();
                    this.chapterProgress.Add(ServerRepos.UserAdventureStages[i].chapter, prog);
                }

                var chapProg = this.chapterProgress[ServerRepos.UserAdventureStages[i].chapter];
                UserDataStageProgress stageProg = new UserDataStageProgress();
                stageProg.clearLevel = ServerRepos.UserAdventureStages[i].flag;
                stageProg.playCount = ServerRepos.UserAdventureStages[i].play;
                stageProg.missionCleared = ServerRepos.UserAdventureStages[i].missionClear != 0;

                chapProg.stageProgress.Add(ServerRepos.UserAdventureStages[i].stage, stageProg);
            }

            for(int i = 0; i < ServerRepos.UserAdventureChapters.Count; ++i)
            {
                UserDataChapterProgress chapProg = null;
                if( chapterProgress.TryGetValue(ServerRepos.UserAdventureChapters[i].chapter, out chapProg ) )
                {
                    chapProg.state = ServerRepos.UserAdventureChapters[i].state;
                }
            }

            stageCursor = -1;
            chapterCursor = -1;
            bool foundPlayableChapter = false;

            int lastChapter = 0;
            int lastStage = 0;

            var chapterContents = Stage.GetChapterIdxList();
            foreach (var chapIdx in chapterContents)
            {
                bool chapterCleared = true;

                bool firstPlayableNormalStageFound = false;
                int stageCursorInChapter = -1;
                var chapterData = ManagerAdventure.Stage.GetChapter(chapIdx);

                if ( chapterProgress.ContainsKey(chapIdx) )
                {
                    var stageList = chapterData.GetStageList();
                    int bonusStageCount = 0;
                    int bonusStageCleared = 0;

                    
                    foreach(var stageData in stageList)
                    {
                        var stageProg = chapterProgress[chapIdx].GetStageProgress(stageData.idx);
                        
                        if ( stageData.stageType == 0 )
                        {
                            if((stageProg == null || stageProg.clearLevel == 0))
                            {
                                chapterCleared = false;

                                if (!firstPlayableNormalStageFound)
                                {
                                    firstPlayableNormalStageFound = true;
                                    stageCursorInChapter = stageData.idx;
                                }
                            }
                        }
                        else if( stageData.stageType == 1)
                        {
                            bonusStageCount++;
                            if ( stageProg != null && stageProg.clearLevel > 0)
                            {
                                bonusStageCleared++;
                            }
                        }

                        lastChapter = chapIdx;
                        lastStage = stageData.idx;
                    }
                    chapterProgress[chapIdx].chapterCleared = chapterCleared;
                    chapterProgress[chapIdx].bonusStageCleared = bonusStageCount > 0 && bonusStageCleared == bonusStageCount;
                }
                else
                {
                    chapterCleared = false;
                    stageCursorInChapter = 1;
                }

                if( !chapterCleared && !foundPlayableChapter )
                {
                    foundPlayableChapter = true;
                    chapterCursor = chapIdx;
                    stageCursor = stageCursorInChapter;
                }
            }

            if( !foundPlayableChapter )
            {
                // 끝까지 왔는데 적당히 뭐 갈만한데가 안보이는 경우
                chapterCursor = lastChapter;
                stageCursor = lastStage;
                stageAllCleared = true;
            }
            firstSync = false;
            return true;
        }

        public int GetLastUnfinishedChapter()
        {
            int ret = 0;
            foreach(var cp in this.chapterProgress)
            {
                if( cp.Value.chapterCleared && cp.Value.state < 2 )
                {
                    ret = cp.Key;
                }
            }

            // 아직 보상 안받은 챕터중에서 가장 뒤에꺼 하나만 리턴한다
            // 중간그어디쯤의 보상을 막 여러개 안받은 경우는 사실 테스트 할 때 이외엔 없다고 보면 된다
            return ret;
        }

        public int GetLastChallengeClearedChapter()
        {
            int ret = 0;
            foreach (var cp in this.chapterProgress)
            {
                if (cp.Value.bonusStageCleared && cp.Value.state < 4)
                {
                    ret = cp.Key;
                }
            }
            return ret;
        }

        public bool IsAllCleared(bool includeExtraStageType)
        {
            var stageOrderList = Stage.GetStageOrderList(includeExtraStageType);

            foreach( var stage in stageOrderList )
            {
                int chapIdx = stage.Value.chapIdx;
                int stageIdx = stage.Value.stageIdx;

                var chapProg = GetChapterProgress(chapIdx);
                if (chapProg == null)
                    return false;
                var stageProg = chapProg.GetStageProgress(stageIdx);
                if (stageProg == null || stageProg.clearLevel == 0)
                    return false;
            }
            return true;
        }

        public int GetClearLevel()
        {
            if (IsAllCleared(true))
                return 2;
            else if (IsAllCleared(false))
                return 1;
            else return 0;
        }

        public bool SyncFromServer_Animal()
        {
            animalDic.Clear();
            decks.Clear();

            for (int i = 0; i < ServerRepos.UserAdventureAnimals.Count; ++i)
            {
                this.animalDic.Add(ServerRepos.UserAdventureAnimals[i].animalId,
                    new UserDataAnimal()
                    {
                        animalIdx = ServerRepos.UserAdventureAnimals[i].animalId,
                        exp = ServerRepos.UserAdventureAnimals[i].exp,
                        totalExp = ServerRepos.UserAdventureAnimals[i].expTotal,
                        gettime = 1,
                        grade = ServerRepos.UserAdventureAnimals[i].grade,
                        level = ServerRepos.UserAdventureAnimals[i].level,
                        overlap = ServerRepos.UserAdventureAnimals[i].Overlap,
                        lookId = ServerRepos.UserAdventureAnimals[i].lookId,
                        //eventTs = ServerRepos.UserAdventureAnimals[i].eventTs
                    });
            }

            for (int i = 0; i < ServerRepos.UserAdventureDecks.Count; ++i)
            {

                var newDeck = new Deck();
                for (int j = 0; j < ServerRepos.UserAdventureDecks[i].animals.Count; ++j)
                {
                    newDeck.party.Add(ServerRepos.UserAdventureDecks[i].animals[j]);
                }

                decks.Add(ServerRepos.UserAdventureDecks[i].deckId, newDeck);
            }

            return true;

        }

        public IEnumerator SaveDeck()
        {
            if (this.decks.ContainsKey(1) == false)
                yield break;

            bool retReceived = false;
            ServerAPI.AdventureSetDeck(1, this.decks[1].party,
                (Protocol.AdventureSetDeckResp resp) =>
                {
                    retReceived = true;

                    if (resp.IsSuccess)
                    {
                    }
                }
                );

            while (!retReceived)
                yield return new WaitForSeconds(0.1f);
        }

        class AnimalStatComparer : IComparer<AnimalInstance>
        {
            public int enemyAttr = 0;
            public int enemyAttrSize = 0;
            public bool eventBonus = false;

            private int GetMultiplier(int value, int weight)
            {
                return Mathf.RoundToInt(value * (1.0f + weight * 0.01f));
            }

            // 보유한 동물 중 라이프 + (속성효과가 적용된 파워*10)의 값이 가장 큰 동물 3명을 자동으로 편성
            // 계산값이 동일할 경우 높은 등급의 동물을 편성
            // 등급까지 동일할 경우 인덱스 번호가 큰(뒤에나온 동물) 우선으로 편성

            public int Compare(AnimalInstance a, AnimalInstance b)
            {
                int aAttack = a.atk;
                int bAttack = b.atk;

                #region Attribute
                int aAttribute = 0;
                int bAttribute = 0;

                if (enemyAttr != 0)
                {
                    aAttribute = AttributeCalculator.Calculate(enemyAttr, a.attr, enemyAttrSize);
                    bAttribute = AttributeCalculator.Calculate(enemyAttr, b.attr, enemyAttrSize);
                }
                #endregion

                #region EventBonus
                int aEventBonus = 0;
                int bEventBonus = 0;

                if (eventBonus)
                {
                    aEventBonus = EventData.IsAdvEventBonusAnimal(a.idx) ? EventData.GetAdvEventBonus() : 0;
                    bEventBonus = EventData.IsAdvEventBonusAnimal(b.idx) ? EventData.GetAdvEventBonus() : 0;
                }
                #endregion

                aAttack = GetMultiplier(aAttack, aAttribute + aEventBonus);
                bAttack = GetMultiplier(bAttack, bAttribute + bEventBonus);

                int aTotal = (aAttack * 10) + a.hp;
                int bTotal = (bAttack * 10) + b.hp;

                int ret = 0;

                //1 순위 : Total 값 기준
                ret = CompareValue(aTotal, bTotal);

                //2 순위 : 이벤트 보너스 효과를 우선
                if (ret == 0) ret = CompareValue(aEventBonus, bEventBonus);

                //마지막 순위 : index 값 기준
                if (ret == 0) ret = CompareValue(a.idx, b.idx);

                //Debug.Log(string.Format("Cmp A {0}: {1} / {2}: {3}, {4}", a.idx, aTotal, b.idx, bTotal, ret));
                return ret;
            }

            private int CompareValue(int a, int b)
            {
                if (a < b)      return 1;
                else if (a > b) return -1;
                else            return 0;
            }
        }

        public IEnumerator SetRecommendDeck(int stageAttr, int stageAttrSize, bool eventBonus, System.Action handler)
        {
            if (this.decks.ContainsKey(1) == false)
                yield break;


            var animalList = GetAnimalList(false);
            animalList.Sort(new AnimalStatComparer() { enemyAttr = stageAttr, enemyAttrSize = stageAttrSize, eventBonus = eventBonus });

            for(int i = 0; i < 3; ++i)
            {
                this.decks[1].party[i] = animalList[i].idx;
            }

            bool retReceived = false;
            ServerAPI.AdventureSetDeck(1, this.decks[1].party,
                (Protocol.AdventureSetDeckResp resp) =>
                {
                    retReceived = true;

                    if (resp.IsSuccess)
                    {
                        handler();
                    }
                }
                );

            while (!retReceived)
                yield return new WaitForSeconds(0.1f);
        }

        private IEnumerator CoAction(float _startDelay = 0.0f, System.Action _action = null)
        {
            yield return new WaitForSeconds(_startDelay);
            _action();
        }

        public void Summon(CdnAdventureGachaProduct product, System.Action<ManagerAdventure.AnimalInstance, ManagerAdventure.AnimalInstance, List<Reward>> summonHandler, System.Action<int> failHandler)
        {
            if (product.expired_at != 0 && Global.LeftTime(product.expired_at) < 0)
            {
                StartCoroutine(CoAction(0f, () => { failHandler(200); }));
                return;
            }

            switch (product.asset_type )
            {
                case 2:
                    Summon_Normal(product, summonHandler, failHandler);
                    break;

                case 3:
                    Summon_Premium(product, summonHandler, failHandler);
                    break;
            }
        }

        public void Summon_Normal(CdnAdventureGachaProduct product, System.Action<ManagerAdventure.AnimalInstance, ManagerAdventure.AnimalInstance, List<Reward> > summonHandler, System.Action<int> failHandler)
        {
            NetworkLoading.MakeNetworkLoading(1f);

            ServerAPI.AdventureCheckGachaProduct(product.product_id, (Protocol.AdventureCheckGachaProductResp resp)
                =>
            {
                if (resp.IsSuccess)
                {
                    if (resp.canGacha)
                    {
                        Summon_Execute_Normal(product, summonHandler, failHandler);
                    }
                    else
                    {
                        NetworkLoading.EndNetworkLoading();
                        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_26"), false);
                        popup.SetResourceImage("Message/tired");
                        popup.SortOrderSetting();
                        return;
                    }

                }
                else
                {
                    NetworkLoading.EndNetworkLoading();
                    failHandler(resp.code);
                }

            });
        }

        public void Summon_Premium(CdnAdventureGachaProduct product, System.Action<ManagerAdventure.AnimalInstance, ManagerAdventure.AnimalInstance, List<Reward>> summonHandler, System.Action<int> failHandler)
        {
            NetworkLoading.MakeNetworkLoading(1f);

            ServerAPI.AdventureCheckGachaProduct(product.product_id, (Protocol.AdventureCheckGachaProductResp resp)
                =>
            {
                if (resp.IsSuccess)
                {
                    if (resp.canGacha)
                    {
                        Summon_Execute_Premium(product, summonHandler, failHandler);
                    }
                    else
                    {
                        NetworkLoading.EndNetworkLoading();
                        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_26"), false);
                        popup.SetResourceImage("Message/tired");
                        popup.SortOrderSetting();
                        return;
                    }

                }
                else
                {
                    NetworkLoading.EndNetworkLoading();
                    failHandler(resp.code);
                }

            });
        }

        private void Summon_Execute_Normal(CdnAdventureGachaProduct product, System.Action<ManagerAdventure.AnimalInstance, ManagerAdventure.AnimalInstance, List<Reward>> summonHandler, System.Action<int> failHandler)
        {
            int usePCoin = 0;
            int useFreeCoin = 0;

            if ((int)ServerRepos.User.coin >= product.price)
            {
                usePCoin = product.price;
            }
            else if ((int)ServerRepos.User.coin > 0)
            {
                usePCoin = (int)ServerRepos.User.coin;
                useFreeCoin = product.price - (int)ServerRepos.User.coin;
            }
            else
            {
                useFreeCoin = product.price;
            }

            long preCoin = ServerRepos.User.AllCoin;

            ServerAPI.ExChangeCoin2Gacha(product.product_id, (Protocol.PurchaseGachaResp resp) =>
            {
                if (resp.IsSuccess)
                {
                    if (resp.userAdvAnimal == null)
                    {
                        string title = Global._instance.GetString("p_t_4");
                        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                        popup.InitSystemPopUp(title, Global._instance.GetString("n_s_26"), false);
                        popup.SetResourceImage("Message/tired");
                        popup.SortOrderSetting();
                    }
                    else
                    {
                        Global.coin = (int)GameData.Asset.AllCoin;
                        Global.exp = (int)GameData.User.expBall;

                        ManagerAdventure.AnimalInstance orgData = ManagerAdventure.User.GetAnimalInstance(resp.userAdvAnimal.animalId);

                        ManagerAdventure.User.SyncFromServerData();
                        ManagerAIAnimal.Sync();

                        ManagerAdventure.AnimalInstance aData = ManagerAdventure.User.GetAnimalInstance(resp.userAdvAnimal.animalId);
                        summonHandler(orgData, aData, resp.rewards);

                        foreach (var listener in this.animalListeners)
                        {
                            listener.OnAnimalChanged(resp.userAdvAnimal.animalId);
                        }

                        long postCoin = ServerRepos.User.AllCoin;
                        long coinDiff = preCoin - postCoin;

                        if(coinDiff == 0)
                        {
                            usePCoin = 0;
                            useFreeCoin = 0;
                        }

                        //그로씨
                        if (product.price != 0 )
                        {
                            var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                                (
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_GACHA,
                                -usePCoin,
                                -useFreeCoin,
                                (int)(ServerRepos.User.coin),
                                (int)(ServerRepos.User.fcoin),
                                mrsn_DTL: $"gacha_{product.gacha_id}"
                                );
                            var docMoney = JsonConvert.SerializeObject(growthyMoney);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
                        }

                        ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                            (int)RewardType.animal,
                            resp.userAdvAnimal.animalId,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.NULL,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_GACHA,
                            null
                            );

                        if (resp.rewards != null)
                        {
                            foreach (var temp in resp.rewards)
                            {
                                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                                (int)temp.type,
                                temp.value,
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_GACHA,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_GACHA,
                                null
                                );
                            }
                        }

                    }
                }
                else if (resp.code == 100 || resp.code == 102)
                {
                    ManagerUI._instance.LackCoinsPopUp();
                }
                else
                {
                    failHandler(resp.code);
                }
                NetworkLoading.EndNetworkLoading();
            }
            );
        }

        private void Summon_Execute_Premium(CdnAdventureGachaProduct product, System.Action<ManagerAdventure.AnimalInstance, ManagerAdventure.AnimalInstance, List<Reward>> summonHandler, System.Action<int> failHandler)
        {
            int usePJewel = 0;
            int useFJewel = 0;

            if ((int)ServerRepos.User.jewel >= product.price)
            {
                usePJewel = product.price;
            }
            else if ((int)ServerRepos.User.jewel > 0)
            {
                usePJewel = (int)ServerRepos.User.jewel;
                useFJewel = product.price - (int)ServerRepos.User.jewel;
            }
            else
            {
                useFJewel = product.price;
            }

            long preJewel = ServerRepos.User.AllJewel;


            ServerAPI.BuyGacha(product.product_id, (Protocol.PurchaseGachaResp resp) =>
            {
                if (resp.IsSuccess)
                {
                    if (resp.userAdvAnimal == null)
                    {
                        string title = Global._instance.GetString("p_t_4");
                        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                        popup.InitSystemPopUp(title, Global._instance.GetString("n_s_26"), false);
                        popup.SetResourceImage("Message/tired");
                        popup.SortOrderSetting();
                    }
                    else
                    {
                        Global.coin = (int)GameData.Asset.AllCoin;
                        Global.jewel = (int)GameData.Asset.AllJewel;
                        Global.exp = (int)GameData.User.expBall;

                        ManagerAdventure.AnimalInstance orgData = ManagerAdventure.User.GetAnimalInstance(resp.userAdvAnimal.animalId);

                        ManagerAdventure.User.SyncFromServerData();
                        ManagerAIAnimal.Sync();

                        ManagerAdventure.AnimalInstance aData = ManagerAdventure.User.GetAnimalInstance(resp.userAdvAnimal.animalId);
                        summonHandler(orgData, aData, resp.rewards);

                        foreach (var listener in this.animalListeners)
                        {
                            listener.OnAnimalChanged(resp.userAdvAnimal.animalId);
                        }

                        long postJewel = ServerRepos.User.AllJewel;
                        long coinDiff = preJewel - postJewel;

                        if (coinDiff == 0)
                        {
                            usePJewel = 0;
                            useFJewel = 0;
                        }

                        //그로씨
                        if (product.price != 0)
                        {
                            var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                                (
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_GACHA,
                                -usePJewel,
                                -useFJewel,
                                (int)(ServerRepos.User.jewel),
                                (int)(ServerRepos.User.fjewel),
                                mrsn_DTL: $"gacha_{product.gacha_id}"
                                );
                            var docMoney = JsonConvert.SerializeObject(growthyMoney);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
                        }

                        ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                            (int)RewardType.animal,
                            resp.userAdvAnimal.animalId,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.NULL,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_GACHA,
                            null
                            );

                        if (resp.rewards != null)
                        {
                            foreach (var temp in resp.rewards)
                            {
                                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                                (int)temp.type,
                                temp.value,
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_GACHA,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_GACHA,
                                null
                                );
                            }
                        }
                    }
                }
                else if (resp.code == 100 || resp.code == 102)
                {
                    ManagerUI._instance.LackDiamondsPopUp();
                }
                else
                {
                    failHandler(resp.code);
                }
                NetworkLoading.EndNetworkLoading();
            }
            );
        }


        public void AnimalLevelup(int animalIdx, System.Action<int> successHandler)
        {
            NetworkLoading.MakeNetworkLoading(1f);
            Debug.Log("OnClickLevelUp");
            ServerAPI.AdventureAnimalLevelUp(animalIdx, 
                (Protocol.AdventureAnimalLevelUpResp resp) =>
                {
                    NetworkLoading.EndNetworkLoading();
                    ManagerAdventure.User.SyncFromServerData();
                    if (resp.IsSuccess)
                    {
                        Global.coin = (int)GameData.Asset.AllCoin;
                        Global.exp = (int)GameData.User.expBall;

                        if (successHandler != null)
                            successHandler(animalIdx);

                        foreach (var listener in this.animalListeners)
                        {
                            listener.OnAnimalChanged(animalIdx);
                        }
                    }
                }
                );

        }

        public void AnimalLookChange(int animalIdx, int lookID, System.Action<bool> completeEvent = null)
        {
            NetworkLoading.MakeNetworkLoading(1f);

            ServerAPI.AdventureAnimalChangeLook
            (
                animalIdx, lookID,
                (Protocol.AdventureAnimalChangeLook resp) =>
                {
                    NetworkLoading.EndNetworkLoading();
                    SyncFromServerData();

                    if (resp.IsSuccess)
                    {
                        foreach (var listener in this.animalListeners)
                        {
                            listener.OnAnimalChanged(animalIdx);
                        }
                    }

                    completeEvent?.Invoke(resp.IsSuccess);
                }
            );
        }

        public int GetAnimalIdxFromDeck(int deckIdx, int posInDeck)
        {
            return decks[deckIdx].party[posInDeck];
        }

        public AnimalInstance GetAnimalFromDeck(int deckIdx, int posInDeck)
        {
            return GetAnimalInstance(animalDic[decks[deckIdx].party[posInDeck]]);
        }

        public bool SetAnimalToDeck(int deckIdx, int posInDeck, int animalIdx)
        {
            decks[deckIdx].party[posInDeck] = animalIdx;
            return true;
        }

        public UserDataChapterProgress GetChapterProgress(int chapterIdx)
        {
            UserDataChapterProgress cp = null;
            chapterProgress.TryGetValue(chapterIdx, out cp);
            return cp;
        }

        public void OnReboot()
        {
            Debug.Log("ManagerAdv.User.OnReboot");
            animalDic.Clear();
            this.decks.Clear();

        }

        public void SetTestData()
        {
            animalDic.Clear();
            this.animalDic.Add(1, new UserDataAnimal() { animalIdx = 1, exp = 10, gettime = 1, grade = 3, level = 1, overlap = 0 });
            this.animalDic.Add(2, new UserDataAnimal() { animalIdx = 2, exp = 10, gettime = 1, grade = 3, level = 1, overlap = 0 });
            this.animalDic.Add(3, new UserDataAnimal() { animalIdx = 3, exp = 10, gettime = 1, grade = 4, level = 1, overlap = 10 });
            this.animalDic.Add(4, new UserDataAnimal() { animalIdx = 4, exp = 10, gettime = 1, grade = 5, level = 1, overlap = 20 });

            this.decks.Clear();
            this.decks.Add(0, new Deck());
            decks[0].party.Add(1);
            decks[0].party.Add(2);
            decks[0].party.Add(3);

        }

        public List<AnimalInstance> GetAnimalList(bool includeNull = true, AnimalFilter filter = AnimalFilter.AF_ALL, AnimalSortOption sortOpt = AnimalSortOption.byGrade, bool deckAnimalFirst = true)
        {
            bool isJp = LanguageUtility.SystemCountryCode.Equals("jp");
            bool isTw = LanguageUtility.SystemCountryCode.Equals("tw");
            
            #region fillterSetting
            System.Func<AnimalInstance, bool> filterFunc;
            switch (filter)
            {
                case AnimalFilter.AF_ALL:
                    filterFunc = filterAnimal_ALL;
                    break;
                case AnimalFilter.AF_MONSTER:
                    filterFunc = filterAnimal_MONSTER;
                    break;
                case AnimalFilter.AF_EVENT_BONUS:
                    filterFunc = filterAnimal_EVENT_BONUS;
                    break;
                case AnimalFilter.AF_OVERLAP_1:
                    filterFunc = (animal) => { return filteranimal_OVERLAP(animal, 1); };
                    break;
                case AnimalFilter.AF_OVERLAP_2:
                    filterFunc = (animal) => { return filteranimal_OVERLAP(animal, 2); };
                    break;
                case AnimalFilter.AF_OVERLAP_3:
                    filterFunc = (animal) => { return filteranimal_OVERLAP(animal, 3); };
                    break;
                case AnimalFilter.AF_OVERLAP_4:
                    filterFunc = (animal) => { return filteranimal_OVERLAP(animal, 4); };
                    break;
                case AnimalFilter.AF_OVERLAP_5:
                    filterFunc = (animal) => { return filteranimal_OVERLAP(animal, 5); };
                    break;
                default:
                    filterFunc = filterAnimal_ALL;
                    break;
            }
            #endregion

            #region sortSetting
            System.Comparison<AnimalInstance> sortFunc;
            switch (sortOpt)
            {
                case AnimalSortOption.byGrade:
                    sortFunc = comparisonAnimal_Grade;
                    break;
                case AnimalSortOption.byMuk:
                    sortFunc = comparisonAnimal_Muk;
                    break;
                case AnimalSortOption.byChi:
                    sortFunc = comparisonAnimal_Chi;
                    break;
                case AnimalSortOption.byBa:
                    sortFunc = comparisonAnimal_Ba;
                    break;
                case AnimalSortOption.byAttr:
                    sortFunc = comparisonAnimal_Attck;
                    break;
                case AnimalSortOption.byLevel:
                    sortFunc = comparisonAnimal_Level;
                    break;

                default:
                    sortFunc = comparisonAnimal_Grade;
                    break;
            }
            #endregion
            
            List<AnimalInstance> retList = new List<AnimalInstance>();

            List<AnimalInstance> newList = new List<AnimalInstance>();
            List<AnimalInstance> haveList = new List<AnimalInstance>();
            List<AnimalInstance> dontHaveList = new List<AnimalInstance>();

            var animalKeyList = ManagerAdventure.Animal.GetAnimalKeyList();
            foreach(var ak in animalKeyList)
            {
                var animalInstance = GetAnimalInstance(ak);

                if (animalInstance == null)
                    continue;
                
                if(NewMarkUtility.CompareNewList(animalInstance.idx))     //newList
                {
                    if (filterFunc(animalInstance))     //filtering
                        newList.Add(animalInstance);
                }
                else if (animalInstance.overlap != 0)                   //haveList
                {
                    if(filterFunc(animalInstance))      //filtering
                        haveList.Add(animalInstance);
                }
                else if(includeNull)                                    //dontHaveList
                {
                    var animalBase = ManagerAdventure.Animal.GetAnimal(ak);
                    if (animalBase.limited <= 0 && ServerContents.AdvAnimals.ContainsKey(ak))
                    {
                        if (filterFunc(animalInstance) && //filtering
                            (animalInstance.endTs == 0 || Global.LeftTime(animalInstance.endTs) > 0))
                        {
                            //유저가 동물을 보유하지 않은 상태에서는 현재 접속중인 국가에서 노출되는 캐릭터인지 검사
                            if (isJp)
                            {
                                if (animalInstance.output_jp == 1)
                                    dontHaveList.Add(animalInstance);
                            }
                            else if (isTw)
                            {
                                if (animalInstance.output_tw == 1)
                                    dontHaveList.Add(animalInstance);
                            }
                            else  // 테스트 환경 (한국어)
                                dontHaveList.Add(animalInstance);
                        }
                    }
                }
            }

            newList.Sort(comparisonAnimal_Grade);   //sorting
            haveList.Sort(sortFunc);                //sorting
            dontHaveList.Sort(sortFunc);            //sorting

            retList.AddRange(newList);
            retList.AddRange(haveList);
            retList.AddRange(dontHaveList);

            if (deckAnimalFirst)
                deckAnimalFirstSort(retList);
            
            return retList;
        }

        private bool filterAnimal_ALL(AnimalInstance animal)
        {
            return true;
        }
        
        private bool filterAnimal_MONSTER(AnimalInstance animal)
        {
            if (animal.tags == null)
                return false;

            return StringHelper.TagContains(animal.tags, "MONSTER");
        }

        private bool filterAnimal_EVENT_BONUS(AnimalInstance animal)
        {
            return EventData.IsAdvEventBonusAnimal(animal.idx);
        }

        private bool filteranimal_OVERLAP(AnimalInstance animal, int grade)
        {
            if (animal.grade != grade || animal.overlap == 20)
                return false;

            return StringHelper.TagContains(animal.grade.ToString(), grade.ToString());
        }

        private int comparisonAnimal_Grade(AnimalInstance animal_A, AnimalInstance animal_B)
        {
            int comparison = animal_B.grade - animal_A.grade;
            if (comparison == 0)
                comparison = animal_B.idx - animal_A.idx;
            
            return comparison;
        }

        private int comparisonAnimal_Level(AnimalInstance animal_A, AnimalInstance animal_B)
        {
            int comparison = animal_B.level - animal_A.level;
            if (comparison == 0)
                comparison = animal_B.idx - animal_A.idx;

            return comparison;
        }

        private int comparisonAnimal_Attck(AnimalInstance animal_A, AnimalInstance animal_B)
        {
            int comparison = animal_B.atk - animal_A.atk;
            if (comparison == 0)
                comparison = animal_B.idx - animal_A.idx;

            return comparison;
        }

        private int comparisonAnimal_Muk(AnimalInstance animal_A, AnimalInstance animal_B)
        {
            int animal_A_Priority = Priority_Muk(animal_A.attr);
            int animal_B_Priority = Priority_Muk(animal_B.attr);

            int comparison = animal_B_Priority - animal_A_Priority;
            if (comparison == 0)
                return comparisonAnimal_Grade(animal_A, animal_B);

            return comparison;
        }

        private int Priority_Muk(int attr)
        {
            switch (attr)
            {
                case 3:
                    return 2;
                case 2:
                    return 3;
                case 1:
                    return 1;
                default:
                    return 2;
            }
        }

        private int comparisonAnimal_Chi(AnimalInstance animal_A, AnimalInstance animal_B)
        {
            int animal_A_Priority = Priority_Chi(animal_A.attr);
            int animal_B_Priority = Priority_Chi(animal_B.attr);

            int comparison = animal_B_Priority - animal_A_Priority;
            if (comparison == 0)
                return comparisonAnimal_Grade(animal_A, animal_B);

            return comparison;
        }

        private int Priority_Chi(int attr)
        {
            switch (attr)
            {
                case 3:
                    return 1;
                case 2:
                    return 2;
                case 1:
                    return 3;
                default:
                    return 1;
            }
        }

        private int comparisonAnimal_Ba(AnimalInstance animal_A, AnimalInstance animal_B)
        {
            int animal_A_Priority = Priority_Ba(animal_A.attr);
            int animal_B_Priority = Priority_Ba(animal_B.attr);

            int comparison = animal_B_Priority - animal_A_Priority;
            if (comparison == 0)
                return comparisonAnimal_Grade(animal_A, animal_B);

            return comparison;
        }

        private int Priority_Ba(int attr)
        {
            switch (attr)
            {
                case 3:
                    return 3;
                case 2:
                    return 1;
                case 1:
                    return 2;
                default:
                    return 3;
            }
        }

        private void deckAnimalFirstSort(List<AnimalInstance> animalList)
        {
            List<AnimalInstance> deckAnimalList = new List<AnimalInstance>();
            
            for (int i = 2; i >= 0; --i)
            {
                var aId = ManagerAdventure.User.GetAnimalIdxFromDeck(1, i);
                
                if (DeleteAnimal(animalList, aId))
                    deckAnimalList.Add(ManagerAdventure.User.GetAnimalInstance(aId));
            }
            
            animalList.InsertRange(0, deckAnimalList);
        }
        
        private bool DeleteAnimal(List<AnimalInstance> animalList, int deleteAnimalIdx)
        {
            for(int i=0; i<animalList.Count; i++)
            {
                if(animalList[i].idx == deleteAnimalIdx)
                {
                    animalList.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public AnimalInstance GetAnimalInstance(int animalIdx)
        {
            if (this.animalDic.ContainsKey(animalIdx))
                return GetAnimalInstance(animalDic[animalIdx]);

            var animalBase = ManagerAdventure.Animal.GetAnimal(animalIdx);

            AnimalInstance ret = new AnimalInstance()
            {
                idx = animalIdx,
                gettime = 0,
                grade = animalBase.grade,
                level = 0,
                exp = 0,
                overlap = 0,
                lookId = 0,
                attr = animalBase.attr,
                atkType = animalBase.atkType,
                animalSize = animalBase.animalSize,
                skill = animalBase.skill,
                skillGrade = animalBase.skillGrade,
                lobbyCharIdx = animalBase.lobbyCharIdx,
                specialLobby = animalBase.specialLobby,
                protectedFromMelee = animalBase.protectedFromMelee,
                output_jp = animalBase.output_jp,
                output_tw = animalBase.output_tw,
                
                bulletImageName = animalBase.bulletImageName,

                endTs = animalBase.endTs,

                animalHitSoundName = animalBase.animalHitSoundName,
                animalDamageSoundName = animalBase.animalDamageSoundName,
                enemyHitSoundName = animalBase.enemyHitSoundName,
                enemyDamageSoundName = animalBase.enemyDamageSoundName,

                damageEffectName_1 = animalBase.damageEffectName_1,
                damageEffectName_2 = animalBase.damageEffectName_2,
                hitEffectName_1 = animalBase.hitEffectName_1,
                hitEffectName_2 = animalBase.hitEffectName_2,

                tags = animalBase.tags
            };

            ret.atk = 0;
            ret.hp = 0;

            return ret;
        }

        public AnimalInstance GetAnimalInstance(UserDataAnimal animalData)
        {
            var animalBase = ManagerAdventure.Animal.GetAnimal(animalData.animalIdx);

            AnimalInstance ret = new AnimalInstance()
            {
                idx = animalData.animalIdx,
                gettime = animalData.gettime,
                grade = animalData.grade,
                level = animalData.level,
                exp = animalData.exp,
                totalExp = animalData.totalExp,
                overlap = animalData.overlap,
                lookId = animalData.lookId,
                attr = animalBase.attr,
                atkType = animalBase.atkType,
                animalSize = animalBase.animalSize,
                skill = animalBase.skill,
                skillGrade = animalBase.skillGrade,
                lobbyCharIdx = animalBase.lobbyCharIdx,
                specialLobby = animalBase.specialLobby,
                protectedFromMelee = animalBase.protectedFromMelee,
                output_jp = animalBase.output_jp,
                output_tw = animalBase.output_tw,
                bulletImageName = animalBase.bulletImageName,

                endTs = animalBase.endTs,

                animalHitSoundName = animalBase.animalHitSoundName,
                animalDamageSoundName = animalBase.animalDamageSoundName,
                enemyHitSoundName = animalBase.enemyHitSoundName,
                enemyDamageSoundName = animalBase.enemyDamageSoundName,

                damageEffectName_1 = animalBase.damageEffectName_1,
                damageEffectName_2 = animalBase.damageEffectName_2,
                hitEffectName_1 = animalBase.hitEffectName_1,
                hitEffectName_2 = animalBase.hitEffectName_2,

                tags = animalBase.tags
            };

            if (animalGradeInfo.ContainsKey(animalData.grade))
            {
                var gradeInfo = animalGradeInfo[animalData.grade];

                if( ret.overlap == 0 || ret.level == 0)
                {
                    ret.atk = 1;
                    ret.hp = 1;
                }
                else
                {
                    ret.atk = CalcAtk(animalData.animalIdx, animalData.grade, ret.level, ret.overlap);
                    ret.hp = CalcHp(animalData.animalIdx, animalData.grade, ret.level, ret.overlap);
                }
            }
            else
            {
                ret.atk = 1;
                ret.hp = 1;
            }

            

            return ret;
        }

        public AnimalInstance GetAnimaDefault(int animalIdx)
        {
            var animalBase = ManagerAdventure.Animal.GetAnimal(animalIdx);

            AnimalInstance ret = new AnimalInstance()
            {
                idx = animalIdx,
                gettime = 0,
                grade = animalBase.grade,
                level = 1,
                exp = 0,
                overlap = 1,
                lookId = 0,
                attr = animalBase.attr,
                atkType = animalBase.atkType,
                animalSize = animalBase.animalSize,
                skill = animalBase.skill,
                skillGrade = animalBase.skillGrade,
                lobbyCharIdx = animalBase.lobbyCharIdx,
                specialLobby = animalBase.specialLobby,
                protectedFromMelee = animalBase.protectedFromMelee,
                output_jp = animalBase.output_jp,
                output_tw = animalBase.output_tw,
                bulletImageName = animalBase.bulletImageName,

                endTs = animalBase.endTs,

                animalHitSoundName = animalBase.animalHitSoundName,
                animalDamageSoundName = animalBase.animalDamageSoundName,
                enemyHitSoundName = animalBase.enemyHitSoundName,
                enemyDamageSoundName = animalBase.enemyDamageSoundName,

                damageEffectName_1 = animalBase.damageEffectName_1,
                damageEffectName_2 = animalBase.damageEffectName_2,
                hitEffectName_1 = animalBase.hitEffectName_1,
                hitEffectName_2 = animalBase.hitEffectName_2,

                tags = animalBase.tags
            };

            if (animalGradeInfo.ContainsKey(animalBase.grade))
            {
                var gradeInfo = animalGradeInfo[animalBase.grade];

                if (ret.overlap == 0 || ret.level == 0)
                {
                    ret.atk = 1;
                    ret.hp = 1;
                }
                else
                {
                    ret.atk = CalcAtk(animalIdx, animalBase.grade, 1, 1);
                    ret.hp = CalcHp(animalIdx, animalBase.grade, 1, 1);
                }
            }
            else
            {
                ret.atk = 1;
                ret.hp = 1;
            }



            return ret;
        }

        static public int CalcAtk(int animalIdx, int grade, int level, int overlap)
        {
            var animalBase = ManagerAdventure.Animal.GetAnimal(animalIdx);
            int atk = 1;

            if (animalGradeInfo.ContainsKey(grade))
            {
                var gradeInfo = animalGradeInfo[grade];

                if (overlap > 0 && level > 0)
                {
                    atk = animalBase.defAtk + ((level - 1) * gradeInfo.atkPerLevel) + ((overlap - 1) * gradeInfo.atkPerOverlap);
                }
            }
            return atk;
        }

        static public int CalcHp(int animalIdx, int grade, int level, int overlap)
        {
            var animalBase = ManagerAdventure.Animal.GetAnimal(animalIdx);
            int hp = 1;

            if (animalGradeInfo.ContainsKey(grade))
            {
                var gradeInfo = animalGradeInfo[grade];

                if (overlap > 0 && level > 0)
                {
                    hp = animalBase.defHp + ((level - 1) * gradeInfo.lifePerLevel) + ((overlap - 1) * gradeInfo.lifePerOverlap);
                }
            }
            return hp;
        }
        
        public bool IsOverlapMax(int animalIdx)
        {
            var animalInstance = GetAnimalInstance(animalIdx);
            if (animalInstance == null)
                return true;

            if( animalInstance.overlap < ManagerAdventure.ManagerAnimalInfo.GetMaxOverlap(animalIdx))
            {
                return false;
            }

            return true;
        }
        
        public void AddUserData_Animal(int index, UserDataAnimal animal)
        {
            animalDic.Add(index, animal);
        }

        public void AddUserData_CProgress(int index, UserDataChapterProgress progress)
        {
            chapterProgress.Add(index, progress);
        }

        public void AddUserData_Deck(int index, Deck deck)
        {
            decks.Add(index, deck);
        }

        public int GetStageCursor()
        {
            return this.stageCursor;
        }

        public int GetChapterCursor()
        {
            return this.chapterCursor;
        }

        public bool AllCleared()
        {
            return this.stageAllCleared;
        }

        public bool IsCleared(int chapterIdx, int stageIdx)
        {
            var chapter = GetChapterProgress(chapterIdx);
            if(chapter != null)
            {
                var stage = chapter.GetStageProgress(stageIdx);
                if (stage != null && stage.clearLevel > 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 마지막 플레이한 탐험모드 스테이지가 일반 모드이고, 최초 클리어한 경우
        /// </summary>
        /// <returns></returns>
        public bool IsNormalFirstCleared()
        {
            int lastChapter = GetLastPlayedChapter();
            int lastStage = GetLastPlayedStage();
            var lastPlayStage = Stage.GetStage(lastChapter, lastStage);

            return lastPlayStage?.stageType == 0 &&
                !GetLastPlayedCSCleared() && 
                IsCleared(lastChapter, lastStage);
        }

        /// <summary>
        /// 마지막 플레이한 탐험모드 스테이지가 챌린지 모드이고, 최초 클리어한 경우
        /// </summary>
        /// <returns></returns>
        public bool IsChallengeFirstCleared()
        {
            int lastChapter = GetLastPlayedChapter();
            int lastStage = GetLastPlayedStage();
            var lastPlayStage = Stage.GetStage(lastChapter, lastStage);

            return
                lastPlayStage?.stageType == 1 &&
                !GetLastPlayedCSCleared() &&
                IsCleared(lastChapter, lastStage);
        }

        public void SaveLastSelectedStage(int chapter, int stage)
        {
            PlayerPrefs.SetInt("Adv_LastChapter", chapter);
            PlayerPrefs.SetInt("Adv_LastStage", stage);
            PlayerPrefs.SetInt("Adv_LastCS_Cleared", IsCleared(chapter, stage) ? 1 : 0);
        }

        public int GetLastPlayedChapter()
        {
            return PlayerPrefs.GetInt("Adv_LastChapter", chapterCursor);
        }

        public int GetLastPlayedStage()
        {
            return PlayerPrefs.GetInt("Adv_LastStage", stageCursor);
        }

        public bool GetLastPlayedCSCleared()
        {
            int cleared = PlayerPrefs.GetInt("Adv_LastCS_Cleared");
            return cleared == 1;
        }

        public bool RecommendStage(out int chapIdx, out int stageIdx)
        {
            int lastPlayedChap = GetLastPlayedChapter();
            int lastPlayedStage = GetLastPlayedStage();
            var lastStageInfo = Stage.GetStage(lastPlayedChap, lastPlayedStage);
            if(lastStageInfo == null)
            {
                chapIdx = 1;
                stageIdx = 1;
                return false;
            }

            bool reqAllOrders = lastStageInfo.stageType == 0 ? false : true;
            var playableStage = ScanFirstPlayStage(lastPlayedChap, lastPlayedStage, reqAllOrders);
            if( playableStage != null )
            {
                chapIdx = playableStage.chapIdx;
                stageIdx = playableStage.stageIdx;
                return true;
            }
            else
            {
                playableStage = ScanFirstPlayStage(1, 1, reqAllOrders);

                if( playableStage != null )
                {
                    chapIdx = playableStage.chapIdx;
                    stageIdx = playableStage.stageIdx;
                    return true;
                }
            }

            chapIdx = lastPlayedChap;
            stageIdx = lastPlayedStage;
            return false;
        }

        public void UpdateChapterCursor(int chapIdx, int stageIdx)
        {
            this.chapterCursor = chapIdx;
            this.stageCursor = stageIdx;
        }

        ManagerStageInfo.StageOrderData ScanFirstPlayStage(int startChapter, int startStage, bool scanAllTypeStages)
        {
            int orderKey = startChapter * 100000 + startStage;
            var stageOrders = Stage.GetStageOrderList(scanAllTypeStages);
            int i = stageOrders.IndexOfKey(orderKey);
            if (i == -1)
            {
                return null;
            }

            for (; i < stageOrders.Count; ++i)
            {
                var stageInfo = stageOrders.Values[i];
                var chapProg = this.GetChapterProgress(stageInfo.chapIdx);
                if (chapProg == null)
                {
                    return stageInfo;
                }

                var stageProg = chapProg.GetStageProgress(stageInfo.stageIdx);
                if (stageProg == null || stageProg.clearLevel == 0)
                {
                    return stageInfo;
                }
            }
            return null;
        }

        static Dictionary<int, AnimalGradeInfo> animalGradeInfo = new Dictionary<int, AnimalGradeInfo>();

        void LoadAnimalGradeInfo()
        {
            animalGradeInfo.Clear();
            TextAsset gradeAsset = Resources.Load("TextAsset/adventureAnimalGrade") as TextAsset;
            System.IO.StringReader stringReader = new System.IO.StringReader(gradeAsset.text);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(stringReader.ReadToEnd());

            //최상위 노드 선택.
            XmlNode rootNode = doc.ChildNodes[1];
            for (int i = 0; i < rootNode.ChildNodes.Count; ++i)
            {
                XmlNode node = rootNode.ChildNodes[i];
                int id = xmlhelper.GetInt(node, "grade", 1);
                AnimalGradeInfo newGrade = new AnimalGradeInfo()
                {
                    maxLevel = xmlhelper.GetInt(node, "max_lv", 10),
                    lifePerLevel = xmlhelper.GetInt(node, "life_per_lv", 100),
                    atkPerLevel = xmlhelper.GetInt(node, "atk_per_lv", 10),
                    maxOverlap = xmlhelper.GetInt(node, "max_overlap", 10),
                    lifePerOverlap = xmlhelper.GetInt(node, "life_per_overlap", 100),
                    atkPerOverlap = xmlhelper.GetInt(node, "atk_per_overlap", 10),
                };
                animalGradeInfo.Add(id, newGrade);
            }

        }
    }


    public class UserDataChapterProgress
    {
        public bool chapterCleared;
        public bool bonusStageCleared;
        public int state;
        public SortedDictionary<int, UserDataStageProgress> stageProgress = new SortedDictionary<int, UserDataStageProgress>();
        public UserDataStageProgress GetStageProgress(int stageIdx)
        {
            UserDataStageProgress sp = null;
            stageProgress.TryGetValue(stageIdx, out sp);
            return sp;
        }
    }

    public class UserDataStageProgress
    {
        public int playCount;
        public int clearLevel;
        public bool missionCleared;
    }

    public class Deck
    {
        public List<int> party = new List<int>();
    }


    public class UserDataAnimal
    {
        internal long gettime;
        internal int animalIdx;
        internal int grade;
        internal int level;
        internal int totalExp;
        internal int exp;
        internal int overlap;
        internal int lookId;
    }

    public class AnimalInstance
    {
        internal int idx;
        internal long gettime;
        internal long endTs;

        internal int grade;
        internal int level;
        internal int totalExp;
        internal int exp;
        internal int overlap;
        internal int lookId;
        
        internal int attr;
        
        internal int hp;
        internal int atk;

        internal int atkType;
        internal int animalSize;

        internal int skill;
        internal int skillGrade;

        internal int lobbyCharIdx;
        internal int specialLobby;

        internal bool protectedFromMelee;

        internal int output_jp;
        internal int output_tw;

        internal string bulletImageName;

        internal string animalHitSoundName;
        internal string animalDamageSoundName;
        internal string enemyHitSoundName;
        internal string enemyDamageSoundName;

        internal string damageEffectName_1;
        internal string damageEffectName_2;
        internal string hitEffectName_1;
        internal string hitEffectName_2;

        internal string tags;
    }

    public class AnimalGradeInfo
    {
        internal int maxLevel = 10;
        internal int maxOverlap = 20;
        internal int lifePerLevel = 100;
        internal int atkPerLevel = 10;

        internal int lifePerOverlap = 100;
        internal int atkPerOverlap = 10;
    }
}
