using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 2.0f;
    private Vector2 touchStart;
    private bool isDragging = false;
    public GameObject UIManager;
    public GameObject Background;
    public GameObject menu;
    private Tilemap backgroundTilemap;
    private Camera mainCamera;
    private Vector3 minBounds;
    private Vector3 maxBounds;
    protected Vector3Int tilePosition;

    void Start()
    {
        backgroundTilemap = Background.GetComponent<Tilemap>();
        mainCamera = GetComponent<Camera>();
        menu.SetActive(false);
        CalculateBounds();
        // AdjustCamera();
    }

    // private float minZoom = 2f;
    // private float maxZoom = 8f;
    // private float currentZoom;

    // void AdjustCamera()
    // {
    //     if (backgroundTilemap == null || mainCamera == null) return;

    //     float screenRatio = (float)Screen.width / Screen.height;
    //     bool isTablet = screenRatio < 1.6f;

    //     if (isTablet)
    //     {
    //         BoundsInt bounds = backgroundTilemap.cellBounds;
    //         Vector3 tilemapSize = backgroundTilemap.CellToWorld(new Vector3Int(bounds.xMax, bounds.yMax, 0)) -
    //             backgroundTilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));
    //         float tilemapRatio = tilemapSize.x / tilemapSize.y;
    //         float zoomByHeight = tilemapSize.y / 2;
    //         float zoomByWidth = (tilemapSize.x / screenRatio) / 2;
    //         currentZoom = Mathf.Max(zoomByHeight, zoomByWidth);
    //         currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    //         mainCamera.orthographicSize = currentZoom;
    //     }
    // }

    void CalculateBounds()
    {
        if (backgroundTilemap != null)
        {
            // Tilemap határainak kiszámítása
            backgroundTilemap.CompressBounds();
            BoundsInt bounds = backgroundTilemap.cellBounds;
            minBounds = backgroundTilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));
            maxBounds = backgroundTilemap.CellToWorld(new Vector3Int(bounds.xMax, bounds.yMax, 0));
        }
    }

    Vector3 ClampCamera(Vector3 targetPosition)
    {
        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        float minX = minBounds.x + cameraWidth;
        float maxX = maxBounds.x - cameraWidth;
        float minY = minBounds.y + cameraHeight;
        float maxY = maxBounds.y - cameraHeight;
        float newX = Mathf.Clamp(targetPosition.x, minX, maxX);
        float newY = Mathf.Clamp(targetPosition.y, minY, maxY);
        return new Vector3(newX, newY, targetPosition.z);
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStart = touch.position;
                    isDragging = true;
                    break;
                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        Vector2 delta = touch.position - touchStart;
                        Vector3 move = new Vector3(-delta.x, -delta.y, 0) * moveSpeed * Time.deltaTime;
                        // Új pozíció kiszámítása és korlátozása
                        Vector3 newPosition = transform.position + move;
                        Vector3 clampedPosition = ClampCamera(newPosition);
                        // A különbség kiszámítása a korlátozás után
                        Vector3 actualMove = clampedPosition - transform.position;
                        // Kamera és UI mozgatása
                        transform.position = clampedPosition;
                        if (UIManager != null)
                        {
                            UIManager.transform.position += actualMove;
                            menu.transform.position += actualMove;
                        }
                        touchStart = touch.position;
                    }
                    break;
                case TouchPhase.Ended:
                    isDragging = false;
                    break;
            }
        }
    }
}