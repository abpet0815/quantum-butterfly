using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(4, 4);
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardContainer;
    
    [Header("Layout")]
    [SerializeField] private GridLayoutManager gridLayoutManager;
    [SerializeField] private bool useResponsiveLayout = true;
    [SerializeField] private bool autoScaleCards = false;
    [SerializeField] private float fallbackCardSpacing = 1.2f;
    
    [Header("Game Rules")]
    [SerializeField] private float matchCheckDelay = 1f;
    [SerializeField] private int maxFlippedCards = 2;
    
    [Header("Managers")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private JsonSaveLoadManager jsonSaveManager;
    
    private List<Card> allCards = new List<Card>();
    private List<Card> flippedCards = new List<Card>();
    private int matchedPairs = 0;
    private int totalPairs;
    private bool isCheckingMatch = false;
    
    [Header("Debug")]
    [SerializeField] private List<Sprite> cardSprites = new List<Sprite>();
    
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Create GridLayoutManager if not assigned
            if (gridLayoutManager == null)
            {
                GameObject layoutManagerObj = new GameObject("GridLayoutManager");
                layoutManagerObj.transform.SetParent(transform);
                gridLayoutManager = layoutManagerObj.AddComponent<GridLayoutManager>();
            }
            
            // Create ScoreManager if not assigned
            if (scoreManager == null)
            {
                GameObject scoreManagerObj = new GameObject("ScoreManager");
                scoreManagerObj.transform.SetParent(transform);
                scoreManager = scoreManagerObj.AddComponent<ScoreManager>();
            }
            
            // Create JsonSaveLoadManager if not assigned
            if (jsonSaveManager == null)
            {
                GameObject saveManagerObj = new GameObject("JsonSaveLoadManager");
                saveManagerObj.transform.SetParent(transform);
                jsonSaveManager = saveManagerObj.AddComponent<JsonSaveLoadManager>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Only create grid if no save data was loaded
        if (!jsonSaveManager.HasSaveFile())
        {
            CreateGrid();
            if (scoreManager != null)
            {
                scoreManager.StartNewGame();
            }
        }
    }
    
    private void CreateGrid()
    {
        // Clear existing cards
        foreach (Card card in allCards)
        {
            if (card != null) DestroyImmediate(card.gameObject);
        }
        allCards.Clear();
        flippedCards.Clear();
        matchedPairs = 0;
        
        // Ensure even number of cards for pairs
        int totalCards = gridSize.x * gridSize.y;
        if (totalCards % 2 != 0)
        {
            Debug.LogWarning($"Grid size {gridSize.x}x{gridSize.y} creates odd number of cards ({totalCards}). Adjusting...");
            if (gridSize.x > 1) gridSize.x -= 1;
            else if (gridSize.y > 1) gridSize.y -= 1;
            totalCards = gridSize.x * gridSize.y;
        }
        
        totalPairs = totalCards / 2;
        
        // Calculate responsive layout
        Vector2 cardSize = Vector2.one;
        Vector2 spacing = Vector2.one * fallbackCardSpacing;
        
        if (useResponsiveLayout && gridLayoutManager != null)
        {
            cardSize = gridLayoutManager.CalculateOptimalLayout(gridSize, cardContainer);
            spacing = gridLayoutManager.GetCalculatedSpacing();
        }
        
        // Create card values (pairs)
        List<int> cardValues = new List<int>();
        for (int i = 0; i < totalPairs; i++)
        {
            cardValues.Add(i);
            cardValues.Add(i); // Add pair
        }
        
        // Shuffle the values
        ShuffleList(cardValues);
        
        // Create cards in grid
        int cardIndex = 0;
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                Vector3 position;
                
                if (useResponsiveLayout && gridLayoutManager != null)
                {
                    position = gridLayoutManager.CalculateCardPosition(
                        new Vector2Int(x, y), gridSize, cardSize, spacing);
                }
                else
                {
                    // Fallback positioning
                    position = new Vector3(
                        x * fallbackCardSpacing - (gridSize.x - 1) * fallbackCardSpacing / 2f,
                        y * fallbackCardSpacing - (gridSize.y - 1) * fallbackCardSpacing / 2f,
                        0
                    );
                }
                
                GameObject cardObj = Instantiate(cardPrefab, position, Quaternion.identity, cardContainer);
                Card card = cardObj.GetComponent<Card>();
                
                // Set card scale based on calculated size
                if (useResponsiveLayout && autoScaleCards)
                {
                    card.SetCardScale(new Vector3(cardSize.x, cardSize.y, 1f));
                }
                
                // Set card value and sprite
                card.SetCardValue(cardValues[cardIndex]);
                if (cardValues[cardIndex] < cardSprites.Count)
                {
                    card.SetFrontSprite(cardSprites[cardValues[cardIndex]]);
                }
                
                allCards.Add(card);
                cardIndex++;
            }
        }
        
        Debug.Log($"Created {gridSize.x}x{gridSize.y} grid with {totalPairs} pairs.");
    }
    
    public void OnCardClicked(Card clickedCard)
    {
        // Prevent clicking during match check or if game is over
        if (isCheckingMatch || clickedCard.IsFlipped || clickedCard.IsMatched)
            return;
            
        // Prevent more than maxFlippedCards from being flipped
        if (flippedCards.Count >= maxFlippedCards)
            return;
        
        // Record the move for scoring
        if (scoreManager != null)
        {
            scoreManager.RecordMove();
        }
        
        // Flip the card
        clickedCard.FlipCard();
        flippedCards.Add(clickedCard);
        
        // Check for match when we have maxFlippedCards flipped
        if (flippedCards.Count == maxFlippedCards)
        {
            StartCoroutine(CheckForMatch());
        }
    }
    
    private IEnumerator CheckForMatch()
    {
        isCheckingMatch = true;
        
        // Wait for the flip animation to complete
        yield return new WaitForSeconds(matchCheckDelay);
        
        bool isMatch = true;
        int firstCardValue = flippedCards[0].CardValue;
        
        // Check if all flipped cards have the same value
        for (int i = 1; i < flippedCards.Count; i++)
        {
            if (flippedCards[i].CardValue != firstCardValue)
            {
                isMatch = false;
                break;
            }
        }
        
        if (isMatch)
        {
            // Cards match!
            foreach (Card card in flippedCards)
            {
                card.SetMatched();
            }
            matchedPairs++;
            
            // Record match for scoring
            if (scoreManager != null)
            {
                scoreManager.RecordMatch(matchedPairs, totalPairs);
            }
            
            Debug.Log($"Match found! Pairs matched: {matchedPairs}/{totalPairs}");
            
            // Check win condition
            if (matchedPairs >= totalPairs)
            {
                Debug.Log("🎉 You Win! All pairs matched!");
            }
        }
        else
        {
            // Record mismatch for scoring
            if (scoreManager != null)
            {
                scoreManager.RecordMismatch();
            }
            
            Debug.Log("No match - flipping cards back");
            // Cards don't match, flip them back
            foreach (Card card in flippedCards)
            {
                card.FlipCard(); // Flip back to face down
            }
        }
        
        flippedCards.Clear();
        isCheckingMatch = false;
    }
    
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    // NEW: Method to load from JSON save data
    // Replace your LoadFromSaveData method in GameManager with this:
public void LoadFromSaveData(GameSaveData saveData)
{
    Debug.Log($"🔄 Loading game from save data...");
    
    // Clear current game
    foreach (Card card in allCards)
    {
        if (card != null) DestroyImmediate(card.gameObject);
    }
    allCards.Clear();
    flippedCards.Clear();
    
    // Restore game settings
    gridSize = saveData.gridSize;
    matchedPairs = saveData.matchedPairs;
    totalPairs = saveData.totalPairs;
    
    // Start coroutine to restore cards with proper timing
    StartCoroutine(RestoreCardsCoroutine(saveData.cards));
}

// NEW: Coroutine to restore cards with proper timing
private IEnumerator RestoreCardsCoroutine(List<CardSaveData> cardStates)
{
    foreach (CardSaveData cardData in cardStates)
    {
        // Create card at saved position
        GameObject cardObj = Instantiate(cardPrefab, cardData.GetPosition(), Quaternion.identity, cardContainer);
        Card card = cardObj.GetComponent<Card>();
        
        // Get the correct sprite for this card
        Sprite frontSprite = null;
        if (cardData.cardValue < cardSprites.Count)
        {
            frontSprite = cardSprites[cardData.cardValue];
        }
        
        // Restore card with sprite - CRITICAL: Pass sprite to restore method
        card.RestoreFromSaveData(cardData, frontSprite);
        
        allCards.Add(card);
        
        // Wait one frame between cards to ensure proper initialization
        yield return null;
    }
    
    Debug.Log($"✅ Game restored: {allCards.Count} cards, Visual states synced!");
}

    
    public void RestartGame()
    {
        // Delete save file on restart
        if (jsonSaveManager != null)
        {
            jsonSaveManager.DeleteSave();
        }
        
        CreateGrid();
        
        // Restart scoring
        if (scoreManager != null)
        {
            scoreManager.StartNewGame();
        }
    }
    
    public void SetGridSize(Vector2Int newGridSize)
    {
        gridSize = newGridSize;
        RestartGame();
    }
    
    // Public getters for save system
    public List<Card> GetAllCards() => allCards;
    public int GetMatchedPairs() => matchedPairs;
    public int GetTotalPairs() => totalPairs;
    public bool IsGameInProgress() => matchedPairs < totalPairs && allCards.Count > 0;
    public Vector2Int GetCurrentGridSize() => gridSize;
    
    // Save/Load methods
    public void SaveCurrentGame()
    {
        if (jsonSaveManager != null)
        {
            jsonSaveManager.SaveGame();
        }
    }
    
    public bool LoadSavedGame()
    {
        if (jsonSaveManager != null)
        {
            return jsonSaveManager.LoadGame();
        }
        return false;
    }
}
