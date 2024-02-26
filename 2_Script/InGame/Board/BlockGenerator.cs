using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockGenerator : DecoBase
{
    public List<UIBlockSprite> listBlockSprite = new List<UIBlockSprite>();
    public Transform transformImageRoot;

    [System.NonSerialized]
    public List<BlockAndColorData> listImage_BlockAndColor = new List<BlockAndColorData>();
    [System.NonSerialized]
    public int generatorIndex = 0;

    //대표 이미지가 물음표로 설정된 경우
    private bool isQMark = false;
    //대표 이미지가 모두 제거된 경우
    private bool isEndCount = false;

    public override bool IsMakerBlock()
    {
        return true;
    }

    public void InitManinSprite()
    {
        uiSprite.cachedTransform.localPosition = new Vector3(0, 71f, 0);
        uiSprite.depth = (int)GimmickDepth.DECO_OBJECT;
        MakePixelPerfect(uiSprite);
    }

    public override void MakeBlockAction()
    {
        StartCoroutine(ScaleHandle());
    }

    public IEnumerator ScaleHandle(float doScale = 0.95f)
    {
        float actionTime = ManagerBlock.instance.GetIngameTime(0.2f);
        transform.DOScaleY(doScale, actionTime).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(actionTime);

        actionTime = ManagerBlock.instance.GetIngameTime(0.3f);
        transform.DOScaleY(1f, actionTime).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(actionTime);
    }

    public void InitStartBlockSprite(List<BlockAndColorData> listDatas)
    {
        if (listDatas != null)
        {
            listImage_BlockAndColor = listDatas.ConvertAll(x => new BlockAndColorData(x));
        }

        //물음표 표시 확인
        if (listImage_BlockAndColor.Count <= 0)
        {
            isQMark = true;
        }
        else
        {
            //라인폭탄 2개 다 설정된 경우
            int lineBombIndex = listImage_BlockAndColor.FindIndex(x => x.blockType == (int)BlockType.START_Line);
            if (lineBombIndex > -1 && listImage_BlockAndColor[lineBombIndex].subType == 2)
            {
                //가로
                BlockAndColorData tempData = new BlockAndColorData();
                tempData.blockType = (int)BlockType.START_Line;
                tempData.subType = 0;
                listImage_BlockAndColor.Add(tempData);

                //세로
                listImage_BlockAndColor[lineBombIndex].subType = 1;
            }
        }

        UpdateStartBlockSprite();
    }

    public void UpdateStartBlockSprite()
    {
        int startBlockCount = 0;

        foreach (var block in listBlockSprite)
        {
            block.gameObject.SetActive(false);
        }

        if (isQMark == false)
        {
            for (int i = 0; i < listImage_BlockAndColor.Count; i++)
            {
                if (startBlockCount > 2) break;

                //블럭 이미지 이름
                listBlockSprite[startBlockCount].spriteName = GetSpriteName(i);
                listBlockSprite[startBlockCount].gameObject.SetActive(true);
                startBlockCount++;
            }
        }

        //블럭 크기/위치 설정
        float scale = 1.1f;
        float yPos = (ManagerBlock.instance.stageInfo.reverseMove == 0) ? 2f : -2f;

        if ((isEndCount == false && startBlockCount == 0)
            || isQMark == true)
        {
            //대표 이미지 없는 상태로 설치된 경우
            scale = 1f;
            listBlockSprite[0].spriteName = "QuestionMark";
            listBlockSprite[0].gameObject.SetActive(true);
            listBlockSprite[0].transform.localPosition = new Vector3(yPos, 6f, 0f);
        }
        else if (startBlockCount == 1)
        {
            scale = 1.4f;
            listBlockSprite[0].transform.localPosition = new Vector3(yPos, 6f, 0f);
        }
        else if (startBlockCount == 2)
        {
            scale = 1.25f;
            listBlockSprite[0].transform.localPosition = new Vector3(-7f, 6f, 0f);
            listBlockSprite[1].transform.localPosition = new Vector3(7f, 6f, 0f);
        }
        else if (startBlockCount == 3)
        {
            listBlockSprite[0].transform.localPosition = new Vector3(-9f, 8f, 0f);
            listBlockSprite[1].transform.localPosition = new Vector3(9, 8f, 0f);
            listBlockSprite[2].transform.localPosition = new Vector3(0f, -3f, 0f);
        }

        //블럭 뎁스 설정
        for (int i = 0; i < listBlockSprite.Count; i++)
        {
            if (listBlockSprite[i].gameObject.activeInHierarchy == false)
                continue;

            listBlockSprite[i].MakePixelPerfect();
            listBlockSprite[i].transform.localScale = Vector3.one * scale;

            listBlockSprite[i].depth = uiSprite.depth + 1 + i;
        }
    }


    public void RefreshImageList()
    {
        bool isRemoveImage = false;
        bool isRemoveQMark = true;

        //물음표 제거 확인
        if (isQMark == true)
        {
            //더 이상 출력될 수 있는 기믹의 여부로 물음표 제거 조건 검사.
            for (int i = 0; i < ManagerBlock.instance.stageInfo.ListStartInfo.Count; i++)
            {
                int blockType = ManagerBlock.instance.stageInfo.ListStartInfo[i].type;
                if ((blockType <= 32 && ((board.startBlockType & (1 << blockType)) != 0))
                    || (blockType > 32 && ((board.startBlockType2 & (1 << (blockType - 32))) != 0)))
                {
                    if (IsCanMakeBlockType(i, blockType) == true)
                    {
                        isRemoveQMark = false;
                        break;
                    }
                }
            }

            //출력될 기믹이 없으면 물음표 제거 처리.
            if (isRemoveQMark == true)
            {
                isQMark = false;
                isEndCount = true;
                isRemoveImage = true;
            }
        }
        else
        {
            //대표 이미지 제거 확인
            for (int i = listImage_BlockAndColor.Count - 1; i >= 0; i--)
            {
                int blockType = listImage_BlockAndColor[i].blockType;
                int stageInfoIndex = ManagerBlock.instance.stageInfo.ListStartInfo.FindIndex(x => x.type == blockType);

                //더 이상 이미지 출력할 수 없는 상황에 이미지 제거.
                if (blockType != (int)BlockType.NORMAL
                    && (stageInfoIndex == -1 || IsCanMakeBlockType(stageInfoIndex, blockType) == false))
                {
                    listImage_BlockAndColor.RemoveAt(i);
                    isRemoveImage = true;
                }
            }

            if (listImage_BlockAndColor.Count == 0)
                isEndCount = true;
        }

        //이미지가 제거 된 경우 처리
        if (isRemoveImage == true)
        {
            //이미지 갱신
            UpdateStartBlockSprite();

            //스케일 연출
            StartCoroutine(ScaleHandle(0.7f));

            //이펙트
            Vector3 effectPos = uiSprite.gameObject.transform.position;
            if(ManagerBlock.instance.stageInfo.reverseMove == 0)
            {
                effectPos.y -= 0.035f;
            }
            InGameEffectMaker.instance.MakeDuckEffect(effectPos);
        }
    }

    bool IsCanMakeBlockType(int stageInfoIndex, int blockType)
    {
        if (ManagerBlock.instance.stageInfo.ListStartInfo[stageInfoIndex].max_stage_Count != 0
            && (ManagerBlock.instance.stageInfo.ListStartInfo[stageInfoIndex].max_stage_Count <= ManagerBlock.instance.totalCreatBlockTypeCount[blockType]))
        {
            return false;
        }

        return true;
    }

    //블럭 이미지 이름
    string GetSpriteName(int listIndex)
    {
        string blockSpriteName = ((BlockType)listImage_BlockAndColor[listIndex].blockType).ToString();
        BlockColorType blockColorType = (BlockColorType)listImage_BlockAndColor[listIndex].blockColorType;
        if (listImage_BlockAndColor[listIndex].subType != -1)
        {
            blockSpriteName = string.Format("{0}_{1}", blockSpriteName, listImage_BlockAndColor[listIndex].subType.ToString());
        }
        if (blockColorType != BlockColorType.NONE && blockColorType != BlockColorType.RANDOM)
        {
            blockSpriteName = string.Format("{0}_{1}", blockSpriteName, blockColorType.ToString());
        }
        return blockSpriteName;
    }

    public virtual void Init(int idx)
    {
        if (ManagerBlock.instance.stageInfo.reverseMove == 1)
        {
            transform.localEulerAngles = new Vector3(0, 0, 180);
            transformImageRoot.localEulerAngles = new Vector3(0, 0, 180);
            transformImageRoot.localPosition = new Vector3(0, 70, 0);
        }

        generatorIndex = idx;

        InitManinSprite();
        UpdateStartBlockSprite();
    }
}
