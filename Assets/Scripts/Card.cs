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
        
        StartCoroutine(FlipCardCoroutine());
    }

    private IEnumerator FlipCardCoroutine()
    {
        isFlipping = true;
        
        // First half of flip
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
        
        // Switch sprites
        isFlipped = !isFlipped;
        if (isFlipped)
            ShowFrontSide();
        else
            ShowBackSide();
        
        UpdateColliderSize(); // Update collider after sprite switch
        
        // Second half of flip
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

    private void UpdateColliderSize()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }

        // Get the currently active sprite
        SpriteRenderer activeSprite = isFlipped ? frontSprite : backSprite;
        
        if (activeSprite != null && activeSprite.sprite != null)
        {
            // Get the sprite size in world units
            Vector2 spriteSize = activeSprite.sprite.bounds.size;
            
            // Apply the transform scale to the collider size
            collider.size = new Vector2(
                spriteSize.x,
                spriteSize.y
            );
            
            // Center the collider
            collider.offset = Vector2.zero;
        }
    }
    
    private void ShowFrontSide()
    {
        frontSprite.gameObject.SetActive(true);
        backSprite.gameObject.SetActive(false);
    }
    
    private void ShowBackSide()
    {
        frontSprite.gameObject.SetActive(false);
        backSprite.gameObject.SetActive(true);
    }
    
    public void SetCardValue(int value)
    {
        cardValue = value;
    }
    
    public void SetFrontSprite(Sprite sprite)
    {
        if (frontSprite != null)
            frontSprite.sprite = sprite;
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
        // Optional: Add matched visual effect
    }
    
    private void OnMouseDown()
    {
        // Send click to GameManager instead of handling directly
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCardClicked(this);
        }
    }
}
