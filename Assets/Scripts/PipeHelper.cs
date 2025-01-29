using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;
using System;

[System.Serializable]
public class Utility
{
    public string name;
    public Tile tile;
}

[System.Serializable]
public class Element
{
    public string name;
    public Vector2Int position;
}

[System.Serializable]
public class StartTiles{
    public string name;
    public Tile tile;
}


public class PipeHelper : MonoBehaviour
{
    [SerializeField]
    private Tilemap[] tilemapHelper;
    [SerializeField]
    private RuleTile[] tilesHelper;
    [SerializeField]
    private RuleTile[] tmpTilesHelper;
    [SerializeField]
    private Tilemap tempTilemapHelper;
    [SerializeField]
    private Tilemap buzaTilemapHelper;
    [SerializeField]
    private Tile[] buzatilesHelper;
    [SerializeField]
    private Utility[] utilitiesHelper;
    [SerializeField]
    private Tilemap talajTilemapHelper;
    [SerializeField]
    private Element[] elementsHelper;
    [SerializeField]
    private StartTiles[] startTilesHelper;
    public static Element[] elements;
    public static Tile[] buzatiles;
    public static Tilemap buzaTilemap;
    public static StartTiles[] startTiles;
    public static Tilemap tmpTilemap;
    public static RuleTile[] tmpTiles;
    public static Tilemap[] tilemap;
    public static RuleTile[] tiles;
    public static Utility[] utilities;
    public static Tilemap talajTilemap;
    public static Dictionary<int[], string> pipes = new Dictionary<int[], string>();
    public static List<Dictionary<int[], string>> tempPipes = new List<Dictionary<int[], string>>();

    protected static int selectedPipe = 0;

    private static Dictionary<string, List<int[]>> startPoints = new Dictionary<string, List<int[]>>();
    private static Dictionary<string, List<int[]>> endPoints = new Dictionary<string, List<int[]>>();

    private static List<List<int[]>> routes = new List<List<int[]>>();

    public static string tempTileType = null;

    void Start()
    {
        tilemap = tilemapHelper;
        tiles = tilesHelper;
        tmpTiles = tmpTilesHelper;
        tmpTilemap = tempTilemapHelper;
        buzaTilemap = buzaTilemapHelper;
        buzatiles = buzatilesHelper;
        utilities = utilitiesHelper;
        talajTilemap = talajTilemapHelper;
        elements = elementsHelper;
        startTiles = startTilesHelper;
        startPoints.Add("electric", new List<int[]> { new int[] { -9, -4, 0 } });
        startPoints.Add("water", new List<int[]> { new int[] { 0, -3, 0 } });
        startPoints.Add("sewage", new List<int[]> { new int[] { -10, -6, 0 } });
        endPoints.Add("electric", new List<int[]>());
        endPoints["electric"].Add(new int[] { -3, 1, 0 });
        endPoints["electric"].Add(new int[] { -3, -1, 0 });

        endPoints.Add("water", new List<int[]> { new int[] { 0, 1, 0 } });
        endPoints.Add("sewage", new List<int[]> { new int[] { -10, -4, 0 } });

        if (utilities != null && utilities.Length > 0)
        {
            foreach (var endpoint in endPoints)
            {
                foreach (var pos in endpoint.Value)
                {
                    if(endpoint.Key == "sewage") {
                        buzaTilemap.SetTile(new Vector3Int(pos[0], pos[1], pos[2]), utilities[5].tile);
                    } else {
                        buzaTilemap.SetTile(new Vector3Int(pos[0], pos[1], pos[2]), utilities[0].tile);
                    }
                }
            }
            foreach (var startpoint in startPoints)
            {
                foreach (var pos in startpoint.Value)
                {
                    var matchingTile = startTiles.FirstOrDefault(t => t.name == startpoint.Key);
                    if (matchingTile != null)
                    {
                        buzaTilemap.SetTile(new Vector3Int(pos[0], pos[1], pos[2]), matchingTile.tile);
                    }
                }
            }
        }

        PlaceElements();
    }

