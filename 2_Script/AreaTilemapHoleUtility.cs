using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AreaTilemapHoleUtility : MonoBehaviour
{
    private Tilemap tilemap = null;

    private Tilemap Tilemap
    {
        get
        {
            if(tilemap == null)
            {
                tilemap = gameObject.GetComponent<Tilemap>();
            }

            return tilemap;
        }
    }

    [SerializeField] private Tile defaultHoleTile;
    [SerializeField] private TextAsset holeData;

    public TextAsset HoleData { set { holeData = value; } }

    public string GetHoleData()
    {
        BoundsInt bounds = Tilemap.cellBounds;
        TileBase[] tiles = Tilemap.GetTilesBlock(bounds);
        Hole.HolePosInt[] tilesPos = new Hole.HolePosInt[tiles.Length];

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                int index = x + y * bounds.size.x;

                if (tiles[index] == defaultHoleTile)
                {
                    tilesPos[index] = new Hole.HolePosInt(bounds.position.x + x, bounds.position.y + y, 0);
                }
                else
                {
                    tilesPos[index] = null;
                }
            }
        }

        Hole tileData = new Hole(Tilemap.cellBounds, tilesPos);

        string jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(tileData);
        
        return jsonText;
    }

    public bool LoadHoleData()
    {
        if(holeData == null || string.IsNullOrEmpty(holeData.text))
        {
            return false;
        }
        else
        {
            SetHoleData(holeData.text);
            return true;
        }
    }

    private void SetHoleData(string jsonText)
    {
        Hole hole = JsonToHole(jsonText);

        foreach(var tilePos in hole.GetTilesPos())
        {
            Tilemap.SetTile(tilePos, defaultHoleTile);
        }
    }

    public static Hole JsonToHole(string jsonText)
    {
        Hole hole = Newtonsoft.Json.JsonConvert.DeserializeObject<Hole>(jsonText);
        return hole;
    }

    public class Hole
    {
        public class HoleBoundInt
        {
            public HolePosInt size;
            public HolePosInt position;

            [Newtonsoft.Json.JsonConstructor]
            public HoleBoundInt(HolePosInt size, HolePosInt position)
            {
                this.size = size;
                this.position = position;
            }

            public HoleBoundInt(int posX, int posY, int posZ, int sizeX, int sizeY, int sizeZ)
            {
                position = new HolePosInt(posX, posY, posZ);
                size = new HolePosInt(sizeX, sizeY, sizeZ);
            }

            public HoleBoundInt(BoundsInt bounds) : this(bounds.position.x, bounds.position.y, bounds.position.z, bounds.size.x, bounds.size.y, bounds.size.z) { }
        }

        public class HolePosInt
        {
            public int x;
            public int y;
            public int z;

            public HolePosInt(int x, int y, int z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public Vector3Int ToVector3Int()
            {
                return new Vector3Int(x, y, z);
            }
        }

        public HoleBoundInt bounds;
        public HolePosInt[] tilesPos;

        [Newtonsoft.Json.JsonConstructor]
        public Hole(HoleBoundInt bounds, HolePosInt[] tilesPos)
        {
            this.bounds = bounds;
            this.tilesPos = tilesPos;
        }

        public Hole(BoundsInt bounds, HolePosInt[] tilesPos) : this(new HoleBoundInt(bounds), tilesPos) { }

        public Vector3Int[] GetTilesPos()
        {
            return GetTilesPos(bounds);
        }

        public Vector3Int[] GetTilesPos(HoleBoundInt bounds)
        {
            List<Vector3Int> vector3s = new List<Vector3Int>();

            int xStartIndex = bounds.position.x - this.bounds.position.x;
            int yStartIndex = bounds.position.y - this.bounds.position.y;

            for (int x = xStartIndex; x < bounds.size.x + xStartIndex; x++)
            {
                for (int y = yStartIndex; y < bounds.size.y + yStartIndex; y++)
                {
                    int index = x + y * this.bounds.size.x;

                    if (tilesPos[index] != null)
                    {
                        vector3s.Add(tilesPos[index].ToVector3Int());
                    }
                }
            }

            return vector3s.ToArray();
        }
    }
}
