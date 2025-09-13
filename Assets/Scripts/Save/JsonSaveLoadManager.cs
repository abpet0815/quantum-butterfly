using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class JsonSaveLoadManager : MonoBehaviour
{
    [Header("Save Settings")]
    private string saveFileName = "cardgame_save.json";
    [SerializeField] private bool debugMode = true;
    
    public static JsonSaveLoadManager Instance { get; private set; }
    
    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);
    
    public System.Action<GameSaveData> OnGameLoaded;
    public System.Action OnGameSaved;
    
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
    if (debugMode)
    {
        Debug.Log($"💾 Save path: {SavePath}");
    }
    

}

    
    public void SaveGame()
    {
        try
        {
            GameSaveData saveData = CreateSaveData();
            
            if (saveData == null)
            {
                Debug.LogError("❌ Failed to create save data");
                return;
            }
            
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, json);
            
            OnGameSaved?.Invoke();
            
            if (debugMode)
            {
                Debug.Log($"💾 Game saved successfully!");
                Debug.Log($"📊 Cards: {saveData.cards.Count}, Score: {saveData.currentScore}");
                Debug.Log($"🗂️ File: {SavePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Save failed: {e.Message}");
        }
    }
    
    public bool LoadGame()
    {
        try
        {
            if (!HasSaveFile())
            {
                if (debugMode) Debug.Log("📂 No save file found");
                return false;
            }
            
            string json = File.ReadAllText(SavePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
            
            if (saveData == null)
            {
                Debug.LogError("❌ Save data is corrupted");
                return false;
            }
            
            RestoreGameFromSave(saveData);
            OnGameLoaded?.Invoke(saveData);
            
            if (debugMode)
            {
                Debug.Log($"📁 Game loaded successfully!");
                Debug.Log($"📊 Cards: {saveData.cards.Count}, Score: {saveData.currentScore}");
                Debug.Log($"📅 Saved: {saveData.saveDateTime}");
            }
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Load failed: {e.Message}");
            return false;
        }
    }
    
    private GameSaveData CreateSaveData()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found");
            return null;
        }
        
        GameSaveData saveData = new GameSaveData();
        
        // Game state
        saveData.gridSize = GameManager.Instance.GetCurrentGridSize();
        saveData.matchedPairs = GameManager.Instance.GetMatchedPairs();
        saveData.totalPairs = GameManager.Instance.GetTotalPairs();
        saveData.gameInProgress = GameManager.Instance.IsGameInProgress();
        
        // Cards data
        var allCards = GameManager.Instance.GetAllCards();
        for (int i = 0; i < allCards.Count; i++)
        {
            Card card = allCards[i];
            Vector2Int gridPos = new Vector2Int(i % saveData.gridSize.x, i / saveData.gridSize.x);
            CardSaveData cardData = new CardSaveData(card, gridPos);
            saveData.cards.Add(cardData);
        }
        
        // Score data
        if (ScoreManager.Instance != null)
        {
            saveData.currentScore = ScoreManager.Instance.GetCurrentScore();
            saveData.totalMoves = ScoreManager.Instance.GetTotalMoves();
            saveData.currentCombo = ScoreManager.Instance.GetCurrentCombo();
            saveData.isPerfectGame = ScoreManager.Instance.GetPerfectGameStatus();
            saveData.gameTime = ScoreManager.Instance.GetGameTime();
        }
        
        return saveData;
    }
    
    private void RestoreGameFromSave(GameSaveData saveData)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadFromSaveData(saveData);
        }
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RestoreFromSave(saveData);
        }
    }
    
    public bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }
    
    public void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                if (debugMode) Debug.Log("🗑️ Save file deleted");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete save: {e.Message}");
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveGame();
    }
    
    private void OnDestroy()
    {
        SaveGame();
    }
    
    #if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        SaveGame();
    }
    #endif
}
