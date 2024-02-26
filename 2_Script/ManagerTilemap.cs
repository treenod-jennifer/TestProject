using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ManagerTilemap : MonoBehaviour
{
    [SerializeField] private Transform root;

    public Transform Root { get { return root; } }

    public static ManagerTilemap Instance { get; private set; }

    private Dictionary<string, Tilemap> tiles = new Dictionary<string, Tilemap>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public Tilemap MakeTilemap(string name, Tilemap origin)
    {
        if (!tiles.ContainsKey(name))
        {
            var makeTile = Instantiate(origin, root);

            makeTile.name = makeTile.name.Replace("(Clone)", string.Empty);

            tiles.Add(name, makeTile);

            return makeTile;
        }
        else
        {
            return tiles[name];
        }
    }

    public bool DeleteTilemap(string name)
    {
        if(tiles.TryGetValue(name, out Tilemap tilemap))
        {
            if(tilemap.gameObject != null)
            {
                Destroy(tilemap.gameObject);
            }
            
            tiles.Remove(name);
            return true;
        }
        else
        {
            return false;
        }
    }

#if UNITY_EDITOR
    public bool DeleteTilemap_EditorMode(string name)
    {
        if (tiles.TryGetValue(name, out Tilemap tilemap))
        {
            if (tilemap.gameObject != null)
            {
                DestroyImmediate(tilemap.gameObject);
            }

            tiles.Remove(name);
            return true;
        }
        else
        {
            return false;
        }
    }
#endif

    public void MakeHole(string name, string jsonHoleData)
    {
        AreaTilemapHoleUtility.Hole hole = AreaTilemapHoleUtility.JsonToHole(jsonHoleData);
        MakeHole(name, hole);
    }

    public void MakeHole(string name, AreaTilemapHoleUtility.Hole hole)
    {
        if (tiles.TryGetValue(name, out Tilemap tilemap))
        {
            Vector3Int[] tilesPos = hole.GetTilesPos();
            tilemap.SetTiles(tilesPos, new TileBase[tilesPos.Length]);
        }
    }

    public void MakeHole(string name, string jsonHoleData, float time, DirectionOption option)
    {
        AreaTilemapHoleUtility.Hole hole = AreaTilemapHoleUtility.JsonToHole(jsonHoleData);
        MakeHole(name, hole, time, option);
    }

    public void MakeHole(string name, AreaTilemapHoleUtility.Hole hole, float time, DirectionOption option)
    {
        if (tiles.TryGetValue(name, out Tilemap tilemap))
        {
            StartCoroutine(OutAll(tilemap, hole, time, option));
        }
    }

    private IEnumerator OutAll(Tilemap target, AreaTilemapHoleUtility.Hole hole, float time, DirectionOption option)
    {
        switch (option)
        {
            case DirectionOption.X:
                yield return FadeOutAllTest(true, false);
                break;
            case DirectionOption.Y:
                yield return FadeOutAllTest(false, false);
                break;
            case DirectionOption.ReverseX:
                yield return FadeOutAllTest(true, true);
                break;
            case DirectionOption.ReverseY:
                yield return FadeOutAllTest(false, true);
                break;
            default:
                yield break;
        }

        IEnumerator FadeOutAllTest(bool isWidthSplit, bool isReverse)
        {
            int count = isWidthSplit ? hole.bounds.size.x : hole.bounds.size.y;

            for (int i = 0; i < count; i++)
            {
                AreaTilemapHoleUtility.Hole.HoleBoundInt bounds;

                if (isWidthSplit)
                {
                    int posX;

                    if (isReverse)
                    {
                        posX = hole.bounds.position.x + (count - 1) - i;
                    }
                    else
                    {
                        posX = hole.bounds.position.x + i;
                    }

                    bounds = new AreaTilemapHoleUtility.Hole.HoleBoundInt(
                    posX,
                    hole.bounds.position.y,
                    hole.bounds.position.z,
                    1,
                    hole.bounds.size.y,
                    hole.bounds.size.z);
                }
                else
                {
                    int posY;

                    if (isReverse)
                    {
                        posY = hole.bounds.position.y + (count - 1) - i;
                    }
                    else
                    {
                        posY = hole.bounds.position.y + i;
                    }

                    bounds = new AreaTilemapHoleUtility.Hole.HoleBoundInt(
                    hole.bounds.position.x,
                    posY,
                    hole.bounds.position.z,
                    hole.bounds.size.x,
                    1,
                    hole.bounds.size.z);
                }

                Vector3Int[] pos = hole.GetTilesPos(bounds);

                if (pos != null && pos.Length > 0)
                {
                    target.SetTiles(pos, new UnityEngine.Tilemaps.TileBase[pos.Length]);
                    yield return new WaitForSeconds(time * 0.1f / Global.timeScaleLobby);
                }
            }
        }
    }

    #region 미사용 연출 코드
    //private IEnumerator FadeOut(Tilemap target, Vector3Int[] tilePos, float time)
    //{
    //    float totalTime = 0.0f;
    //    Color startColor = Color.white;
    //    Color endColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);

    //    while (totalTime <= time)
    //    {
    //        totalTime += Global.deltaTimeLobby;

    //        foreach (var pos in tilePos)
    //        {
    //            target.SetColor(pos, Color.Lerp(startColor, endColor, totalTime / time));
    //        }

    //        yield return null;
    //    }

    //    target.SetTiles(tilePos, new UnityEngine.Tilemaps.TileBase[tilePos.Length]);
    //}

    //private IEnumerator FadeOutAll(Tilemap target, AreaTilemapHoleUtility.Hole hole, float time, DirectionOption option)
    //{
    //    switch (option)
    //    {
    //        case DirectionOption.X:
    //            yield return FadeOutAllTest(true, false);
    //            break;
    //        case DirectionOption.Y:
    //            yield return FadeOutAllTest(false, false);
    //            break;
    //        case DirectionOption.ReverseX:
    //            yield return FadeOutAllTest(true, true);
    //            break;
    //        case DirectionOption.ReverseY:
    //            yield return FadeOutAllTest(false, true);
    //            break;
    //        default:
    //            yield break;
    //    }

    //    IEnumerator FadeOutAllTest(bool isWidthSplit, bool isReverse)
    //    {
    //        int count = isWidthSplit ? hole.bounds.size.x : hole.bounds.size.y;

    //        for (int i = 0; i < count; i++)
    //        {
    //            BoundsInt bounds;

    //            if (isWidthSplit)
    //            {
    //                int posX;

    //                if (isReverse)
    //                {
    //                    posX = hole.bounds.position.x + (count - 1) - i;
    //                }
    //                else
    //                {
    //                    posX = hole.bounds.position.x + i;
    //                }

    //                bounds = new BoundsInt(
    //                posX,
    //                hole.bounds.position.y,
    //                hole.bounds.position.z,
    //                1,
    //                hole.bounds.size.y,
    //                hole.bounds.size.z);
    //            }
    //            else
    //            {
    //                int posY;

    //                if (isReverse)
    //                {
    //                    posY = hole.bounds.position.y + (count - 1) - i;
    //                }
    //                else
    //                {
    //                    posY = hole.bounds.position.y + i;
    //                }

    //                bounds = new BoundsInt(
    //                hole.bounds.position.x,
    //                posY,
    //                hole.bounds.position.z,
    //                hole.bounds.size.x,
    //                1,
    //                hole.bounds.size.z);
    //            }

    //            Vector3Int[] pos = hole.GetTilesPos(bounds);

    //            if (pos != null && pos.Length > 0)
    //            {
    //                StartCoroutine(FadeOut(target, hole.GetTilesPos(bounds), time));
    //                yield return new WaitForSeconds(time * 0.1f);
    //            }
    //        }
    //    }
    //}

    //public IEnumerator RandomOut(Tilemap target, AreaTilemapHoleUtility.Hole hole, float time)
    //{
    //    List<Vector3Int> tilesPos = new List<Vector3Int>(hole.GetTilesPos());

    //    int maxCount = tilesPos.Count;

    //    while (tilesPos.Count > 0)
    //    {
    //        int index = UnityEngine.Random.Range(0, tilesPos.Count);

    //        target.SetTile(tilesPos[index], null);

    //        tilesPos.RemoveAt(index);

    //        yield return new WaitForSeconds(time / maxCount);
    //    }
    //}
    #endregion

    public enum DirectionOption
    {
        X,
        Y,
        ReverseX,
        ReverseY
    }
}

