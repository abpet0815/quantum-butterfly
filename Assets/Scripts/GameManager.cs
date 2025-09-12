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
    [SerializeField] private bool autoScaleCards = false; // NEW: Control scaling separately
    [SerializeField] private float fallbackCardSpacing = 1.2f;
    
    [Header("Game Rules")]
    [SerializeField] private float matchCheckDelay = 1f;
    [SerializeField] private int maxFlippedCards = 2;
    
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
            
            if (gridLayoutManager == null)
            {
                GameObject layoutManagerObj = new GameObject("GridLayoutManager");
                layoutManagerObj.transform.SetParent(transform);
                gridLayoutManager = layoutManagerObj.AddComponent<GridLayoutManager>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        CreateGrid();
    }
    
    private void CreateGrid()
    {
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
            cardValues.Add(i);
        }
        
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
                    position = new Vector3(
                        x * fallbackCardSpacing - (gridSize.x - 1) * fallbackCardSpacing / 2f,
                        y * fallbackCardSpacing - (gridSize.y - 1) * fallbackCardSpacing / 2f,
                        0
                    );
                }
                
                GameObject cardObj = Instantiate(cardPrefab, position, Quaternion.identity, cardContainer);
                Card card = cardObj.GetComponent<Card>();
                
                // REMOVED: Auto-scaling - only scale if explicitly enabled
                if (useResponsiveLayout && autoScaleCards)
                {
                    card.SetCardScale(new Vector3(cardSize.x, cardSize.y, 1f));
                }
                // Cards now keep their original scale from the prefab!
                
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
        if (isCheckingMatch || clickedCard.IsFlipped || clickedCard.IsMatched)
            return;
            
        if (flippedCards.Count >= maxFlippedCards)
            return;
        
        clickedCard.FlipCard();
        flippedCards.Add(clickedCard);
        
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
            foreach (Card card in flippedCards)
            {
                card.SetMatched();
            }
            matchedPairs++;
            
            Debug.Log($"Match found! Pairs matched: {matchedPairs}/{totalPairs}");
            
            if (matchedPairs >= totalPairs)
            {
                Debug.Log("🎉 You Win! All pairs matched!");
            }
        }
        else
        {
            Debug.Log("No match - flipping cards back");
            foreach (Card card in flippedCards)
            {
                card.FlipCard();
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
    
    public void RestartGame()
    {
        foreach (Card card in allCards)
        {
            if (card != null)
                DestroyImmediate(card.gameObject);
        }
        
        allCards.Clear();
        flippedCards.Clear();
        matchedPairs = 0;
        isCheckingMatch = false;
        
        CreateGrid();
    }
    
    public void SetGridSize(Vector2Int newGridSize)
    {
        gridSize = newGridSize;
        RestartGame();
    }
}
