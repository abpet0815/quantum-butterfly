using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    [Header("Scoring Settings")]
    [SerializeField] private int baseMatchScore = 100;
    [SerializeField] private int comboMultiplier = 50;
    [SerializeField] private float comboTimeWindow = 3f;
    [SerializeField] private int timeBonus = 10;
    [SerializeField] private int perfectMatchBonus = 500;
    
    [Header("Current Game Stats")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int currentCombo = 0;
    [SerializeField] private int totalMatches = 0;
    [SerializeField] private int totalMoves = 0;
    [SerializeField] private float gameStartTime;
    [SerializeField] private float lastMatchTime;
    
    [Header("Performance Tracking")]
    [SerializeField] private int consecutiveMatches = 0;
    [SerializeField] private bool isPerfectGame = true; // No mismatches
    
    public static ScoreManager Instance { get; private set; }
    
    // Events for UI updates
    public event Action<int> OnScoreChanged;
    public event Action<int> OnComboChanged;
    public event Action<int> OnMovesChanged;
    public event Action<GameStats> OnGameCompleted;
    
    [System.Serializable]
    public class GameStats
    {
        public int finalScore;
        public int totalMoves;
        public float totalTime;
        public int maxCombo;
        public bool perfectGame;
        public float efficiency; // Score per move
    }
    
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
    
    public void StartNewGame()
    {
        currentScore = 0;
        currentCombo = 0;
        totalMatches = 0;
        totalMoves = 0;
        consecutiveMatches = 0;
        isPerfectGame = true;
        gameStartTime = Time.time;
        lastMatchTime = 0f;
        
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(currentCombo);
        OnMovesChanged?.Invoke(totalMoves);
        
        Debug.Log("New game started - Score tracking initialized!");
    }
    
    public void RecordMove()
    {
        totalMoves++;
        OnMovesChanged?.Invoke(totalMoves);
    }
    
    public void RecordMatch(int matchedPairs, int totalPairs)
    {
        totalMatches++;
        float currentTime = Time.time;
        float matchTime = currentTime - (lastMatchTime > 0 ? lastMatchTime : gameStartTime);
        
        // Calculate base score
        int matchScore = baseMatchScore;
        
        // Apply combo system
        if (currentTime - lastMatchTime <= comboTimeWindow && lastMatchTime > 0)
        {
            currentCombo++;
            matchScore += currentCombo * comboMultiplier;
            Debug.Log($"🔥 COMBO x{currentCombo}! Bonus: +{currentCombo * comboMultiplier}");
            
            // Play combo sound for high combos
            if (currentCombo >= 3)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(AudioManager.SoundType.ComboBonus);
                    Debug.Log($"ScoreManager: Playing ComboBonus sound for combo {currentCombo}.");
                }
                else
                {
                    Debug.LogWarning("ScoreManager: AudioManager.Instance is null. Cannot play combo bonus sound.");
                }
            }
        }
        else
        {
            currentCombo = 1; // Reset combo but start at 1 for this match
        }
        
        // Time bonus (faster matches get more points)
        if (matchTime < 2f)
        {
            int timeBonusPoints = Mathf.RoundToInt(timeBonus * (2f - matchTime));
            matchScore += timeBonusPoints;
            Debug.Log($"⚡ Speed bonus: +{timeBonusPoints}");
        }
        
        // Consecutive match bonus
        consecutiveMatches++;
        if (consecutiveMatches >= 3)
        {
            matchScore += 50;
            Debug.Log($"🎯 Hot streak bonus: +50");
        }
        
        currentScore += matchScore;
        lastMatchTime = currentTime;
        
        // Check for perfect game completion
        if (matchedPairs >= totalPairs)
        {
            CompleteGame(totalPairs);
        }
        
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(currentCombo);
        
        Debug.Log($"ScoreManager: Match scored! Points: {matchScore}, Total: {currentScore}");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.SoundType.CardMatch);
            Debug.Log("ScoreManager: Playing CardMatch sound.");
        }
        else
        {
            Debug.LogWarning("ScoreManager: AudioManager.Instance is null. Cannot play card match sound.");
        }
    }
    
    public void RecordMismatch()
    {
        isPerfectGame = false;
        consecutiveMatches = 0; // Break hot streak
        
        // Reset combo on mismatch (harsh but encourages careful play)
        if (currentCombo > 1)
        {
            Debug.Log($"💥 Combo broken! Was at x{currentCombo}");
            currentCombo = 0;
            OnComboChanged?.Invoke(currentCombo);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.SoundType.CardMismatch);
            Debug.Log("ScoreManager: Playing CardMismatch sound.");
        }
        else
        {
            Debug.LogWarning("ScoreManager: AudioManager.Instance is null. Cannot play card mismatch sound.");
        }
    }
    
    private void CompleteGame(int totalPairs)
    {
        float totalGameTime = Time.time - gameStartTime;
        
        // Perfect game bonus
        if (isPerfectGame)
        {
            currentScore += perfectMatchBonus;
            Debug.Log($"🏆 PERFECT GAME! Bonus: +{perfectMatchBonus}");
        }
        
        // Efficiency bonus (high score with few moves)
        float efficiency = (float)currentScore / totalMoves;
        if (efficiency > 50f) // Arbitrary threshold
        {
            int efficiencyBonus = Mathf.RoundToInt(efficiency * 10);
            currentScore += efficiencyBonus;
            Debug.Log($"🧠 Efficiency bonus: +{efficiencyBonus}");
        }
        
        GameStats finalStats = new GameStats
        {
            finalScore = currentScore,
            totalMoves = totalMoves,
            totalTime = totalGameTime,
            maxCombo = GetMaxComboThisGame(),
            perfectGame = isPerfectGame,
            efficiency = efficiency
        };
        
        OnScoreChanged?.Invoke(currentScore);
        OnGameCompleted?.Invoke(finalStats);
        
        Debug.Log($"🎉 Game Complete! Final Score: {currentScore}, Time: {totalGameTime:F1}s, Moves: {totalMoves}");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.SoundType.GameWin);
            Debug.Log("ScoreManager: Playing GameWin sound.");
        }
        else
        {
            Debug.LogWarning("ScoreManager: AudioManager.Instance is null. Cannot play game win sound.");
        }
    }
    
    private int GetMaxComboThisGame()
    {
        // For now return current combo, but you could track max separately
        return currentCombo;
    }
    
    // Method to restore from JSON save data
    public void RestoreFromSave(GameSaveData saveData)
    {
        currentScore = saveData.currentScore;
        currentCombo = saveData.currentCombo;
        totalMoves = saveData.totalMoves;
        isPerfectGame = saveData.isPerfectGame;
        
        // Restore timing
        gameStartTime = Time.time - saveData.gameTime;
        
        // Trigger UI updates
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(currentCombo);
        OnMovesChanged?.Invoke(totalMoves);
        
        Debug.Log($"📊 Score restored: {currentScore} points, {totalMoves} moves");
    }
    
    // Public getters for UI
    public int GetCurrentScore() => currentScore;
    public int GetCurrentCombo() => currentCombo;
    public int GetTotalMoves() => totalMoves;
    public float GetGameTime() => Time.time - gameStartTime;
    public bool GetPerfectGameStatus() => isPerfectGame;
    
    // Method to calculate score preview (for UI)
    public int CalculatePotentialScore(bool wouldBeCombo, float timeSinceLastMatch)
    {
        int potentialScore = baseMatchScore;
        
        if (wouldBeCombo)
        {
            potentialScore += (currentCombo + 1) * comboMultiplier;
        }
        
        if (timeSinceLastMatch < 2f)
        {
            potentialScore += Mathf.RoundToInt(timeBonus * (2f - timeSinceLastMatch));
        }
        
        return potentialScore;
    }
}
