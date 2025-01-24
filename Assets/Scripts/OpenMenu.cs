using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class OpenMenu : MonoBehaviour
{
    [SerializeField]
    public GameObject UIManager;
    public GameObject menu;
    public float tapMaxTime = 0.2f;
    public float tapMaxMovement = 20f;
    protected Vector3Int tilePosition;
    private float touchStartTime;
    private Vector2 touchStartPosition;
    private Tilemap tilemaps;
    static public bool isMenuOpened = false;

    void Start()
    {
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
                        //TODO: Átírni a nevet a megfelelőre
                        if (tile != null && tile.name == "pendroid_assets_253")
                        {
                            tilePosition = new Vector3Int(x + uiBounds.x, y + uiBounds.y, 0);
                        }
                    }
                }
            }
        }
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
                    Vector2 worldPoint = Camera.main.ScreenToWorldPoint(touch.position);
                    Vector3Int touchCell = tilemaps.WorldToCell(worldPoint);

                    if (touchCell == tilePosition)
                    {
                        isMenuOpened = !isMenuOpened;
                        menu.SetActive(isMenuOpened);
                    }
                }
            }
        }
    }
}