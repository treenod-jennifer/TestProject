public partial class ManagerGroupRanking
{
    public class GroupRankingUserData
    {
        public string name;
        public long   score;
        public long   rank;
        public string alterPicture;
        public bool   isGetRankReward;
        public bool   isMyRank;

        public GroupRankingUserData(Protocol.GroupRankingUserData data, bool isMyRank = false, bool isGetRankReward = false)
        {
            if (data == null)
            {
                return;
            }

            name                 = data.name;
            score                = data.score;
            rank                 = data.rank;
            alterPicture         = data.alterPicture;
            this.isMyRank        = isMyRank;
            this.isGetRankReward = isGetRankReward;
        }

        public override string ToString() => $"UserName: {name}, score:{score}, rank:{rank}";
    }
}