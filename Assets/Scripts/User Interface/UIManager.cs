using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject pausePanel;
    
    [Header("Overlay System")]
    [SerializeField] private GameObject overlayBlocker;
    
    [Header("Gameplay UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button pauseButton;
    
    [Header("Main Menu")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button continueButton;
    
    [Header("Game Over")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalMovesText;
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private TextMeshProUGUI perfectGameText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Settings - Grid Size Only")]
    [SerializeField] private TMP_Dropdown gridSizeDropdown;
    [SerializeField] private Button settingsBackButton;
    
    [Header("Pause Menu")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseSettingsButton;
    [SerializeField] private Button pauseMainMenuButton;
    
    public static UIManager Instance { get; private set; }
    
    private bool isPaused = false;
    private Vector2Int pendingGridSize;
    private bool isInGameSettings = false;
    
    public enum GameState
    {
        MainMenu,
        Gameplay,
        GameOver,
        Settings,
        Paused
    }
    
    private GameState currentState = GameState.MainMenu;
    private GameState previousState = GameState.MainMenu;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SetupButtons();
        SetupGridSizeDropdown();
        SubscribeToGameEvents();
        UpdateContinueButton();
        
        pendingGridSize = new Vector2Int(4, 4);
        
        ShowMainMenu();
    }
    
    private void SetupButtons()
    {
        // Main Menu
        playButton.onClick.AddListener(StartNewGame);
        settingsButton.onClick.AddListener(() => {
            isInGameSettings = false;
            ShowSettings();
        });
        quitButton.onClick.AddListener(QuitGame);
        continueButton.onClick.AddListener(ContinueGame);
        
        // Gameplay
        pauseButton.onClick.AddListener(PauseGame);
        
        // Game Over
        playAgainButton.onClick.AddListener(StartNewGame);
        mainMenuButton.onClick.AddListener(ShowMainMenu);
        
        // Settings
        settingsBackButton.onClick.AddListener(CloseSettings);
        
        // Pause Menu
        resumeButton.onClick.AddListener(ResumeGame);
        pauseSettingsButton.onClick.AddListener(() => {
            isInGameSettings = true;
            ShowSettings();
        });
        pauseMainMenuButton.onClick.AddListener(ShowMainMenu);
    }
    
    private void SetupGridSizeDropdown()
    {
        gridSizeDropdown.ClearOptions();
        gridSizeDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "2x2 (4 cards)",
            "3x4 (12 cards)",
            "4x4 (16 cards)",
            "5x4 (20 cards)",
            "6x4 (24 cards)"
        });
        
        gridSizeDropdown.value = 2;
        gridSizeDropdown.RefreshShownValue();
        
        gridSizeDropdown.onValueChanged.AddListener(OnGridSizeChanged);
    }
    
    private void SubscribeToGameEvents()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            ScoreManager.Instance.OnMovesChanged += UpdateMovesDisplay;
            ScoreManager.Instance.OnComboChanged += UpdateComboDisplay;
            ScoreManager.Instance.OnGameCompleted += OnGameCompleted;
        }
    }
    
    private void UpdateContinueButton()
    {
        if (JsonSaveLoadManager.Instance != null && continueButton != null)
        {
            bool hasSave = JsonSaveLoadManager.Instance.HasSaveFile();
            continueButton.gameObject.SetActive(hasSave);
            
            if (hasSave)
            {
                Debug.Log("✅ Save file found - Continue button enabled");
            }
            else
            {
                Debug.Log("📂 No save file - Continue button hidden");
            }
        }
    }
    
    private void Update()
    {
        // Update timer during gameplay
        if (currentState == GameState.Gameplay && !isPaused && ScoreManager.Instance != null)
        {
            float gameTime = ScoreManager.Instance.GetGameTime();
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
        
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Gameplay)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }
    
    public void ShowMainMenu()
    {
        // Clear all cards before showing main menu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearAllCards();
        }
        
        SetActivePanel(GameState.MainMenu);
        Time.timeScale = 1f;
        isPaused = false;
        isInGameSettings = false;
        
        // Set input manager to not paused
        GameInputManager.SetPaused(false);
        
        // Update continue button visibility
        UpdateContinueButton();
        
        PlayButtonSound();
        Debug.Log("📋 Returned to Main Menu - cards cleared");
    }
    
    public void ShowGameplay()
    {
        SetActivePanel(GameState.Gameplay);
        Time.timeScale = 1f;
        isPaused = false;
        
        // Set input manager to not paused
        GameInputManager.SetPaused(false);
        
        // Initialize UI displays
        UpdateScoreDisplay(0);
        UpdateMovesDisplay(0);
        UpdateComboDisplay(0);
    }
    
    public void ShowGameOver(ScoreManager.GameStats stats)
    {
        SetActivePanel(GameState.GameOver);
        
        finalScoreText.text = $"Final Score: {stats.finalScore:N0}";
        finalMovesText.text = $"Moves: {stats.totalMoves}";
        
        int minutes = Mathf.FloorToInt(stats.totalTime / 60f);
        int seconds = Mathf.FloorToInt(stats.totalTime % 60f);
        finalTimeText.text = $"Time: {minutes:00}:{seconds:00}";
        
        perfectGameText.text = stats.perfectGame ? "🏆 PERFECT GAME!" : "";
        perfectGameText.gameObject.SetActive(stats.perfectGame);
    }
    
    public void ShowSettings()
    {
        previousState = currentState;
        SetActivePanel(GameState.Settings);
        PlayButtonSound();
    }
    
    private void StartNewGame()
    {
        PlayButtonSound();
        
        // Delete existing save to start fresh
        if (JsonSaveLoadManager.Instance != null)
        {
            JsonSaveLoadManager.Instance.DeleteSave();
        }
        
        // Apply pending grid size and start new game
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGridSize(pendingGridSize);
            GameManager.Instance.RestartGame();
        }
        
        ShowGameplay();
        Debug.Log($"🆕 Starting new game with grid {pendingGridSize.x}x{pendingGridSize.y}");
    }
    
    private void ContinueGame()
    {
        PlayButtonSound();
        
        // Load saved game only when continue is clicked
        if (GameManager.Instance != null && JsonSaveLoadManager.Instance != null)
        {
            JsonSaveLoadManager.Instance.LoadGame();
        }
        
        ShowGameplay();
        Debug.Log("📁 Continuing saved game");
    }
    
    private void PauseGame()
    {
        PlayButtonSound();
        SetActivePanel(GameState.Paused);
        Time.timeScale = 0f;
        isPaused = true;
        
        // Set global pause state to block card inputs
        GameInputManager.SetPaused(true);
    }
    
    private void ResumeGame()
    {
        PlayButtonSound();
        SetActivePanel(GameState.Gameplay);
        Time.timeScale = 1f;
        isPaused = false;
        
        // Remove global pause state to allow card inputs
        GameInputManager.SetPaused(false);
    }
    
    private void QuitGame()
    {
        PlayButtonSound();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    private void CloseSettings()
    {
        PlayButtonSound();
        
        if (previousState == GameState.Paused)
        {
            SetActivePanel(GameState.Paused);
        }
        else
        {
            ShowMainMenu();
        }
    }
    
    private void OnGridSizeChanged(int index)
    {
        Vector2Int[] gridSizes = {
            new Vector2Int(2, 2),  // 2x2 (4 cards)
            new Vector2Int(3, 4),  // 3x4 (12 cards)
            new Vector2Int(4, 4),  // 4x4 (16 cards)
            new Vector2Int(5, 4),  // 5x4 (20 cards)
            new Vector2Int(6, 4)   // 6x4 (24 cards)
        };
        
        if (index >= 0 && index < gridSizes.Length)
        {
            Vector2Int selectedSize = gridSizes[index];
            
            if (isInGameSettings)
            {
                // IN-GAME SETTINGS: Apply immediately
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetGridSize(selectedSize);
                    Debug.Log($"🎯 In-game grid changed to: {selectedSize.x}x{selectedSize.y} (applied immediately)");
                }
            }
            else
            {
                // MAIN MENU SETTINGS: Store for later use
                pendingGridSize = selectedSize;
                Debug.Log($"📋 Main menu grid set to: {selectedSize.x}x{selectedSize.y} (will apply on Play)");
            }
        }
    }
    
    private void SetActivePanel(GameState newState)
    {
        // Hide all panels
        mainMenuPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        settingsPanel.SetActive(false);
        pausePanel.SetActive(false);
        
        // Control overlay and input blocking
        bool shouldBlockInput = (newState == GameState.Paused || newState == GameState.Settings || newState == GameState.GameOver);
        
        // Show/hide overlay blocker
        if (overlayBlocker != null)
        {
            overlayBlocker.SetActive(shouldBlockInput);
        }
        
        // Set global input blocking
        if (shouldBlockInput)
        {
            GameInputManager.SetPaused(true);
        }
        else if (newState == GameState.Gameplay)
        {
            GameInputManager.SetPaused(false);
        }
        
        // Show appropriate panel
        switch (newState)
        {
            case GameState.MainMenu:
                mainMenuPanel.SetActive(true);
                break;
            case GameState.Gameplay:
                gameplayPanel.SetActive(true);
                break;
            case GameState.GameOver:
                gameOverPanel.SetActive(true);
                break;
            case GameState.Settings:
                settingsPanel.SetActive(true);
                break;
            case GameState.Paused:
                pausePanel.SetActive(true);
                break;
        }
        
        currentState = newState;
        
        Debug.Log($"🎯 UI State: {newState}, Input Blocked: {shouldBlockInput}");
    }
    
    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score:N0}";
    }
    
    private void UpdateMovesDisplay(int moves)
    {
        if (movesText != null)
            movesText.text = $"Moves: {moves}";
    }
    
    private void UpdateComboDisplay(int combo)
    {
        if (comboText != null)
        {
            if (combo > 1)
            {
                comboText.text = $"Combo: x{combo}";
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }
    
    private void OnGameCompleted(ScoreManager.GameStats stats)
    {
        // Clear cards before showing game over screen
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearAllCards();
        }
        
        ShowGameOver(stats);
        Debug.Log("🎉 Game completed - cards cleared");
    }
    
    private void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.SoundType.ButtonClick);
        }
    }
    
    public bool IsPaused() => isPaused;
    
    public GameState GetCurrentState() => currentState;
}