    public static void PlaceElements()
    {
        foreach (var element in elements)
        {
            var tilePos = new Vector3Int(element.position.x, element.position.y, 0);
            var currentTile = buzaTilemap.GetTile(tilePos);

            var electricNearby = Enumerable.Range(-1, 3)
                .SelectMany(x => Enumerable.Range(-1, 3)
                    .Select(y => new Vector3Int(element.position.x + x, element.position.y + y, 0)))
                .Any(pos => tilemap[0].GetTile(pos)?.name == "electric")
                || Enumerable.Range(-1, 3).SelectMany(x => Enumerable.Range(-1, 3)
                    .Select(y => new Vector3Int(element.position.x + x, element.position.y + y, 1)))
                .Any(pos => tilemap[1].GetTile(pos)?.name == "electric");

            if (!electricNearby &&
            (currentTile == null ||
            currentTile == buzatiles[1] ||
            currentTile == buzatiles[2] ||
            currentTile == utilities[2].tile))
            {
                switch (element.name)
                {
                    case "buza":
                        buzaTilemap.SetTile(tilePos, buzatiles[0]);
                        break;
                    case "hegy":
                        buzaTilemap.SetTile(tilePos, utilities[3].tile);
                        break;
                    case "hal":
                        buzaTilemap.SetTile(tilePos, utilities[4].tile);
                        break;
                }
            }
        }
    }


    public static void Test()
    {
        // if (pipes != null && pipes.Count > 0) Debug.Log(pipes.Count);
    }

    public static List<Vector3Int> GetAdjacentPositions(Vector3Int position)
    {
        Debug.Log($"pos: {position.x}, {position.y}, {PipeBuilder.currentLayer}");
        position.z = PipeBuilder.currentLayer;
        return new List<Vector3Int>
        {
            position + Vector3Int.right,
            position + Vector3Int.left,
            position + Vector3Int.up,
            position + Vector3Int.down
        };
    }

    private static bool IsRouteEndpoint(Vector3Int position, string pipeType)
    {
        Debug.Log($"pos: {position.x}, {position.y}, {PipeBuilder.currentLayer}");
        foreach (var route in routes) {
            Debug.Log($"route: {route.Count}");
            var ends = GetRouteEndPositions(route);
            Debug.Log($"ends: {ends.Count}");
            foreach (var end in ends) {
                Debug.Log($"end: {end[0]}, {end[1]}, {end[2]} \n {position.x}, {position.y}, {PipeBuilder.currentLayer}");
            }
            if(ends.Any(end => end[0] == position.x && end[1] == position.y && end[2] == (PipeBuilder.currentLayer == 0 ? 1 : 0))) {
                return true;
            }
        }
        return false;
    }

