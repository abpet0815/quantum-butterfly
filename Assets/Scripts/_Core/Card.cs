using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour
{
    [Header("Card Settings")]
    [SerializeField] private SpriteRenderer frontSprite;
    [SerializeField] private SpriteRenderer backSprite;
    [SerializeField] private float flipDuration = 0.3f;
    [SerializeField] private Vector3 cardScale = new Vector3(1f, 1f, 1f);
    
    [Header("Card Data")]
    [SerializeField] private int cardValue;
    [SerializeField] private bool isFlipped = false;
    [SerializeField] private bool isMatched = false;
    
    private bool isFlipping = false;
    private Coroutine currentFlipCoroutine;
    
    public bool IsFlipped => isFlipped;
    public bool IsMatched => isMatched;
    public int CardValue => cardValue;
    public bool IsFlipping => isFlipping;
    
    private void Start()
    {
        transform.localScale = cardScale;
        ShowBackSide();
        UpdateColliderSize();
    }
    
    public void FlipCard()
{
    // ADDITIONAL: Double-check pause state in flip method
    if (GameInputManager.IsPaused || isFlipping || isMatched) 
    {
        Debug.Log($"🚫 Card {cardValue}: Flip blocked - Paused: {GameInputManager.IsPaused}, Flipping: {isFlipping}, Matched: {isMatched}");
        return;
    }
    
    // Play sound immediately
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.PlaySound(AudioManager.SoundType.CardFlip);
    }
    
    // Start flip animation
    currentFlipCoroutine = StartCoroutine(FlipCoroutine());
}

    private IEnumerator FlipCoroutine()
    {
        // CRITICAL: Set flipping flag FIRST to block further clicks
        isFlipping = true;
        
        float halfDuration = flipDuration / 2f;
        Vector3 startScale = transform.localScale;
        Vector3 midScale = new Vector3(0f, startScale.y, startScale.z);
        
        // First half: Scale down to 0 width
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = Vector3.Lerp(startScale, midScale, t);
            yield return null;
        }
        
        // Change card state at middle of animation
        isFlipped = !isFlipped;
        if (isFlipped)
            ShowFrontSide();
        else
            ShowBackSide();
            
        UpdateColliderSize();
        
        // Second half: Scale back up to full width
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = Vector3.Lerp(midScale, startScale, t);
            yield return null;
        }
        
        // CRITICAL: Ensure exact final scale and clear flags
        transform.localScale = startScale;
        isFlipping = false;
        currentFlipCoroutine = null;
        
        Debug.Log($"Card {cardValue}: Flip complete - Now {(isFlipped ? "Front" : "Back")}");
    }

    public void RestoreFromSaveData(CardSaveData saveData, Sprite frontSpr = null)
    {
        SetCardValue(saveData.cardValue);
        
        if (frontSpr != null)
        {
            SetFrontSprite(frontSpr);
        }
        
        // Force both sprites active for setup
        frontSprite.gameObject.SetActive(true);
        backSprite.gameObject.SetActive(true);
        
        // Restore states
        isMatched = saveData.isMatched;
        isFlipped = saveData.isFlipped;
        
        StartCoroutine(DelayedVisualSync());
    }
    
    private IEnumerator DelayedVisualSync()
    {
        yield return null; // Wait one frame
        
        if (isMatched)
        {
            SetMatched();
            ShowFrontSide();
        }
        else if (isFlipped)
        {
            ShowFrontSide();
        }
        else
        {
            ShowBackSide();
        }
        
        UpdateColliderSize();
    }

    private void UpdateColliderSize()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }

        SpriteRenderer activeSprite = isFlipped ? frontSprite : backSprite;
        if (activeSprite != null && activeSprite.sprite != null)
        {
            Vector2 spriteSize = activeSprite.sprite.bounds.size;
            collider.size = spriteSize;
            collider.offset = Vector2.zero;
        }
    }
    
    private void ShowFrontSide()
    {
        if (frontSprite != null && backSprite != null)
        {
            frontSprite.gameObject.SetActive(true);
            backSprite.gameObject.SetActive(false);
        }
    }
    
    private void ShowBackSide()
    {
        if (frontSprite != null && backSprite != null)
        {
            frontSprite.gameObject.SetActive(false);
            backSprite.gameObject.SetActive(true);
        }
    }
    
    public void SetCardValue(int value)
    {
        cardValue = value;
    }
    
    public void SetFrontSprite(Sprite sprite)
    {
        if (frontSprite != null && sprite != null)
        {
            frontSprite.sprite = sprite;
        }
    }
    
    public void SetCardScale(Vector3 newScale)
    {
        cardScale = newScale;
        transform.localScale = cardScale;
        UpdateColliderSize();
    }
    
    public void SetMatched()
    {
        isMatched = true;
        
        // Stop any ongoing flip animation
        if (currentFlipCoroutine != null)
        {
            StopCoroutine(currentFlipCoroutine);
            currentFlipCoroutine = null;
            isFlipping = false;
            transform.localScale = cardScale; // Reset scale
        }
    }
    
    private void OnMouseDown()
{
    // CRITICAL: Block ALL card input when game is paused
    if (GameInputManager.IsPaused)
    {
        Debug.Log($"🚫 Card {cardValue}: Click blocked - Game is paused");
        return;
    }
    
    if (GameManager.Instance != null)
    {
        GameManager.Instance.OnCardClicked(this);
    }
}
}
