using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections.Generic;


public class PipeBuilder : MonoBehaviour
{
    public static Tilemap[] layers;
    private Tilemap[] tilemap;
    private RuleTile[] tiles;

    [SerializeField]
    private Tilemap[] layersHelper;
    static public int currentLayer = 0;
    public Tilemap selectNyil;
    public float tapMaxTime = 0.2f;
    public float tapMaxMovement = 20f;
    public static int selectedPipe = 0;
    public Tilemap tileSelection;
    public UIDetection UIDetection;
    private float touchStartTime;
    private Vector2 touchStartPosition;
    protected List<GameObject> UIPipes = new List<GameObject>();
    protected Tile selectionTile;
    protected Tile right;
    protected Tile left;

    void Start()
    {
        tilemap = PipeHelper.tilemap;
        tiles = PipeHelper.tiles;
        beforeSerialize();
        if (tileSelection != null)
        {
            BoundsInt bounds = tileSelection.cellBounds;
            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                selectionTile = (Tile)tileSelection.GetTile(pos);
                if (selectionTile != null)
                {
                    tileSelection.SetTile(pos, tiles[0]);
                }
            }
            BoundsInt boundsNyil = selectNyil.cellBounds;
            foreach (Vector3Int pos in boundsNyil.allPositionsWithin)
            {
                TileBase tile = selectNyil.GetTile(pos);
                if (tile != null)
                {
                    if (tile.name == "balnyil")
                    {
                        left = (Tile)tile;
                    }
                    else if (tile.name == "jobbnyil")
                    {
                        right = (Tile)tile;
                    }
                }
            }
        }
    }

    public void beforeSerialize()
    {
        layers = layersHelper;
    }

    void SelectPipe(int index)
    {
        // Ensure index stays between 0 and the last valid array index
        // Example: if tiles array has 4 elements, valid indices are 0,1,2,3
        selectedPipe = Mathf.Clamp(index, 0, tiles.Length - 1);
        RenderSelectedPipe();
    }
    void RenderSelectedPipe()
    {
        BoundsInt bounds = tileSelection.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = tileSelection.GetTile(pos);
            if (tile != null)
            {
                var ruleTile = tiles[selectedPipe];
                if (ruleTile != null)
                {
                    var tempTile = ScriptableObject.CreateInstance<Tile>();
                    tempTile.sprite = ruleTile.m_DefaultSprite;
                    tileSelection.SetTile(pos, tempTile);
                }
            }
        }
    }

    static public void SetLayer(int layer)
    {
        TilemapRenderer renderer = layers[currentLayer].GetComponent<TilemapRenderer>();
        layers[currentLayer].color = new Color(1, 1, 1, .5f);
        renderer.sortingOrder = 2;
        currentLayer = layer;
        layers[currentLayer].color = new Color(1, 1, 1, 1);
        renderer = layers[currentLayer].GetComponent<TilemapRenderer>();
        renderer.sortingOrder = 3;
    }
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartTime = Time.time;
                touchStartPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                float touchDuration = Time.time - touchStartTime;
                float touchMovement = Vector2.Distance(touchStartPosition, touch.position);

                if (touchDuration < tapMaxTime && touchMovement < tapMaxMovement)
                {
                    Vector2 position = Camera.main.ScreenToWorldPoint(touch.position);
                    if (!UIDetection.IsTapOnUI(position) && OpenMenu.isMenuOpened)
                    {
                        Vector3Int cellPosition = tilemap[currentLayer].WorldToCell(position);
                        if (selectedPipe == 3)
                            PipeHelper.Remove(cellPosition);
                        else
                            PipeHelper.Place(cellPosition);
                    }
                    else
                    {
                        TileBase clickedTile = selectNyil.GetTile(selectNyil.WorldToCell(position));
                        if (clickedTile == left)
                        {
                            if (selectedPipe == 0) SelectPipe(tiles.Length - 1);
                            else SelectPipe(selectedPipe -= 1);
                            PipeHelper.getValidTiles();
                        }
                        else if (clickedTile == right)
                        {
                            if (selectedPipe == tiles.Length - 1) SelectPipe(0);
                            else SelectPipe(selectedPipe += 1);
                            PipeHelper.getValidTiles();
                        }
                    }
                }
            }
        }
    }
}