    public static void Place(Vector3Int position)
    {
        int selectedPipe = PipeBuilder.selectedPipe;
        Debug.Log(tiles[selectedPipe].name);
        if (!canBePlaced(new int[] { position.x, position.y, PipeBuilder.currentLayer }))
        {
            return;
        }

        var pipeType = tiles[selectedPipe].name;
        if (pipeType != "water" && pipeType != "electric" && pipeType != "sewage") return;

        var adjacentPositions = GetAdjacentPositions(position);

        string compatibleType = pipeType;
        bool placeable = false;

        if(startPoints[pipeType].Any(startPos => startPos[0] == position.x && startPos[1] == position.y && startPos[2] == position.z)) {
            placeable = true;
            Debug.Log("1. true");
        }


        foreach (var pos in adjacentPositions)
        {
            Debug.Log($"{position.x}, {position.y}, {PipeBuilder.currentLayer} \n pos: {pos.x}, {pos.y}, {PipeBuilder.currentLayer}");
            if (!Exists(new int[] { pos.x, pos.y, PipeBuilder.currentLayer })) {
                if(Exists(new int[] { pos.x, pos.y, PipeBuilder.currentLayer == 0 ? 1 : 0 }) && pipes[GetPipeKey(new int[] { pos.x, pos.y, PipeBuilder.currentLayer == 0 ? 1 : 0 })] == pipeType ) {
{
                        Debug.Log("69-");
                        if(IsRouteEndpoint(new Vector3Int(position.x, position.y, PipeBuilder.currentLayer), pipeType)) {

                            placeable = true;
                            Debug.Log("2. true");
                            break;
                        }
}
                }
                Debug.Log("skipping");                
                continue;
            };
            Debug.Log(pipeType);
            Debug.Log("Exists");


            var existingPipe = pipes[GetPipeKey(new int[] { pos.x, pos.y, PipeBuilder.currentLayer })];
            var adjHelper = GetAdjacentPositions(pos);

            if(Exists(new int[] { pos.x, pos.y, PipeBuilder.currentLayer })) {
                Debug.Log($"1.1  \n {pos.x}, {pos.y}, {PipeBuilder.currentLayer} \n {pipes[GetPipeKey(new int[] { pos.x, pos.y, PipeBuilder.currentLayer })]} \n {pipeType} \n {pipes[GetPipeKey(new int[] { pos.x, pos.y, PipeBuilder.currentLayer })] == pipeType}");
                if(pipes[GetPipeKey(new int[] { pos.x, pos.y, PipeBuilder.currentLayer })] == pipeType) {
                    placeable = true;
                    Debug.Log("3. true");
                    break;
                }
            }

            foreach (var adj in adjHelper)
            {
                if(Exists(new int[] { adj.x, adj.y, PipeBuilder.currentLayer })) {
                    Debug.Log("2.1");
                    if(pipes[GetPipeKey(new int[] { adj.x, adj.y, PipeBuilder.currentLayer })] == pipeType) {
                        placeable = true;
                        Debug.Log("4. true");
                        break;
                    }
                }
            }
        }

        if(Exists(new int[] { position.x, position.y, PipeBuilder.currentLayer})) {
            if(pipes[GetPipeKey(new int[] { position.x, position.y, PipeBuilder.currentLayer})] != tiles[selectedPipe].name) {
                return;
            }
        }
            if(!placeable) {
                return;
            };

        tilemap[PipeBuilder.currentLayer].SetTile(position, tiles[selectedPipe]);
        PipeHelper.AddPipe(new int[] { position.x, position.y, PipeBuilder.currentLayer }, tiles[selectedPipe].name);
        PipeHelper.Test();
    }

    public static void Remove(Vector3Int position)
    {
        List<int[]> keys = new List<int[]>();
        if (pipes.Count == 0) return;
        if (Exists(new int[] { position.x, position.y, PipeBuilder.currentLayer }))
        {
            tempTileType = tilemap[PipeBuilder.currentLayer].GetTile(position).name;
            tilemap[PipeBuilder.currentLayer].SetTile(position, null);
            pipes.Remove(GetPipeKey(new int[] { position.x, position.y, PipeBuilder.currentLayer }));
        }
        Check();
        foreach (var key in pipes.Keys.ToList())
        {
            bool isPartOfRoute = false;
            foreach (var route in routes)
            {
                if (route.Any(r => r[0] == key[0] && r[1] == key[1] && r[2] == key[2]))
                {
                    isPartOfRoute = true;
                    break;
                }
            }

            if (!isPartOfRoute)
            {
                tilemap[key[2]].SetTile(new Vector3Int(key[0], key[1], 0), null);
                pipes.Remove(GetPipeKey(new int[] { key[0], key[1], key[2] }));
                PlaceElements();
            }
        }
        Check();
    }

    public static int[] GetPipeKey(int[] key)
    {
        return pipes.Keys.First(k =>
            k[0] == key[0] && k[1] == key[1] && k[2] == key[2]);
    }

