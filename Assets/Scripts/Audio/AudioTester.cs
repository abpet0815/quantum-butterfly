using UnityEngine;

public class AudioTester : MonoBehaviour
{
    private void Update()
    {
        if (AudioManager.Instance == null) return;
        
        // Test different sounds with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AudioManager.Instance.PlaySound(AudioManager.SoundType.CardFlip);
            Debug.Log("🔊 Test: Card Flip Sound");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AudioManager.Instance.PlaySound(AudioManager.SoundType.CardMatch);
            Debug.Log("🔊 Test: Card Match Sound");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AudioManager.Instance.PlaySound(AudioManager.SoundType.CardMismatch);
            Debug.Log("🔊 Test: Card Mismatch Sound");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            AudioManager.Instance.PlaySound(AudioManager.SoundType.GameWin);
            Debug.Log("🔊 Test: Game Win Sound");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            AudioManager.Instance.PlaySound(AudioManager.SoundType.ComboBonus);
            Debug.Log("🔊 Test: Combo Bonus Sound");
        }
        
        // Volume controls
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            float newVol = Mathf.Max(0f, AudioManager.Instance.GetMasterVolume() - 0.1f);
            AudioManager.Instance.SetMasterVolume(newVol);
            Debug.Log($"🔊 Master Volume: {newVol:F1}");
        }
        
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus))
        {
            float newVol = Mathf.Min(1f, AudioManager.Instance.GetMasterVolume() + 0.1f);
            AudioManager.Instance.SetMasterVolume(newVol);
            Debug.Log($"🔊 Master Volume: {newVol:F1}");
        }
    }
}
