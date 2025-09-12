using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour
{
    [Header("Card Settings")]
    [SerializeField] private SpriteRenderer frontSprite;
    [SerializeField] private SpriteRenderer backSprite;
    [SerializeField] private float flipDuration = 0.5f;
    [SerializeField] private Vector3 cardScale = new Vector3(1f, 1f, 1f);
    
    [Header("Card Data")]
    [SerializeField] private int cardValue;
    [SerializeField] private bool isFlipped = false;
    [SerializeField] private bool isMatched = false;
    
    private bool isFlipping = false;
    
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
        if (isFlipping || isMatched) return;
        
        // Play flip sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.SoundType.CardFlip);
        }
        
        StartCoroutine(FlipCardCoroutine());
    }

    private IEnumerator FlipCardCoroutine()
    {
        isFlipping = true;
        
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 halfScale = new Vector3(0f, startScale.y, startScale.z);
        
        while (elapsed < flipDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (flipDuration / 2f);
            transform.localScale = Vector3.Lerp(startScale, halfScale, progress);
            yield return null;
        }
        
        isFlipped = !isFlipped;
        if (isFlipped)
            ShowFrontSide();
        else
            ShowBackSide();
        
        UpdateColliderSize();
        
        elapsed = 0f;
        while (elapsed < flipDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (flipDuration / 2f);
            transform.localScale = Vector3.Lerp(halfScale, startScale, progress);
            yield return null;
        }
        
        transform.localScale = startScale;
        isFlipping = false;
    }

    // Method to restore card from save data with proper timing
    public void RestoreFromSaveData(CardSaveData saveData, Sprite frontSpr = null)
    {
        // Set card value first
        SetCardValue(saveData.cardValue);
        
        // Set sprite if provided - CRITICAL: Do this BEFORE state restoration
        if (frontSpr != null)
        {
            SetFrontSprite(frontSpr);
        }
        
        // CRITICAL: Force both sprites to be active initially so they exist
        frontSprite.gameObject.SetActive(true);
        backSprite.gameObject.SetActive(true);
        
        // Now restore states
        isMatched = saveData.isMatched;
        isFlipped = saveData.isFlipped;
        
        // CRITICAL: Wait one frame for sprites to initialize, then sync visuals
        StartCoroutine(DelayedVisualSync());
    }
    
    // CRITICAL: Delayed visual sync to ensure sprites are ready
    private IEnumerator DelayedVisualSync()
    {
        yield return null; // Wait one frame
        
        // Now sync visuals with state
        if (isMatched)
        {
            SetMatched();
            ShowFrontSide(); // Matched cards show front
        }
        else if (isFlipped)
        {
            ShowFrontSide(); // Flipped cards show front
        }
        else
        {
            ShowBackSide(); // Normal cards show back
        }
        
        UpdateColliderSize();
        
        Debug.Log($"🔄 Card {cardValue}: Restored - Flipped={isFlipped}, Matched={isMatched}, Visual={frontSprite.gameObject.activeInHierarchy}");
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
            collider.size = new Vector2(spriteSize.x, spriteSize.y);
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
    }
    
    private void OnMouseDown()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCardClicked(this);
        }
    }
}
