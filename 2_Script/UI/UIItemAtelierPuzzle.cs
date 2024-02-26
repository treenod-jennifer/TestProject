using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIItemAtelierPuzzle : MonoBehaviour
{
    public enum PuzzleState
    {
        CLEAR    = -1,
        COVERED  = 0,
        SELECTED = 1,
    }

    public UIPokoButton _btnPuzzle;
    public int          PuzzleIndex { get; private set; }

    [SerializeField] private UISprite       _spriteCover;
    [SerializeField] private UISprite       _spriteScore;
    [SerializeField] private List<UISprite> _sprAtelierList; //번들 처리 sprite

    public void Init(int index)
    {
        //번들 Atlas 세팅
        foreach (var t in _sprAtelierList)
        {
            t.atlas = ManagerAtelier.instance._atelierPack.AtlasUI;
        }

        PuzzleIndex = index;
    }

    private void OnDestroy()
    {
        foreach (var spr in _sprAtelierList)
            spr.atlas = null;
    }

    // 퍼즐 오픈 실패 - covered / 퍼즐 오픈 성공 - select > clear
    public void SetIcon(ManagerAtelier.PuzzleGameType gameType, PuzzleState puzzleState, int score, bool effect = false)
    {
        var spriteIdx = puzzleState == PuzzleState.COVERED ? 0 : 1;
        _spriteCover.spriteName = $"puzzle_{((int)gameType).ToString()}_{spriteIdx.ToString()}";
        _spriteScore.spriteName = $"drop_{score.ToString()}";
        _spriteScore.gameObject.SetActive(gameType == ManagerAtelier.PuzzleGameType.SCORE);

        var active     = puzzleState != PuzzleState.CLEAR;
        var effectTime = 0.5f;
        var endValue   = active ? 1f : 0f;

        if (effect)
        {
            DOTween.ToAlpha(() => _spriteCover.color, x => _spriteCover.color = x, endValue, effectTime).OnComplete(() => { _spriteCover.gameObject.SetActive(active); });
        }
        else
        {
            _spriteCover.gameObject.SetActive(active);
        }
    }

    public bool IsCanSelect()
    {
        //선택된 스테이지가 없거나 현재 선택된 스테이지라면
        var bCanSelect = ManagerAtelier.instance._currentPuzzleIndex < 0 || ManagerAtelier.instance._currentPuzzleIndex == PuzzleIndex;
        //퍼즐 클리어가 아니라면 선택 가능
        bCanSelect = bCanSelect && ManagerAtelier.instance.IsPuzzleClear(PuzzleIndex, ManagerAtelier.instance._puzzleState) == false;

        return bCanSelect;
    }
}