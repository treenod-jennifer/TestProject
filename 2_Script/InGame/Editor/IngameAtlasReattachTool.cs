using UnityEngine;
using UnityEditor;

public class IngameAtlasReattachTool : MonoBehaviour
{
    [MenuItem("Poko/IngameAtlas Reattach")]

    private static void IngameAtlasReattach()
    {
        SetIngameAtlasMaterial("InGameBlock");
        SetIngameAtlasMaterial("InGameBlock2");
        SetIngameAtlasMaterial("InGameTile");
        SetIngameAtlasMaterial("InGameTile_Top");
        SetIngameAtlasMaterial("InGameEffect");
        SetIngameAtlasMaterial("InGameTarget");
        Debug.Log(" === 인게임 아틀라스 적용 ===");
    }

    private static void SetIngameAtlasMaterial(string atlasName)
    {
        string atlasPath = "Assets/4_InResource/Atlas/InGame/";

        //아틀라스 오브젝트 가져오기
        Object atlasObj = AssetDatabase.LoadAssetAtPath(string.Format("{0}/{1}.asset", atlasPath, atlasName), typeof(Object));
        if (atlasObj == null)
            return;

        //아틀라스 머테리얼 가져오기
        Material atlasMaterial = (Material)AssetDatabase.LoadAssetAtPath(string.Format("{0}/{1}.mat", atlasPath, atlasName), typeof(Material));
        if (atlasMaterial == null)
            return;

        //오브젝트 내 ngui 설정 가져오기
        INGUIAtlas ingameAtlas = atlasObj as INGUIAtlas;
        if (ingameAtlas == null)
            return;

        //머테리얼 추가
        ingameAtlas.spriteMaterial = atlasMaterial;
    }
}
