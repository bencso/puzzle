using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class UIDetection : MonoBehaviour
{
    [SerializeField]
    public GameObject UIManager;
    public GameObject menu;
    public float tapMaxTime = 0.2f;
    public float tapMaxMovement = 20f;
    protected Vector3Int tilePosition;
    protected List<Vector3Int> UICellPositions = new List<Vector3Int>();
    protected List<Vector3Int> menuCellPositions = new List<Vector3Int>();
    private float touchStartTime;
    private Vector2 touchStartPosition;
    private Tilemap tilemaps;
    private Tilemap menuTilemaps;

    void Start()
    {
        UICellPositions.Clear();
        GetPosition();
    }

    void GetPosition()
    {
        if (UIManager != null)
        {
            tilemaps = UIManager.GetComponent<Tilemap>();
            if (tilemaps != null)
            {
                tilemaps.CompressBounds();
                BoundsInt uiBounds = tilemaps.cellBounds;
                TileBase[] allTiles = tilemaps.GetTilesBlock(uiBounds);

                for (int x = 0; x < uiBounds.size.x; x++)
                {
                    for (int y = 0; y < uiBounds.size.y; y++)
                    {
                        TileBase tile = allTiles[x + y * uiBounds.size.x];
                        if (tile != null)
                        {
                            Vector3Int cellPos = new Vector3Int(x + uiBounds.x, y + uiBounds.y, 0);
                            UICellPositions.Add(cellPos);
                        }
                    }
                }
            }
        }

        if (menu != null)
        {
            menuTilemaps = menu.GetComponent<Tilemap>();
            if (menuTilemaps != null)
            {
                menuTilemaps.CompressBounds();
                BoundsInt uiBounds = menuTilemaps.cellBounds;
                TileBase[] allTiles = menuTilemaps.GetTilesBlock(uiBounds);

                for (int x = 0; x < uiBounds.size.x; x++)
                {
                    for (int y = 0; y < uiBounds.size.y; y++)
                    {
                        TileBase tile = allTiles[x + y * uiBounds.size.x];
                        if (tile != null)
                        {
                            Vector3Int cellPos = new Vector3Int(x + uiBounds.x, y + uiBounds.y, 0);
                            menuCellPositions.Add(cellPos);
                        }
                    }
                }
            }
        }
    }

    public bool IsTapOnUI(Vector2 worldPosition)
    {
        if (tilemaps == null) return false;

        Vector3Int tapCellPosition = tilemaps.WorldToCell(worldPosition);

        foreach (Vector3Int uiCellPos in UICellPositions)
        {
            if (uiCellPos == tapCellPosition)
            {
                return true;
            }
        }

        if (menu != null && OpenMenu.isMenuOpened)
        {
            Vector3Int tapMenuCellPosition = menuTilemaps.WorldToCell(worldPosition);

            foreach (Vector3Int menuCellPos in menuCellPositions)
            {
                if (menuCellPos == tapMenuCellPosition)
                {
                    return true;
                }
            }
        }
        return false;
    }
}