    public static void getValidTiles()
    {
        tmpTilemap.ClearAllTiles();
        tempPipes.Clear();
        foreach (var route in routes)
        {
            var pipeType = tiles[selectedPipe];
            var ends = new List<int[]>();
            ends = GetRouteEndPositions(route);
            foreach (var pos in route) {

            }
            foreach (var pos in route)
            {
                tilemap[PipeBuilder.currentLayer].CompressBounds();
                BoundsInt uiBounds = tilemap[PipeBuilder.currentLayer].cellBounds;
                TileBase[] allTiles = tilemap[PipeBuilder.currentLayer].GetTilesBlock(uiBounds);
                if (PipeBuilder.currentLayer != pos[2]
                && tilemap[PipeBuilder.currentLayer].GetTile(new Vector3Int(pos[0], pos[1], pos[2])) == null
                && ends.Any(e => e.SequenceEqual(pos)))
                {
                    if (!Exists(new int[] { pos[0], pos[1], (pos[2] == 0 ? 1 : 0) }))
                    {
                        tmpTilemap.SetTile(new Vector3Int(pos[0], pos[1], PipeBuilder.currentLayer), tmpTiles[0]);
                        AddTemp(new int[] { pos[0], pos[1], PipeBuilder.currentLayer }, "temp");
                    }
                }
                var adjacentPositions = new List<int[]>();

                adjacentPositions.Add(new int[] { pos[0] + 1, pos[1], pos[2] });
                adjacentPositions.Add(new int[] { pos[0] - 1, pos[1], pos[2] });
                adjacentPositions.Add(new int[] { pos[0], pos[1] + 1, pos[2] });
                adjacentPositions.Add(new int[] { pos[0], pos[1] - 1, pos[2] });

                foreach (var adjacentPos in adjacentPositions)
                {
                    var tilePosition = new Vector3Int(adjacentPos[0], adjacentPos[1], adjacentPos[2]);
                    if (Exists(adjacentPos) ||
                    pos[2] != PipeBuilder.currentLayer ||
                    tilemap[PipeBuilder.currentLayer].GetTile(tilePosition) != null)
                        continue;
                    if (tilemap[PipeBuilder.currentLayer].GetTile(tilePosition) != null ||
                    (PipeBuilder.currentLayer == 0 && talajTilemap.GetTile(tilePosition).name == "VizA") ||
                    (buzaTilemap.GetTile(new Vector3Int(tilePosition[0], tilePosition[1], 0)) != null &&
                    buzaTilemap.GetTile(new Vector3Int(tilePosition[0], tilePosition[1], 0)).name == "pendroid_assets_326" && PipeBuilder.currentLayer == 0))
                        continue;
                    tmpTilemap.SetTile(new Vector3Int(tilePosition[0], tilePosition[1], PipeBuilder.currentLayer), tmpTiles[0]);
                    AddTemp(new int[] { tilePosition[0], tilePosition[1], PipeBuilder.currentLayer }, "temp");
                }
            }
        }
    }

    public static bool Exists(int[] key)
    {
        return pipes.Keys.Any(existingKey =>
        existingKey.Length == key.Length &&
        existingKey[0] == key[0] &&
        existingKey[1] == key[1] &&
        existingKey[2] == key[2]);
    }

    public static void AddTemp(int[] key, string value)
    {
        tempPipes.Add(new Dictionary<int[], string> { { key, value } });
    }

    private static bool canBePlaced(int[] pos)
    {
        foreach (var startPointEntry in startPoints)
        {
            if (startPointEntry.Value.Any(startPos =>
                startPos[0] == pos[0] &&
                startPos[1] == pos[1]))
            {
                return true;
            }
        }

        if (tmpTilemap.GetTile(new Vector3Int(pos[0], pos[1], pos[2])) != null)
        {
            return true;
        }


        return false;
    }

    public static void AddPipe(int[] key, string value)
    {
        if (Exists(key))
        {
            // Debug.Log($"Pipe already exists at {key[0]}, {key[1]}, {key[2]}");
            int[] existingKey = pipes.Keys.First(k =>
                k[0] == key[0] && k[1] == key[1] && k[2] == key[2]);
            pipes[existingKey] = value;
            Check();

            return;
        }

        pipes.Add(key, value);
        // Debug.Log($"{value} placed at {key[0]}, {key[1]}, {key[2]}");

        Check();

        //Debug.Log(tiles[selectedPipe].name);

        if (tiles[selectedPipe].name == "electric")
        {
            bool hasBuzaNearby = Enumerable.Range(-1, 3)
                .SelectMany(x => Enumerable.Range(1, 3)
                    .Select(y => new Vector3Int(key[0] + x, key[1] + y, 0)))
                .Any(pos => buzaTilemap.GetTile(pos) == buzatiles[0]);

            if ((hasBuzaNearby || GetBuzaTile(0, key, 0) || GetBuzaTile(0, key, 1)) && buzaTilemap.GetTile(new Vector3Int(key[0], key[1], 0)) != null)
            {
                if (key[2] == 0)
                {
                    buzaTilemap.SetTile(new Vector3Int(key[0], key[1], 0), null);
                    SorroundPlaceBuza(key);
                }
                if (key[2] == 1)
                {
                    buzaTilemap.SetTile(new Vector3Int(key[0], key[1], 0), buzatiles[1]);
                    SorroundPlaceBuza(key);
                }
            }
        }

        if (talajTilemap.GetTile(new Vector3Int(key[0], key[1], 0)).name == "VizA" || (talajTilemap.GetTile(new Vector3Int(key[0], key[1], 0)).name == "Bridge" && key[2] == 1))
            SorroundGetHal(key);
    }

