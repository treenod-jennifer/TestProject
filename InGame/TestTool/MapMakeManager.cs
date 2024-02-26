using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class LevelData
{
    public int id = 0;
    public string data = string.Empty;
    public int status = 0;
    public string updatetd_at = string.Empty;
    public string created_at = string.Empty;
}

public class MapMakeManager : MonoBehaviour {

    public static MapMakeManager instance = null;

    string LevelURL = "https://sms.dev.treenod.com:8088/api/v1/level";
    
    void Awake(){instance = this;}

    [SerializeField]
    public LevelData levelData = new LevelData();

    public void GetMapFromWeb()
    {
        StartCoroutine(WaitForRequest());
    }

    IEnumerator WaitForRequest()
    {
        WWW www;
        www = new WWW(LevelURL);

        yield return www; 
        yield return null;

        if (www.error != null)
        {
            //Debug.Log("실패");
        }
        else
        {
            //Debug.Log("성공");
            string Data = System.Text.Encoding.UTF8.GetString(www.bytes);
            levelData = JsonConvert.DeserializeObject<LevelData>(Data);

            levelData.data = levelData.data.Replace("[", "").Replace("]", "");
            string[] words = levelData.data.Split(',');
            yield return null;

            //맵변경
            float endFloat = 0.8f - 1;

            ManagerBlock.instance.stageInfo = new StageInfo();
            ManagerBlock.instance.stageInfo.ListBlock = new List<BlockInfo>();

            for (int i = 0; i < words.Length; i++)
            {
                float levelCount = (System.Convert.ToSingle(words[i])) * 12;

                int inX = i % 9;
                int inY = i / 9;

                BlockInfo blockInfo = new BlockInfo();
                blockInfo.inX = inX + 1;
                blockInfo.inY = inY + 2;
                blockInfo.ListDeco = new List<DecoInfo>();

                if (levelCount > endFloat + 1)
                {
                    blockInfo.isActiveBoard = 1;
                    blockInfo.type = (int)BlockType.NORMAL;
                    blockInfo.colorType = (int)BlockColorType.RANDOM;
                }
                else
                {
                    blockInfo.isActiveBoard = 0;
                }


                if (levelCount > 12 + endFloat)
                {
                    DecoInfo boardInfo = new DecoInfo();
                    boardInfo.BoardType = (int)BoardDecoType.START;
                    boardInfo.index = int.MaxValue;
                    blockInfo.ListDeco.Add(boardInfo);
                }
                else if (levelCount > 11 + endFloat)//포탈
                {
                    if (EditManager.instance.MM_POTAL)
                    {
                        DecoInfo boardInfo = new DecoInfo();
                        boardInfo.BoardType = (int)BoardDecoType.START;
                        boardInfo.index = int.MaxValue;
                        blockInfo.ListDeco.Add(boardInfo);
                    }
                }
                else if (levelCount > 7 + endFloat)//석판
                {
                    if (levelCount > 10 + endFloat)
                    {
                        if (EditManager.instance.MM_PLANT)
                        {
                            blockInfo.type = (int)BlockType.PLANT;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.count = 2;
                        }
                        else if (EditManager.instance.MM_STONE)
                        {
                            blockInfo.type = (int)BlockType.STONE;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.count = 2;
                        }
                    }
                    else if (levelCount > 9 + endFloat)
                    {
                        if (EditManager.instance.MM_WATER)
                        {
                            DecoInfo boardInfo = new DecoInfo();
                            boardInfo.BoardType = (int)BoardDecoType.WATER;
                            boardInfo.count = 1;
                            blockInfo.ListDeco.Add(boardInfo);
                        }
                    }
                    else if (levelCount > 8 + endFloat)
                    {
                        if (EditManager.instance.MM_CANDY)
                        {
                            blockInfo.type = (int)BlockType.BOX;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.count = 2;
                        }
                        else if (EditManager.instance.MM_SAND)
                        {
                            blockInfo.type = (int)BlockType.GROUND;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                            blockInfo.count = 2;
                        }
                        else if (EditManager.instance.MM_BLACK)
                        {
                            blockInfo.type = (int)BlockType.BLOCK_BLACK;
                            blockInfo.colorType = (int)BlockColorType.NONE;
                        }
                    }

                    if (EditManager.instance.MM_CRACK)
                    {
                        DecoInfo boardInfo = new DecoInfo();
                        boardInfo.BoardType = (int)BoardDecoType.CARCK1;
                        boardInfo.count = 2;
                        blockInfo.ListDeco.Add(boardInfo);
                    }
                }
                else if (levelCount > 6 + endFloat)//잡기돌
                {
                    if (EditManager.instance.MM_NET)
                    {
                        DecoInfo boardInfo = new DecoInfo();
                        boardInfo.BoardType = (int)BoardDecoType.NET;
                        boardInfo.count = 1;
                        blockInfo.ListDeco.Add(boardInfo);
                    }
                }
                else if (levelCount > 5 + endFloat)//열쇠
                {
                    if (EditManager.instance.MM_PLANT)
                    {
                        blockInfo.type = (int)BlockType.PLANT;
                        blockInfo.colorType = (int)BlockColorType.NONE;
                        blockInfo.subType = (int)PLANT_TYPE.NORMAL;
                        blockInfo.count = 2;
                        if (EditManager.instance.MM_KEY)
                        {
                            blockInfo.subType = (int)PLANT_TYPE.KEY;
                        }

                    }
                    else if (EditManager.instance.MM_STONE)
                    {
                        blockInfo.type = (int)BlockType.STONE;
                        blockInfo.colorType = (int)BlockColorType.NONE;
                        blockInfo.subType = (int)STONE_TYPE.NORMAL;
                        blockInfo.count = 2;
                        if (EditManager.instance.MM_KEY)
                        {
                            blockInfo.subType = (int)STONE_TYPE.KEY;
                        }
                    }
                    else if (EditManager.instance.MM_KEY)
                    {
                        blockInfo.type = (int)BlockType.KEY;
                        blockInfo.colorType = (int)BlockColorType.NONE;
                    }
                }
                else if (levelCount > 4 + endFloat)//식물 바위
                {
                    if (EditManager.instance.MM_PLANT)
                    {
                        blockInfo.type = (int)BlockType.PLANT;
                        blockInfo.colorType = (int)BlockColorType.NONE;
                        blockInfo.subType = (int)PLANT_TYPE.NORMAL;
                        blockInfo.count = 2;
                    }
                    else if (EditManager.instance.MM_STONE)
                    {
                        blockInfo.type = (int)BlockType.STONE;
                        blockInfo.colorType = (int)BlockColorType.NONE;
                        blockInfo.subType = (int)STONE_TYPE.NORMAL;
                        blockInfo.count = 2;
                    }
                }
                else if (levelCount > 3 + endFloat)//물
                {
                    if (EditManager.instance.MM_WATER)
                    {
                        DecoInfo boardInfo = new DecoInfo();
                        boardInfo.BoardType = (int)BoardDecoType.WATER;
                        boardInfo.count = 1;
                        blockInfo.ListDeco.Add(boardInfo);
                    }
                }
                else if (levelCount > 2 + endFloat)
                {
                    if (EditManager.instance.MM_CANDY)
                    {
                        blockInfo.type = (int)BlockType.BOX;
                        blockInfo.colorType = (int)BlockColorType.NONE;
                        blockInfo.count = 2;
                    }
                    else if (EditManager.instance.MM_SAND)
                    {
                        blockInfo.type = (int)BlockType.GROUND;
                        blockInfo.colorType = (int)BlockColorType.NONE;
                        blockInfo.count = 2;
                    }
                    else if (EditManager.instance.MM_BLACK)
                    {
                        blockInfo.type = (int)BlockType.BLOCK_BLACK;
                        blockInfo.colorType = (int)BlockColorType.NONE;
                    }
                }

                ManagerBlock.instance.stageInfo.ListBlock.Add(blockInfo);
            }

            ManagerBlock.instance.RefreshBlockManagerByStageData();

            EditManager.instance.InitBlockGuide();
            EditManager.instance.CreateBlockGuide();
        }

    }

}
