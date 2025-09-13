using UnityEngine;

public class GridLayoutManager : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float paddingPercent = 0.1f;
    
    [Header("Spacing Control")]
    [SerializeField] private Vector2 cardSpacing = new Vector2(0.2f, 0.2f); // Manual spacing control
    [SerializeField] private bool useFixedSpacing = true; // Toggle between fixed and responsive spacing
    [SerializeField] private Vector2 minCardSize = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 maxCardSize = new Vector2(2f, 2f);
    
    [Header("Debug Info")]
    [SerializeField] private Vector2 calculatedCardSize;
    [SerializeField] private Vector2 actualSpacing;
    [SerializeField] private Vector2 gridWorldSize;
    
    public Vector2 CalculateOptimalLayout(Vector2Int gridSize, Transform container)
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        
        Vector2 screenWorldSize = GetScreenWorldSize();
        Vector2 availableArea = screenWorldSize * (1f - paddingPercent * 2f);
        
        Vector2 finalCardSize;
        Vector2 finalSpacing;
        
        if (useFixedSpacing)
        {
            // Use your specified spacing and calculate card size to fit
            finalSpacing = cardSpacing;
            
            // Calculate total space needed for spacing
            Vector2 totalSpacing = new Vector2(
                finalSpacing.x * (gridSize.x - 1),
                finalSpacing.y * (gridSize.y - 1)
            );
            
            // Remaining space for cards
            Vector2 cardArea = availableArea - totalSpacing;
            
            // Calculate card size that fits
            Vector2 cardSize = new Vector2(
                cardArea.x / gridSize.x,
                cardArea.y / gridSize.y
            );
            
            // Make cards square (use smaller dimension)
            float minDimension = Mathf.Min(cardSize.x, cardSize.y);
            finalCardSize = new Vector2(minDimension, minDimension);
            
            // Clamp to min/max limits
            finalCardSize.x = Mathf.Clamp(finalCardSize.x, minCardSize.x, maxCardSize.x);
            finalCardSize.y = Mathf.Clamp(finalCardSize.y, minCardSize.y, maxCardSize.y);
        }
        else
        {
            // Responsive spacing (old behavior)
            Vector2 totalSpacing = new Vector2(
                availableArea.x * 0.02f * (gridSize.x - 1),
                availableArea.y * 0.02f * (gridSize.y - 1)
            );
            
            Vector2 cardArea = availableArea - totalSpacing;
            Vector2 cardSize = new Vector2(cardArea.x / gridSize.x, cardArea.y / gridSize.y);
            
            float minDimension = Mathf.Min(cardSize.x, cardSize.y);
            finalCardSize = new Vector2(minDimension, minDimension);
            
            finalCardSize.x = Mathf.Clamp(finalCardSize.x, minCardSize.x, maxCardSize.x);
            finalCardSize.y = Mathf.Clamp(finalCardSize.y, minCardSize.y, maxCardSize.y);
            
            finalSpacing = new Vector2(
                (availableArea.x - finalCardSize.x * gridSize.x) / Mathf.Max(1, gridSize.x - 1),
                (availableArea.y - finalCardSize.y * gridSize.y) / Mathf.Max(1, gridSize.y - 1)
            );
        }
        
        // Store debug info
        calculatedCardSize = finalCardSize;
        actualSpacing = finalSpacing;
        gridWorldSize = screenWorldSize;
        
        return finalCardSize;
    }
    
    public Vector3 CalculateCardPosition(Vector2Int gridPos, Vector2Int gridSize, Vector2 cardSize, Vector2 spacing)
    {
        // Use actual spacing from calculation
        Vector2 useSpacing = useFixedSpacing ? cardSpacing : actualSpacing;
        
        // Calculate total grid dimensions
        float totalWidth = cardSize.x * gridSize.x + useSpacing.x * (gridSize.x - 1);
        float totalHeight = cardSize.y * gridSize.y + useSpacing.y * (gridSize.y - 1);
        
        // Calculate starting position to center the grid
        float startX = -totalWidth / 2f + cardSize.x / 2f;
        float startY = totalHeight / 2f - cardSize.y / 2f;
        
        // Position this card within the centered grid
        Vector3 cardPosition = new Vector3(
            startX + gridPos.x * (cardSize.x + useSpacing.x),
            startY - gridPos.y * (cardSize.y + useSpacing.y),
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
        return useFixedSpacing ? cardSpacing : actualSpacing;
    }
    
    // Public method to change spacing at runtime
    public void SetCardSpacing(Vector2 newSpacing)
    {
        cardSpacing = newSpacing;
        
        // Trigger GameManager to recreate grid if needed
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
    
    // Public method to toggle spacing mode
    public void SetUseFixedSpacing(bool useFixed)
    {
        useFixedSpacing = useFixed;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}