    public static bool GetBuzaTile(int layer, int[] key, int buzaType)
    {
        return buzaTilemap.GetTile(new Vector3Int(key[0], key[1], layer)) == buzatiles[buzaType];
    }

    public static void SorroundGetHal(int[] key)
    {
        for (int x = -4; x <= 4; x++)
        {
            for (int y = -4; y <= 4; y++)
            {
                if (x == 0 && y == 0) continue;
                var surroundPos = new Vector3Int(key[0] + x, key[1] + y, 0);
                var surroundTile = buzaTilemap.GetTile(surroundPos);

                if (surroundTile != null && surroundTile.name == "pendroid_assets_207")
                {
                    buzaTilemap.SetTile(surroundPos, utilities[2].tile);
                }
            }
        }
    }

    public static void SorroundPlaceBuza(int[] key)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                var surroundPos = new Vector3Int(key[0] + x, key[1] + y, 0);
                var surroundTile = buzaTilemap.GetTile(surroundPos);
                if (surroundTile != null && surroundTile == buzatiles[0])
                {
                    buzaTilemap.SetTile(surroundPos, buzatiles[1]);
                }
            }
        }
    }

    public static void Check()
    {
        var pipeTypes = pipes.Values.Distinct().ToList();
        // Debug.Log($"Pipe types: {string.Join(", ", pipeTypes)}");
        GetAllRoutes();
        bool isLevelNotCompleted = false;
        foreach (var pipeType in endPoints.Keys)
        {
            Debug.Log($"Pipe type: {pipeType}");
            //PrintPipeRoute(startPoints[pipeType][0], pipeType)
            // var startPos = startPoints[pipeType][0];
            foreach (var startPos in startPoints[pipeType])
            {
            Debug.Log($"Pipe type: {pipeType} | startPos: {startPos[0]}, {startPos[1]}, {startPos[2]}");

                if(routes.Where(r => r.Any(pos => pos[0] == startPos[0] && pos[1] == startPos[1] && pos[2] == startPos[2])).Count() == 0) {
                    Debug.Log($"No route found for {pipeType}");
                    isLevelNotCompleted = true;
                    break;
                }
                foreach (var route in routes.Where(r => r.Any(pos => pos[0] == startPos[0] && pos[1] == startPos[1] && pos[2] == startPos[2])))
                {
                    foreach (var endPoint in endPoints[pipeType])
                    {
                        // Debug.Log($"Is completed: {isCompleted(route, endPoint, pipeType)} {pipeType}");
                        if (isCompleted(route, endPoint, pipeType))
                        {
                            Debug.Log($"Route completed: {route.Count} {pipeType}");
                        }
                        else
                        {
                            Debug.Log($"Route not completed: {route.Count} {pipeType}");
                            isLevelNotCompleted = true;
                        }
                    }
                }
            }


        }

                    if (!isLevelNotCompleted)
            {
                Debug.Log($"Level completed");
            }
            else
            {
                Debug.Log($"Level not completed");
            }
        // Debug.Log("Checking completed");
        getValidTiles();
    }

    private static bool CanConnect(string pipe1, string pipe2)
    {
        if (pipe1 == pipe2) return true;
        return false;
    }

    void Update()
    {

    }

    public static void GetAllRoutes()
    {
        routes.Clear();
        var route = new List<int[]>();
        foreach (var startingPoint in startPoints)
        {
            string pipeType = startingPoint.Key;
            foreach (var startPos in startingPoint.Value)
            {
                route = GetPipeRoute(startPos, pipeType);
                if (route != null) routes.Add(route);
            }
        }
        Debug.Log($"Found {routes.Count} routes");
    }

    public static List<int[]> GetPipeRoute(int[] startPos, string pipeType)
    {
        HashSet<string> visited = new HashSet<string>();
        var route = new List<int[]>();
        route = TraceRoute(startPos, visited, route, pipeType);
        return route;
    }

    public static List<int[]> GetRouteEndPositions(List<int[]> route)
    {
        List<int[]> endPositions = new List<int[]>();

        foreach (var pos in route)
        {
            int connectionCount = 0;
            bool hasValidConnection = false;

            var adjacentPositions = new List<int[]>();

            adjacentPositions.Add(new int[] { pos[0] + 1, pos[1], pos[2] });
            adjacentPositions.Add(new int[] { pos[0] - 1, pos[1], pos[2] });
            adjacentPositions.Add(new int[] { pos[0], pos[1] + 1, pos[2] });
            adjacentPositions.Add(new int[] { pos[0], pos[1] - 1, pos[2] });

            var allLayers = pipes.Select(p => p.Key[2]).Distinct().ToList();
            foreach (var layer in allLayers)
            {
                if (layer != pos[2])
                {
                    adjacentPositions.Add(new int[] { pos[0], pos[1], layer });
                }
            }

            foreach (var neighborPos in adjacentPositions)
            {
                if (Exists(neighborPos))
                {
                    int[] neighborPipeKey = GetPipeKey(neighborPos);
                    if (route.Any(r => r.SequenceEqual(neighborPos)) &&
                        CanConnect(pipes[GetPipeKey(pos)], pipes[neighborPipeKey]))
                    {
                        connectionCount++;
                        if (connectionCount > 1)
                        {
                            hasValidConnection = true;
                            break;
                        }
                    }
                }
            }

            if (!hasValidConnection)
            {
                endPositions.Add(pos);
            }
        }

        return endPositions;
    }
    private static List<int[]> TraceRoute(int[] currentPos, HashSet<string> visited, List<int[]> route, string pipeType)
    {
        string posKey = $"{currentPos[0]},{currentPos[1]},{currentPos[2]}";
        if (visited.Contains(posKey) || !Exists(currentPos))
            return route;

        visited.Add(posKey);
        route.Add(currentPos);

        var adjacentPositions = new List<int[]>();

        adjacentPositions.Add(new int[] { currentPos[0] + 1, currentPos[1], currentPos[2] });  // Right
        adjacentPositions.Add(new int[] { currentPos[0] - 1, currentPos[1], currentPos[2] });  // Left
        adjacentPositions.Add(new int[] { currentPos[0], currentPos[1] + 1, currentPos[2] });  // Up
        adjacentPositions.Add(new int[] { currentPos[0], currentPos[1] - 1, currentPos[2] });  // Down

        var allLayers = pipes.Select(p => p.Key[2]).Distinct().ToList();
        foreach (var layer in allLayers)
        {
            if (layer != currentPos[2])
            {
                adjacentPositions.Add(new int[] { currentPos[0], currentPos[1], layer });
            }
        }

        foreach (var neighborPos in adjacentPositions)
        {
            if (Exists(neighborPos))
            {
                string neighborKey = $"{neighborPos[0]},{neighborPos[1]},{neighborPos[2]}";
                if (!visited.Contains(neighborKey))
                {
                    int[] neighborPipeKey = GetPipeKey(neighborPos);

                    if (CanConnect(pipes[GetPipeKey(currentPos)], pipes[neighborPipeKey]) &&
                        pipes[neighborPipeKey] == pipeType)
                    {
                        TraceRoute(neighborPos, visited, route, pipeType);
                    }
                }
            }
        }
        return route;
    }

    public static bool isCompleted(List<int[]> route, int[] endPoint, string pipeType)
    {
        // Log($"endpoints: {endPoints[pipeType].Count}");
        if (route.Any(pos => pos[0] == endPoint[0] && pos[1] == endPoint[1] && pos[2] == endPoint[2]))
        {
            return true;
        }
        return false;
    }

    public static void PrintPipeRoute(int[] startPos, string pipeType)
    {
        // Debug.Log($"Found route with {route.Count} connected pipes:");
        Debug.Log("--------------------------------");
        foreach (var route in routes.Where(r => r.Any(pos => pos[0] == startPos[0] && pos[1] == startPos[1] && pos[2] == startPos[2])))
        {
            foreach (var endPoint in endPoints[pipeType])
            {
                Debug.Log($"Is completed: {isCompleted(route, endPoint, pipeType)} {pipeType}");
            }
        }
    }
}




//a ház -9 -4 0
// hegy -3 1 0
