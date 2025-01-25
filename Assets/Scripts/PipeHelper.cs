using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;

public class PipeHelper : MonoBehaviour
{
    [SerializeField]
    private Tilemap tilemapHelper;

    [SerializeField]
    private RuleTile[] tilesHelper;
    public static Tilemap tilemap;
    public static RuleTile[] tiles;
    public static Dictionary<int[], string> pipes = new Dictionary<int[], string>();
    public static List<Dictionary<int[], string>> tempPipes = new List<Dictionary<int[], string>>();

    protected int selectedPipe = 0;

    private static Dictionary<string, List<int[]>> startPoints = new Dictionary<string, List<int[]>>();
    private static Dictionary<string, List<int[]>> endPoints = new Dictionary<string, List<int[]>>();

    void Start()
    {
        tilemap = tilemapHelper;
        tiles = tilesHelper;
        startPoints.Add("electric", new List<int[]> { new int[] { -9, -4, 0 } });
        startPoints.Add("water", new List<int[]> { new int[] { 0, -3, 0 } });
        endPoints.Add("electric", new List<int[]> { new int[] { -3, 1, 0 } });
        endPoints.Add("water", new List<int[]> { new int[] { 0, 1, 0 } });
    }

    public static void Test()
    {
        if (pipes != null && pipes.Count > 0) Debug.Log(pipes.Count);
    }

    public static void Place(Vector3Int position)
    {
        int selectedPipe = PipeBuilder.selectedPipe;
        tilemap.SetTile(position, tiles[selectedPipe]);
        PipeHelper.AddPipe(new int[] { position.x, position.y, PipeBuilder.currentLayer }, tiles[selectedPipe].name);
        PipeHelper.Test();
    }

    public static int[] GetPipeKey(int[] key)
    {
        return pipes.Keys.First(k =>
            k[0] == key[0] && k[1] == key[1] && k[2] == key[2]);
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

    

    public static void AddPipe(int[] key, string value)
    {
        if (Exists(key))
        {
            Debug.Log($"Pipe already exists at {key[0]}, {key[1]}, {key[2]}");
            int[] existingKey = pipes.Keys.First(k =>
                k[0] == key[0] && k[1] == key[1] && k[2] == key[2]);
            pipes[existingKey] = value;
            Check();
            return;
        }
        pipes.Add(key, value);
        Debug.Log($"{value} placed at {key[0]}, {key[1]}, {key[2]}");
        Check();
    }

    public static void Check() 
    {
        var pipeTypes = pipes.Values.Distinct().ToList();
        Debug.Log($"Pipe types: {string.Join(", ", pipeTypes)}");
        foreach (var pipeType in pipeTypes) {
            if(GetPipeRoute(startPoints[pipeType][0], pipeType) != null) PrintPipeRoute(startPoints[pipeType][0], pipeType);
        }
    }


    private static bool CanConnect(string pipe1, string pipe2)
    {
        if (pipe1 == pipe2) return true;  
        return false;
    }

    void Update()
    {

    }

    public static List<int[]> GetPipeRoute(int[] startPos, string pipeType)
    {
        HashSet<string> visited = new HashSet<string>();
        List<int[]> route = new List<int[]>();
        TraceRoute(startPos, visited, route, pipeType);
        return route;
    }

    public static List<int[]> GetRouteEndPositions(int[] startPos, string pipeType)
{
    List<int[]> route = GetPipeRoute(startPos, pipeType);
    List<int[]> endPositions = new List<int[]>();
    
    foreach (var pos in route)
    {
        int connectionCount = 0;
        bool hasValidConnection = false;
        var adjacentPositions = new[]
        {
            new int[] { pos[0] + 1, pos[1], pos[2] },  // Right
            new int[] { pos[0] - 1, pos[1], pos[2] },  // Left
            new int[] { pos[0], pos[1] + 1, pos[2] },  // Up
            new int[] { pos[0], pos[1] - 1, pos[2] }   // Down
        };

        foreach (var neighborPos in adjacentPositions)
        {
            if (Exists(neighborPos))
            {
                int[] neighborPipeKey = GetPipeKey(neighborPos);
                if (route.Any(r => r.SequenceEqual(neighborPos)) && 
                    CanConnect(pipes[GetPipeKey(pos)], pipes[neighborPipeKey]))
                {
                    connectionCount++;
                    if (connectionCount > 1) {
                        hasValidConnection = true;
                        break;
                    };
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

    private static void TraceRoute(int[] currentPos, HashSet<string> visited, List<int[]> route, string pipeType)
    {
        // Create a unique key for the current position
        string posKey = $"{currentPos[0]},{currentPos[1]},{currentPos[2]}";
        
        // If we've already visited this position, return
        if (visited.Contains(posKey))
            return;
        
        // Mark as visited and add to route
        visited.Add(posKey);
        route.Add(currentPos);
        
        // Check all adjacent positions
        var adjacentPositions = new[]
        {
            new int[] { currentPos[0] + 1, currentPos[1], currentPos[2] },  // Right
            new int[] { currentPos[0] - 1, currentPos[1], currentPos[2] },  // Left
            new int[] { currentPos[0], currentPos[1] + 1, currentPos[2] },  // Up
            new int[] { currentPos[0], currentPos[1] - 1, currentPos[2] }   // Down
        };

        foreach (var neighborPos in adjacentPositions)
        {
            if (Exists(neighborPos))
            {
                string neighborKey = $"{neighborPos[0]},{neighborPos[1]},{neighborPos[2]}";
                if (!visited.Contains(neighborKey))
                {
                    int[] neighborPipeKey = GetPipeKey(neighborPos);
                    
                    if (CanConnect(pipes[GetPipeKey(currentPos)], pipes[neighborPipeKey]) && pipes[neighborPipeKey] == pipeType)
                    {
                        TraceRoute(neighborPos, visited, route, pipeType);
                    }
                }
            }
        }
    }


    public static bool isCompleted(int[] startPos, int[] endPos, string pipeType)
    {
        var route = GetPipeRoute(startPos, pipeType);
        var endposes = GetRouteEndPositions(startPos, pipeType);
        return endposes.Find(pos => pos.SequenceEqual(endPos)) != null || route.Find(pos => pos.SequenceEqual(endPos)) != null;
    }

    public static void PrintPipeRoute(int[] startPos, string pipeType)
    {
        List<int[]> route = GetPipeRoute(startPos, pipeType);
        List<int[]> endPositions = GetRouteEndPositions(startPos, pipeType);
        Debug.Log($"Found route with {route.Count} connected pipes:");
        foreach (var pos in route)
        {
            // Debug.Log($"Pipe at ({pos[0]}, {pos[1]}, {pos[2]}) type: {pipes[GetPipeKey(pos)]}");
        }
        Debug.Log("--------------------------------");
        Debug.Log($"Found {endPositions.Count} end positions:");
        foreach (var endPos in endPositions)
        {
            Debug.Log($"Route ends at: ({endPos[0]}, {endPos[1]}, {endPos[2]})");
        }
        Debug.Log("--------------------------------");
        int[] destinationPos = endPoints[pipeType][0];
        Debug.Log($"Is completed: {isCompleted(startPos, destinationPos, pipeType)}");
    }
}

//a ház -9 -4 0
// hegy -3 1 0
