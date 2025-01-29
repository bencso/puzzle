using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class RandomGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap talajTilemap;
    [SerializeField] private AnimatedTile waterTile;    
    [SerializeField] private RuleTile islandTile;       
    [SerializeField] private RuleTile bridgeTile;       
    
    private int mapWidth;
    private int mapHeight;
    private List<Vector2Int> landTiles = new List<Vector2Int>();
    private List<Vector2Int> waterTiles = new List<Vector2Int>();
    private List<Vector2Int> coastTiles = new List<Vector2Int>(); 

    void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        talajTilemap.ClearAllTiles();
        InitializeMapSize();
        GenerateBaseTerrain();
        GenerateIslands();
        GenerateStartAndEndPoints();
        GenerateElements();
        PipeHelper.initMap();
        checkEmptyTile();
    }

    private void InitializeMapSize()
    {
        mapWidth = 30;
        mapHeight = 30;
    }

    private void GenerateBaseTerrain()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int tilePos = new Vector3Int(x - mapWidth/2, y - mapHeight/2, 0);
                talajTilemap.SetTile(tilePos, waterTile);
                waterTiles.Add(new Vector2Int(x - mapWidth/2, y - mapHeight/2));
            }
        }
        talajTilemap.RefreshAllTiles();
    }

    private void GenerateIslands()
    {
        int numIslands = Random.Range(1, 3); 
        float centerSpread = mapWidth / 6;


        Vector2Int mainCenter = Vector2Int.zero;
        int mainRadius = Random.Range(6, 10); 
        GenerateIsland(mainCenter, mainRadius);

        for (int i = 1; i < numIslands; i++)
        {
            float angle = Random.Range(0f, 2f * Mathf.PI);
            float distance = Random.Range(mainRadius * 0.5f, centerSpread);
            
            Vector2Int center = new Vector2Int(
                Mathf.RoundToInt(distance * Mathf.Cos(angle)),
                Mathf.RoundToInt(distance * Mathf.Sin(angle))
            );

            center.x = Mathf.Clamp(center.x, -mapWidth/4, mapWidth/4);
            center.y = Mathf.Clamp(center.y, -mapHeight/4, mapHeight/4);
            
            int radius = Mathf.RoundToInt(Random.Range(4, 6));
            GenerateIsland(center, radius);

            var path = FindPath(mainCenter, center);
            foreach (var pos in path)
            {
                Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);
                if (!landTiles.Contains(pos))
                {
                    talajTilemap.SetTile(tilePos, islandTile);
                    landTiles.Add(pos);
                    waterTiles.Remove(pos);
                }
            }
        }
        
        UpdateCoastTiles();
    }

    private void GenerateIsland(Vector2Int center, int radius)
    {
        // Add some randomness to island shape
        float[] angleDistortion = new float[8];
        for (int i = 0; i < 8; i++)
        {
            angleDistortion[i] = Random.Range(0.8f, 1.2f);
        }

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                float angle = Mathf.Atan2(y, x);
                if (angle < 0) angle += 2 * Mathf.PI;
                
                int segment = Mathf.FloorToInt((angle / (2 * Mathf.PI)) * 8);
                float distortedRadius = radius * angleDistortion[segment];

                if (x*x + y*y <= distortedRadius * distortedRadius + Random.Range(-2, 3))
                {
                    Vector2Int pos = center + new Vector2Int(x, y);
                    if (IsInBounds(pos))
                    {
                        Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);
                        talajTilemap.SetTile(tilePos, islandTile);
                        landTiles.Add(pos);
                        waterTiles.Remove(pos);
                    }
                }
            }
        }
        talajTilemap.RefreshAllTiles();
    }

    private void UpdateCoastTiles()
    {
        coastTiles.Clear();
        foreach (var waterTile in waterTiles)
        {
            if (GetAdjacentPositions(waterTile).Any(pos => landTiles.Contains(pos)))
            {
                coastTiles.Add(waterTile);
            }
        }
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        var path = new List<Vector2Int>();
        var current = start;

        while (current != end)
        {
            path.Add(current);
            current = new Vector2Int(
                current.x + Mathf.Clamp(end.x - current.x, -1, 1),
                current.y + Mathf.Clamp(end.y - current.y, -1, 1)
            );
        }
        path.Add(end);

        return path;
    }

    private bool IsCoastTile(Vector2Int pos)
    {
        if (landTiles.Contains(pos))
        {
            return GetAdjacentPositions(pos).Any(adj => waterTiles.Contains(adj));
        }
        return false;
    }

    private Vector2Int GetRandomInlandTile()
    {
        Vector2Int pos;
        do
        {
            pos = landTiles[Random.Range(0, landTiles.Count)];
        } while (IsCoastTile(pos));
        return pos;
    }

    private void GenerateStartAndEndPoints()
    {
        PipeHelper.startPoints.Clear();
        PipeHelper.endPoints.Clear();

        var electricStart = GetRandomInlandTile();
        PipeHelper.startPoints["electric"] = new List<int[]> { new int[] { electricStart.x, electricStart.y, 0 } };

        int numEndPoints = Random.Range(1, 6);
        List<int[]> endPoints = new List<int[]>();
        for (int i = 0; i < numEndPoints; i++)
        {
            var endPoint = GetRandomInlandTile();
            endPoints.Add(new int[] { endPoint.x, endPoint.y, 0 });
        }
        PipeHelper.endPoints["electric"] = endPoints;

        var waterStart = coastTiles[Random.Range(0, coastTiles.Count)];
        PipeHelper.startPoints["water"] = new List<int[]> { new int[] { waterStart.x, waterStart.y, 0 } };
        
        var validWaterEnds = GetAdjacentPositions(electricStart)
            .Where(pos => landTiles.Contains(pos) && !IsCoastTile(pos))
            .ToList();
        var waterEnd = validWaterEnds[Random.Range(0, validWaterEnds.Count)];
        PipeHelper.endPoints["water"] = new List<int[]> { new int[] { waterEnd.x, waterEnd.y, 0 } };

        var validSewageStarts = GetAdjacentPositions(electricStart)
            .Where(pos => landTiles.Contains(pos) && !IsCoastTile(pos) && 
                   pos != waterEnd) 
            .ToList();
        var sewageStart = validSewageStarts[Random.Range(0, validSewageStarts.Count)];
        PipeHelper.startPoints["sewage"] = new List<int[]> { new int[] { sewageStart.x, sewageStart.y, 0 } };
        
        var sewageEnd = GetRandomInlandTile();
        PipeHelper.endPoints["sewage"] = new List<int[]> { new int[] { sewageEnd.x, sewageEnd.y, 0 } };
    }

    private void GenerateElements()
    {
        var elements = new List<Element>();

        int numWheatGroups = Random.Range(2, 4); 
        for (int g = 0; g < numWheatGroups; g++)
        {
            var centerPos = GetRandomInlandTile();
            if (!IsNearElectric(centerPos))
            {
                int wheatInGroup = Random.Range(3, 6);
                elements.Add(new Element { name = "buza", position = centerPos });

                for (int i = 0; i < wheatInGroup - 1; i++)
                {
                    var adjacentPositions = GetGroupAdjacentPositions(centerPos, 2);
                    var validPositions = adjacentPositions
                        .Where(pos => landTiles.Contains(pos) 
                               && !IsCoastTile(pos) 
                               && !IsNearElectric(pos)
                               && !elements.Any(e => e.position.x == pos.x && e.position.y == pos.y))
                        .ToList();

                    if (validPositions.Count > 0)
                    {
                        var wheatPos = validPositions[Random.Range(0, validPositions.Count)];
                        elements.Add(new Element { name = "buza", position = wheatPos });
                    }
                }
            }
        }

        int numFish = Random.Range(3, 7);
        var deepWaterTiles = waterTiles.Except(coastTiles).ToList();
        for (int i = 0; i < numFish; i++)
        {
            var pos = deepWaterTiles[Random.Range(0, deepWaterTiles.Count)];
            elements.Add(new Element { name = "hal", position = pos });
        }

        int numMountains = Random.Range(4, 11);
        for (int i = 0; i < numMountains; i++)
        {
            var pos = GetRandomInlandTile();
            elements.Add(new Element { name = "hegy", position = pos });
        }

        PipeHelper.elements = elements.ToArray();
    }

    private bool IsNearElectric(Vector2Int pos)
    {
        var electricPoints = PipeHelper.startPoints["electric"]
            .Concat(PipeHelper.endPoints["electric"])
            .Select(p => new Vector2Int(p[0], p[1]));

        return electricPoints.Any(ep => Vector2Int.Distance(pos, ep) < 3);
    }

    private List<Vector2Int> GetAdjacentPositions(Vector2Int pos)
    {
        return new List<Vector2Int>
        {
            pos + Vector2Int.up,
            pos + Vector2Int.down,
            pos + Vector2Int.left,
            pos + Vector2Int.right
        }.Where(p => IsInBounds(p)).ToList();
    }

    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= -mapWidth/2 && pos.x < mapWidth/2 &&
               pos.y >= -mapHeight/2 && pos.y < mapHeight/2;
    }
    private void checkEmptyTile()
    {
        BoundsInt bounds = new BoundsInt(-mapWidth/2, -mapHeight/2, 0, mapWidth, mapHeight, 1);
        
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                if (talajTilemap.GetTile(position) == null)
                {
                    talajTilemap.SetTile(position, waterTile);
                }
            }
        }
        talajTilemap.RefreshAllTiles();
    }

    private List<Vector2Int> GetGroupAdjacentPositions(Vector2Int center, int radius)
    {
        var positions = new List<Vector2Int>();
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x == 0 && y == 0) continue;
                var pos = new Vector2Int(center.x + x, center.y + y);
                if (IsInBounds(pos))
                {
                    positions.Add(pos);
                }
            }
        }
        return positions;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
