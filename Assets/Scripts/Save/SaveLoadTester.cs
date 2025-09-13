using UnityEngine;

public class SaveLoadTester : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            // Manual save
            if (JsonSaveLoadManager.Instance != null)
            {
                JsonSaveLoadManager.Instance.SaveGame();
                Debug.Log("💾 Manual save (F5)");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            // Manual load
            if (JsonSaveLoadManager.Instance != null)
            {
                JsonSaveLoadManager.Instance.LoadGame();
                Debug.Log("📁 Manual load (F9)");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            // Delete save
            if (JsonSaveLoadManager.Instance != null)
            {
                JsonSaveLoadManager.Instance.DeleteSave();
                Debug.Log("🗑️ Save deleted (Delete)");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Restart game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
                Debug.Log("🔄 Game restarted (R)");
            }
        }
    }
}
