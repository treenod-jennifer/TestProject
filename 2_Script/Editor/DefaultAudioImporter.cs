using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class DefaultAudioImporter : AssetPostprocessor
{


    void OnPreprocessAudio()
    {
        AudioImporter importer = assetImporter as AudioImporter;
        if (null == importer) return;

        importer.forceToMono = true;
    }

}
