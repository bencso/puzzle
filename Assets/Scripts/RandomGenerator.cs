using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class RandomGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap talajTilemap;
    [SerializeField] private AnimatedTile waterTile;    // VizA animated water tile
    [SerializeField] private RuleTile islandTile;       // Regular land tile
    [SerializeField] private RuleTile bridgeTile;       // Bridge tile
    
    private int mapWidth;
    private int mapHeight;
    private List<Vector2Int> landTiles = new List<Vector2Int>();
    private List<Vector2Int> waterTiles = new List<Vector2Int>();
    private List<Vector2Int> coastTiles = new List<Vector2Int>(); // Water tiles next to land

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        ConnectIslandsWithBridges();
        GenerateStartAndEndPoints();
        GenerateElements();
        PipeHelper.initMap();
        checkEmptyTile();
    }

    private void InitializeMapSize()
    {
        mapWidth = Random.Range(33, 33);
        mapHeight = Random.Range(33, 33);
    }

    private void GenerateBaseTerrain()
    {
        // Fill entire map with animated water
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int tilePos = new Vector3Int(x - mapWidth/2, y - mapHeight/2, 0);
                talajTilemap.SetTile(tilePos, waterTile);
                waterTiles.Add(new Vector2Int(x - mapWidth/2, y - mapHeight/2));
            }
        }
        // Force RuleTile update
        talajTilemap.RefreshAllTiles();
    }

    private void GenerateIslands()
    {
        int numIslands = Random.Range(1, 4);
        for (int i = 0; i < numIslands; i++)
        {
            Vector2Int center = new Vector2Int(
                Random.Range(-mapWidth/2 + 3, mapWidth/2 - 3),
                Random.Range(-mapHeight/2 + 3, mapHeight/2 - 3)
            );
            
            GenerateIsland(center, Random.Range(4, 8));
        }
        UpdateCoastTiles();
    }

    private void GenerateIsland(Vector2Int center, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x*x + y*y <= radius*radius + Random.Range(-2, 3))
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
        // Force RuleTile update after island generation
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

    private void ConnectIslandsWithBridges()
    {
        var islands = FindIslands();
        for (int i = 1; i < islands.Count; i++)
        {
            ConnectIslands(islands[i-1], islands[i]);
        }
    }

    private List<List<Vector2Int>> FindIslands()
    {
        List<List<Vector2Int>> islands = new List<List<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        foreach (var tile in landTiles)
        {
            if (!visited.Contains(tile))
            {
                var island = FloodFill(tile, visited);
                islands.Add(island);
            }
        }

        return islands;
    }

    private List<Vector2Int> FloodFill(Vector2Int start, HashSet<Vector2Int> visited)
    {
        var island = new List<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var pos = queue.Dequeue();
            if (!visited.Contains(pos) && landTiles.Contains(pos))
            {
                visited.Add(pos);
                island.Add(pos);

                foreach (var adj in GetAdjacentPositions(pos))
                {
                    queue.Enqueue(adj);
                }
            }
        }

        return island;
    }

    private void ConnectIslands(List<Vector2Int> island1, List<Vector2Int> island2)
    {
        var start = island1.OrderBy(p => island2.Min(p2 => Vector2Int.Distance(p, p2))).First();
        var end = island2.OrderBy(p => Vector2Int.Distance(p, start)).First();

        var path = FindPath(start, end);
        foreach (var pos in path)
        {
            if (!landTiles.Contains(pos))
            {
                Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);
                talajTilemap.SetTile(tilePos, bridgeTile);
                landTiles.Add(pos);
                waterTiles.Remove(pos);
            }
        }
        // Force RuleTile update after bridge placement
        talajTilemap.RefreshAllTiles();
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

    private void GenerateStartAndEndPoints()
    {
        // Clear existing points
        PipeHelper.startPoints.Clear();
        PipeHelper.endPoints.Clear();

        // Generate Electric start point (on land)
        var electricStart = landTiles[Random.Range(0, landTiles.Count)];
        PipeHelper.startPoints["electric"] = new List<int[]> { new int[] { electricStart.x, electricStart.y, 0 } };

        // Generate Electric end points (1-5 random points on land)
        int numEndPoints = Random.Range(1, 6);
        PipeHelper.endPoints["electric"] = new List<int[]>();
        List<int[]> endPoints = new List<int[]>();
        for (int i = 0; i < numEndPoints; i++)
        {
            var endPoint = landTiles[Random.Range(0, landTiles.Count)];
            endPoints.Add(new int[] { endPoint.x, endPoint.y, 0 });
        }
        PipeHelper.endPoints["electric"] = endPoints;

        // Generate Water start point (in water next to land)
        var waterStart = coastTiles[Random.Range(0, coastTiles.Count)];
        PipeHelper.startPoints["water"] = new List<int[]> { new int[] { waterStart.x, waterStart.y, 0 } };
        
        // Generate Water end point
        var waterEnd = landTiles[Random.Range(0, landTiles.Count)];
        PipeHelper.endPoints["water"] = new List<int[]> { new int[] { waterEnd.x, waterEnd.y, 0 } };

        // Generate Sewage start point (next to electric start)
        var sewageStart = GetAdjacentPositions(electricStart).First(pos => landTiles.Contains(pos));
        PipeHelper.startPoints["sewage"] = new List<int[]> { new int[] { sewageStart.x, sewageStart.y, 0 } };
        
        // Generate Sewage end point
        var sewageEnd = landTiles[Random.Range(0, landTiles.Count)];
        PipeHelper.endPoints["sewage"] = new List<int[]> { new int[] { sewageEnd.x, sewageEnd.y, 0 } };
    }

    private void GenerateElements()
    {
        var elements = new List<Element>();

        // Generate wheat (on land, away from electric points)
        int numWheat = Random.Range(5, 11);
        for (int i = 0; i < numWheat; i++)
        {
            var pos = landTiles[Random.Range(0, landTiles.Count)];
            if (!IsNearElectric(pos))
            {
                elements.Add(new Element { name = "buza", position = pos });
            }
        }

        // Generate fish (in water)
        int numFish = Random.Range(3, 7);
        for (int i = 0; i < numFish; i++)
        {
            var pos = waterTiles[Random.Range(0, waterTiles.Count)];
            elements.Add(new Element { name = "hal", position = pos });
        }

        // Generate mountains (on land)
        int numMountains = Random.Range(2, 5);
        for (int i = 0; i < numMountains; i++)
        {
            var pos = landTiles[Random.Range(0, landTiles.Count)];
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

    // check if we got an empty tile on the talaj tilemap we have a tile which is null and if so, place a water tile on it
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
