using UnityEngine;
using UnityEditor;

public class StageEncryptTool : MonoBehaviour
{

    [MenuItem("Poko/Stage/StageEncrypt All")]
    public static void StageEncryptAll()
    {
        StageHelper.StageEncrypt("Stage/");
        // StageEncrypt(1, "Stage/");
    }
    /*
    [MenuItem("STAGE/StageEncrypt Edit")]
    public static void StageEncryptEdit()
    {
        int version = 1;
        StageHelper.StageEncrypt(version, "StageEdit/");
    }
    */
}
