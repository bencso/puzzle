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

    void Start()
    {
        tilemap = tilemapHelper;
        tiles = tilesHelper;
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
            return;
        }
        pipes.Add(key, value);
        Debug.Log($"{value} placed at {key[0]}, {key[1]}, {key[2]}");
    }

    void Update()
    {

    }
}
