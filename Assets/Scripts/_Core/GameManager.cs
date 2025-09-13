using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

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
            
            // FIXED: Setup card container with proper Canvas component
            SetupCardContainer();
            
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
            
            // Create AudioManager if not assigned
            GameObject audioManagerObj = GameObject.Find("AudioManager");
            if (audioManagerObj == null)
            {
                audioManagerObj = new GameObject("AudioManager");
                audioManagerObj.AddComponent<AudioManager>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // FIXED: Setup card container to render above backgrounds
    private void SetupCardContainer()
    {
        if (cardContainer != null)
        {
            Canvas cardCanvas = cardContainer.GetComponent<Canvas>();
            if (cardCanvas == null)
            {
                cardCanvas = cardContainer.gameObject.AddComponent<Canvas>();
                cardContainer.gameObject.AddComponent<GraphicRaycaster>();
            }
            
            // Set canvas to render mode and high sorting order
            cardCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cardCanvas.overrideSorting = true;
            cardCanvas.sortingOrder = 10; // High priority to render above backgrounds
            
            Debug.Log("🃏 Card container setup with high sorting order for visibility above backgrounds");
        }
    }
    
    private void Start()
    {
        Debug.Log("🎮 GameManager ready - waiting for UI to trigger grid creation");
    }
    
    public void CreateGrid()
    {
        ClearAllCards();
        
        int totalCards = gridSize.x * gridSize.y;
        if (totalCards % 2 != 0)
        {
            Debug.LogWarning($"Grid size {gridSize.x}x{gridSize.y} creates odd number of cards ({totalCards}). Adjusting...");
            if (gridSize.x > 1) gridSize.x -= 1;
            else if (gridSize.y > 1) gridSize.y -= 1;
            totalCards = gridSize.x * gridSize.y;
        }
        
        totalPairs = totalCards / 2;
        
        Vector2 cardSize = Vector2.one;
        Vector2 spacing = Vector2.one * fallbackCardSpacing;
        
        if (useResponsiveLayout && gridLayoutManager != null)
        {
            cardSize = gridLayoutManager.CalculateOptimalLayout(gridSize, cardContainer);
            spacing = gridLayoutManager.GetCalculatedSpacing();
        }
        
        List<int> cardValues = new List<int>();
        for (int i = 0; i < totalPairs; i++)
        {
            cardValues.Add(i);
            cardValues.Add(i);
        }
        
        ShuffleList(cardValues);
        
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
                    position = new Vector3(
                        x * fallbackCardSpacing - (gridSize.x - 1) * fallbackCardSpacing / 2f,
                        y * fallbackCardSpacing - (gridSize.y - 1) * fallbackCardSpacing / 2f,
                        0
                    );
                }
                
                GameObject cardObj = Instantiate(cardPrefab, position, Quaternion.identity, cardContainer);
                Card card = cardObj.GetComponent<Card>();
                
                if (useResponsiveLayout && autoScaleCards)
                {
                    card.SetCardScale(new Vector3(cardSize.x, cardSize.y, 1f));
                }
                
                card.SetCardValue(cardValues[cardIndex]);
                if (cardValues[cardIndex] < cardSprites.Count)
                {
                    card.SetFrontSprite(cardSprites[cardValues[cardIndex]]);
                }
                
                allCards.Add(card);
                cardIndex++;
            }
        }
        
        Debug.Log($"✅ Created {gridSize.x}x{gridSize.y} grid with {totalPairs} pairs.");
    }
    
    public void ClearAllCards()
    {
        foreach (Card card in allCards)
        {
            if (card != null && card.gameObject != null)
            {
                Destroy(card.gameObject);
            }
        }
        
        allCards.Clear();
        flippedCards.Clear();
        matchedPairs = 0;
        isCheckingMatch = false;
        
        Debug.Log("🧹 All cards cleared from scene");
    }
    
    public void OnCardClicked(Card clickedCard)
    {
        if (isCheckingMatch || clickedCard.IsMatched)
            return;
            
        if (flippedCards.Count >= maxFlippedCards)
            return;
        
        if (clickedCard.IsFlipped && flippedCards.Contains(clickedCard))
            return;
        
        if (scoreManager != null)
        {
            scoreManager.RecordMove();
        }
        
        clickedCard.FlipCard();
        
        if (!flippedCards.Contains(clickedCard))
        {
            flippedCards.Add(clickedCard);
        }
        
        if (flippedCards.Count == maxFlippedCards)
        {
            StartCoroutine(CheckForMatch());
        }
    }
    
    private IEnumerator CheckForMatch()
    {
        isCheckingMatch = true;
        
        yield return new WaitForSeconds(matchCheckDelay);
        
        bool isMatch = true;
        int firstCardValue = flippedCards[0].CardValue;
        
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
            Debug.Log($"✅ MATCH! Cards {firstCardValue}");
            
            foreach (Card card in flippedCards)
            {
                card.SetMatched();
            }
            matchedPairs++;
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(AudioManager.SoundType.CardMatch);
            }
            
            if (scoreManager != null)
            {
                scoreManager.RecordMatch(matchedPairs, totalPairs);
            }
            
            Debug.Log($"Match found! Pairs matched: {matchedPairs}/{totalPairs}");
            
            if (matchedPairs >= totalPairs)
            {
                Debug.Log("🎉 You Win! All pairs matched!");
                
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(AudioManager.SoundType.GameWin);
                }
            }
        }
        else
        {
            Debug.Log($"❌ MISMATCH! Forcing cards back to face down");
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(AudioManager.SoundType.CardMismatch);
            }
            
            if (scoreManager != null)
            {
                scoreManager.RecordMismatch();
            }
            
            foreach (Card card in flippedCards)
            {
                card.ForceFlipToBack();
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
    
    public void LoadFromSaveData(GameSaveData saveData)
    {
        Debug.Log($"🔄 Loading game from save data...");
        
        ClearAllCards();
        
        gridSize = saveData.gridSize;
        matchedPairs = saveData.matchedPairs;
        totalPairs = saveData.totalPairs;
        
        StartCoroutine(RestoreCardsCoroutine(saveData.cards));
    }
    
    private IEnumerator RestoreCardsCoroutine(List<CardSaveData> cardStates)
    {
        foreach (CardSaveData cardData in cardStates)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardData.GetPosition(), Quaternion.identity, cardContainer);
            Card card = cardObj.GetComponent<Card>();
            
            Sprite frontSprite = null;
            if (cardData.cardValue < cardSprites.Count)
            {
                frontSprite = cardSprites[cardData.cardValue];
            }
            
            card.RestoreFromSaveData(cardData, frontSprite);
            
            allCards.Add(card);
            
            yield return null;
        }
        
        yield return new WaitForSeconds(0.1f);
        
        foreach (Card card in allCards)
        {
            if (!card.IsMatched && card.IsFlipped)
            {
                card.ForceFlipToBack();
            }
        }
        
        Debug.Log($"✅ Game restored: {allCards.Count} cards, all visuals synced!");
    }
    
    public void RestartGame()
    {
        if (jsonSaveManager != null)
        {
            jsonSaveManager.DeleteSave();
        }
        
        CreateGrid();
        
        if (scoreManager != null)
        {
            scoreManager.StartNewGame();
        }
        
        Debug.Log("🔄 Game restarted with fresh grid");
    }
    
    public void SetGridSize(Vector2Int newGridSize)
    {
        gridSize = newGridSize;
        Debug.Log($"🎯 Grid size set to: {gridSize.x}x{gridSize.y}");
        
        if (allCards.Count > 0)
        {
            Debug.Log($"🔄 Recreating grid immediately for active game");
            
            bool hadSave = jsonSaveManager != null && jsonSaveManager.HasSaveFile();
            
            CreateGrid();
            
            if (scoreManager != null)
            {
                scoreManager.StartNewGame();
            }
            
            if (hadSave && jsonSaveManager != null)
            {
                jsonSaveManager.DeleteSave();
                Debug.Log("🗑️ Save deleted due to grid size change");
            }
            
            Debug.Log($"✅ Grid recreated with {allCards.Count} new cards");
        }
        else
        {
            Debug.Log("📋 Grid size stored for next game creation");
        }
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
