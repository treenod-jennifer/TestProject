using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileMap))]
public class EditorTileMap : Editor
{
    private Vector3 mouseHitPos;

    private void OnSceneGUI()
    {
        if (this.UpdateHitPosition())
            SceneView.RepaintAll();
        this.RecalculateMarkerPosition();
        Event current = Event.current;
        //        if (this.IsMouseOnLayer())
        {
            if (current.type == EventType.MouseDown || current.type == EventType.MouseDrag)
            {
                if (current.button == 1)
                {
                    this.ChangeTileDiretionToMesh();
                    current.Use();
                }
                else if (current.button == 0)
                {
                    this.ChangeTileTextureToMesh();
                    current.Use();
                }
            }
        }
        Handles.BeginGUI();
        GUI.color = Color.red;
        GUI.Label(new Rect(10, 10, 100, 100), "LMB: Draw");
        GUI.Label(new Rect(10, 25, 100, 100), "RMB: Rotation");
        var map = (TileMap)this.target;
        GUI.Label(new Rect(10, 40, 100, 100), "Vertices : " + map.GetVerticesCount());
        GUI.Label(new Rect(10, 55, 100, 100), "Triangles : " + map.GetTrianglesCount());
        GUI.Label(new Rect(10, 70, 100, 100), "x :" + this.GetTilePositionFromMouseLocation().x + ",  z :" + this.GetTilePositionFromMouseLocation().y);
        //        GUI.Label(new Rect(10, 40, 100, 100), "Hit Position : " + mouseHitPos.ToString("0.0"));
        /*
        GUI.Label(new Rect(10, Screen.height - 90, 100, 100), "LMB: Draw");
        GUI.Label(new Rect(10, Screen.height - 105, 100, 100), "RMB: Rotation");
         * */
        Handles.EndGUI();
    }
    
    private void OnEnable()
    {
        Tools.current = Tool.View;
        Tools.viewTool = ViewTool.FPS;
    }

    private void ChangeTileTextureToMesh()
    {
        var map = (TileMap)this.target;
        var tilePos = this.GetTilePositionFromMouseLocation();
        if(tilePos != -Vector2.one)
            map.ChangeTileTextureToMesh((int)tilePos.x, (int)tilePos.y);
    }

    private void ChangeTileDiretionToMesh()
    {
        var map = (TileMap)this.target;
        var tilePos = this.GetTilePositionFromMouseLocation();
        if (tilePos != -Vector2.one)
            map.ChangeTileDiretionToMesh((int)tilePos.x, (int)tilePos.y);
    }

    static RaycastHit[] _rayHits;

    private Vector2 GetTilePositionFromMouseLocation()
    {
        /*
        var view = SceneView.currentDrawingSceneView;
        Camera camera = view.camera;
        */

        var map = (TileMap)this.target;
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        LayerMask mask = -1;
        int nTriagleIndex = 0;

        if (_rayHits == null)
            _rayHits = new RaycastHit[10];
        int nHitCount = Physics.RaycastNonAlloc(ray, _rayHits, Mathf.Infinity, mask, QueryTriggerInteraction.Collide);
        if (nHitCount > 0)
        {
            for (int i = 0; i < nHitCount; i++)
            {
                GameObject go = _rayHits[i].collider.gameObject;
                if (go != null)
                {
                    TileMap tileMap = go.GetComponent<TileMap>();
                    if (tileMap != null)
                    {
                        EditorLevelBlindData window = (EditorLevelBlindData)EditorWindow.GetWindow(typeof(EditorLevelBlindData));
                        if (tileMap.GetBlindAreaIndex() == window._nCurrentArea)
                        {
                            nTriagleIndex = _rayHits[i].triangleIndex/2;
                            break;
                        }
                        else
                            return -Vector2.one;
                    }
                }
            }
        }
        else
            return -Vector2.one;
        /*
        if (Physics.RaycastAll(ray, out hit, Mathf.Infinity, mask))
        {
            if (hit.collider != null)
            {
                nTriagleIndex = hit.triangleIndex / 2;
                //Debug.Log(nTriagleIndex.ToString());
            }
        }
        */

        var col = (int)nTriagleIndex % map._nColumns;
        var row = (int)nTriagleIndex / map._nColumns;
        if (row < 0)
            row = 0;
        if (row > map._nRows - 1)
            row = map._nRows - 1;
        if (col < 0)
            col = 0;
        if (col > map._nColumns - 1)
            col = map._nColumns - 1;

        return new Vector2(col, row);
    }

    private bool IsMouseOnLayer()
    {
        var map = (TileMap)this.target;
        return this.mouseHitPos.x > 0 && this.mouseHitPos.x < (map._nColumns * map._fTileWidthSize) &&
                this.mouseHitPos.y > 0 && this.mouseHitPos.y < (map._nRows * map._fTileHeightSize);
    }

    private void RecalculateMarkerPosition()
    {
        var map = (TileMap)this.target;
        var tilepos = this.GetTilePositionFromMouseLocation();
        var pos = new Vector3(tilepos.x * map._fTileWidthSize, 0, tilepos.y * map._fTileHeightSize);
        map.MarkerPosition = map.transform.position + new Vector3(pos.x + (map._fTileWidthSize / 2), 0, pos.z + (map._fTileHeightSize / 2));
        //        Debug.Log("MarkerPosition : " + map.MarkerPosition.ToString("0.0"));
    }

    private bool UpdateHitPosition()
    {
        var map = (TileMap)this.target;
        var p = new Plane(map.transform.TransformDirection(Vector3.up), map.transform.position);
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        var hit = new Vector3();
        float dist;
        if (p.Raycast(ray, out dist))
            hit = ray.origin + (ray.direction.normalized * dist);
        if (hit != this.mouseHitPos)
        {
            this.mouseHitPos = hit;
            return true;
        }
        return false;
    }
}