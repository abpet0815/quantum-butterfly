using UnityEngine;

public class GridLayoutManager : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float paddingPercent = 0.1f;
    [SerializeField] private float fixedSpacing = 0.1f; // Fixed spacing instead of responsive
    [SerializeField] private Vector2 minCardSize = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 maxCardSize = new Vector2(2f, 2f);
    
    [Header("Debug Info")]
    [SerializeField] private Vector2 calculatedCardSize;
    [SerializeField] private Vector2 calculatedSpacing;
    [SerializeField] private Vector2 gridWorldSize;
    
    public Vector2 CalculateOptimalLayout(Vector2Int gridSize, Transform container)
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        
        // Get screen bounds in world coordinates
        Vector2 screenWorldSize = GetScreenWorldSize();
        
        // Apply padding
        Vector2 availableArea = screenWorldSize * (1f - paddingPercent * 2f);
        
        // Calculate card size based on available area but don't spread spacing
        Vector2 cardSize = new Vector2(
            availableArea.x / (gridSize.x + 1), // +1 for some margin
            availableArea.y / (gridSize.y + 1)
        );
        
        // Ensure cards are square (use the smaller dimension)
        float minDimension = Mathf.Min(cardSize.x, cardSize.y);
        cardSize = new Vector2(minDimension, minDimension);
        
        // Clamp card size to min/max limits
        cardSize.x = Mathf.Clamp(cardSize.x, minCardSize.x, maxCardSize.x);
        cardSize.y = Mathf.Clamp(cardSize.y, minCardSize.y, maxCardSize.y);
        
        // Use fixed spacing instead of responsive spacing
        calculatedCardSize = cardSize;
        calculatedSpacing = new Vector2(fixedSpacing, fixedSpacing);
        gridWorldSize = screenWorldSize;
        
        return cardSize;
    }
    
    public Vector3 CalculateCardPosition(Vector2Int gridPos, Vector2Int gridSize, Vector2 cardSize, Vector2 spacing)
    {
        // Calculate total grid size with fixed spacing
        float totalWidth = cardSize.x * gridSize.x + fixedSpacing * (gridSize.x - 1);
        float totalHeight = cardSize.y * gridSize.y + fixedSpacing * (gridSize.y - 1);
        
        // Calculate starting position to center the grid
        float startX = -totalWidth / 2f + cardSize.x / 2f;
        float startY = totalHeight / 2f - cardSize.y / 2f;
        
        // Position this card within the centered grid
        Vector3 cardPosition = new Vector3(
            startX + gridPos.x * (cardSize.x + fixedSpacing),
            startY - gridPos.y * (cardSize.y + fixedSpacing),
            0
        );
        
        return cardPosition;
    }
    
    private Vector2 GetScreenWorldSize()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        
        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;
        
        return new Vector2(cameraWidth, cameraHeight);
    }
    
    public Vector2 GetCalculatedSpacing()
    {
        return new Vector2(fixedSpacing, fixedSpacing);
    }
}
