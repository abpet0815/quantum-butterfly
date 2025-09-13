using UnityEngine;
using System.Collections;

public class UIAnimator : MonoBehaviour
{
    public static void FadeIn(GameObject panel, float duration = 0.3f)
    {
        if (panel != null)
        {
            panel.SetActive(true);
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.AddComponent<CanvasGroup>();
            
            MonoBehaviour mono = panel.GetComponent<MonoBehaviour>();
            if (mono != null)
                mono.StartCoroutine(FadeCoroutine(canvasGroup, 0f, 1f, duration));
        }
    }
    
    public static void FadeOut(GameObject panel, float duration = 0.3f)
    {
        if (panel != null)
        {
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.AddComponent<CanvasGroup>();
            
            MonoBehaviour mono = panel.GetComponent<MonoBehaviour>();
            if (mono != null)
                mono.StartCoroutine(FadeOutCoroutine(canvasGroup, panel, duration));
        }
    }
    
    private static IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        canvasGroup.alpha = startAlpha;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
    }
    
    private static IEnumerator FadeOutCoroutine(CanvasGroup canvasGroup, GameObject panel, float duration)
    {
        yield return FadeCoroutine(canvasGroup, 1f, 0f, duration);
        panel.SetActive(false);
    }
    
    public static void ScaleButton(Transform button)
    {
        // Simple scale effect using Unity's built-in animation
        if (button.GetComponent<MonoBehaviour>() != null)
        {
            button.GetComponent<MonoBehaviour>().StartCoroutine(ScaleCoroutine(button));
        }
    }
    
    private static IEnumerator ScaleCoroutine(Transform button)
    {
        Vector3 originalScale = button.localScale;
        Vector3 targetScale = originalScale * 1.1f;
        
        // Scale up
        float elapsed = 0f;
        float duration = 0.1f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            button.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }
        
        // Scale down
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            button.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }
        
        button.localScale = originalScale;
    }
}
