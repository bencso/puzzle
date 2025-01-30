using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
public class Layer : MonoBehaviour
{
    public Tilemap tilemap;
    public float tapMaxTime = 0.2f;
    public float tapMaxMovement = 20f;
    protected Tile up;
    protected Tile down;
    private Vector2 touchStartPosition;
    private float touchStartTime;
    public TMP_Text text;
    void Start()
    {
        if (tilemap != null)
        {
            BoundsInt boundsNyil = tilemap.cellBounds;
            foreach (Vector3Int pos in boundsNyil.allPositionsWithin)
            {
                TileBase tile = tilemap.GetTile(pos);
                if (tile != null)
                {
                    if (tile.name == "fellayer") up = (Tile)tile;
                    else if (tile.name == "lelayer") down = (Tile)tile;
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
                    Vector2 position = Camera.main.ScreenToWorldPoint(touch.position);
                    TileBase clickedTile = tilemap.GetTile(tilemap.WorldToCell(position));
                    if (clickedTile == up) {
                        PipeBuilder.SetLayer(0);
                        text.text = "0";
                        PipeHelper.tmpTilemap.ClearAllTiles();
                        PipeHelper.Check();
                        }
                    else if (clickedTile == down){
                        PipeBuilder.SetLayer(1);
                        text.text = "-1";
                        PipeHelper.tmpTilemap.ClearAllTiles();
                        PipeHelper.Check();
                    }
                }
            }
        }
    }
}
