using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(4, 4);
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private float cardSpacing = 1.2f;
    
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
        totalPairs = (gridSize.x * gridSize.y) / 2;
        
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
                Vector3 position = new Vector3(
                    x * cardSpacing - (gridSize.x - 1) * cardSpacing / 2f,
                    y * cardSpacing - (gridSize.y - 1) * cardSpacing / 2f,
                    0
                );
                
                GameObject cardObj = Instantiate(cardPrefab, position, Quaternion.identity, cardContainer);
                Card card = cardObj.GetComponent<Card>();
                
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
    }
    
    public void OnCardClicked(Card clickedCard)
    {
        // Prevent clicking during match check or if game is over
        if (isCheckingMatch || clickedCard.IsFlipped || clickedCard.IsMatched)
            return;
            
        // Prevent more than maxFlippedCards from being flipped
        if (flippedCards.Count >= maxFlippedCards)
            return;
        
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
            
            // Check win condition
            if (matchedPairs >= totalPairs)
            {
                Debug.Log("You Win!");
            }
        }
        else
        {
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
    
    public void RestartGame()
    {
        // Clear current game
        foreach (Card card in allCards)
        {
            if (card != null)
                DestroyImmediate(card.gameObject);
        }
        
        allCards.Clear();
        flippedCards.Clear();
        matchedPairs = 0;
        isCheckingMatch = false;
        
        // Create new grid
        CreateGrid();
    }
}
