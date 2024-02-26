using System.Collections;

public class ConnectingStrategyInEditor : BaseConnectingStrategy
{
    public override IEnumerator CoSetUserProfile()
    {
        ProfileManager.InitData();
        yield break;
    }
}
