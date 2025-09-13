using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Backgrounds")]
    [SerializeField] private GameObject mainMenuBackground;
    [SerializeField] private GameObject gameplayBackground;

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

    [Header("Settings - Grid Size Buttons")]
    [SerializeField] private Button gridSize2x2Button;
    [SerializeField] private Button gridSize3x4Button;
    [SerializeField] private Button gridSize4x4Button;
    [SerializeField] private Button gridSize5x6Button;
    [SerializeField] private Button settingsBackButton;

    [Header("Pause Menu")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseSettingsButton;
    [SerializeField] private Button pauseMainMenuButton;

    public static UIManager Instance { get; private set; }

    private bool isPaused = false;
    private Vector2Int pendingGridSize;
    private bool isInGameSettings = false;
    private int selectedGridIndex = 0; // Default to 2x2

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
        SetupGridSizeButtons();
        SubscribeToGameEvents();
        
        // Default to 2x2
        pendingGridSize = new Vector2Int(2, 2);
        
        // FIXED: Setup background layering on start
        SetupBackgroundLayering();
        
        UpdateContinueButton();
        ShowMainMenu();
    }

    // FIXED: Method to ensure backgrounds render behind cards
    private void SetupBackgroundLayering()
    {
        // Set backgrounds to render behind everything
        if (mainMenuBackground != null)
        {
            Canvas bgCanvas = mainMenuBackground.GetComponent<Canvas>();
            if (bgCanvas == null)
                bgCanvas = mainMenuBackground.AddComponent<Canvas>();
                
            bgCanvas.overrideSorting = true;
            bgCanvas.sortingOrder = -100; // Very low priority
            
            // Disable raycast target on background images
            Image bgImage = mainMenuBackground.GetComponent<Image>();
            if (bgImage != null)
                bgImage.raycastTarget = false;
        }

        if (gameplayBackground != null)
        {
            Canvas bgCanvas = gameplayBackground.GetComponent<Canvas>();
            if (bgCanvas == null)
                bgCanvas = gameplayBackground.AddComponent<Canvas>();
                
            bgCanvas.overrideSorting = true;
            bgCanvas.sortingOrder = -100; // Very low priority
            
            // Disable raycast target on background images
            Image bgImage = gameplayBackground.GetComponent<Image>();
            if (bgImage != null)
                bgImage.raycastTarget = false;
        }

        Debug.Log("🎨 Background layering setup complete - backgrounds set to lowest rendering priority");
    }

    private void SetupButtons()
    {
        // Main Menu
        playButton.onClick.AddListener(StartNewGame);
        settingsButton.onClick.AddListener(() =>
        {
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
        pauseSettingsButton.onClick.AddListener(() =>
        {
            isInGameSettings = true;
            ShowSettings();
        });
        pauseMainMenuButton.onClick.AddListener(ShowMainMenu);
    }

    private void SetupGridSizeButtons()
    {
        // Grid Size Buttons
        if (gridSize2x2Button != null)
            gridSize2x2Button.onClick.AddListener(() => OnGridSizeButtonClicked(0));
        if (gridSize3x4Button != null)
            gridSize3x4Button.onClick.AddListener(() => OnGridSizeButtonClicked(1));
        if (gridSize4x4Button != null)
            gridSize4x4Button.onClick.AddListener(() => OnGridSizeButtonClicked(2));
        if (gridSize5x6Button != null)
            gridSize5x6Button.onClick.AddListener(() => OnGridSizeButtonClicked(3));

        UpdateGridSizeButtonVisuals();
    }

    private void OnGridSizeButtonClicked(int gridIndex)
    {
        PlayButtonSound();
        
        selectedGridIndex = gridIndex;
        UpdateGridSizeButtonVisuals();

        Vector2Int[] gridSizes = {
            new Vector2Int(2, 2),  // 2x2 (4 cards)
            new Vector2Int(3, 4),  // 3x4 (12 cards)
            new Vector2Int(4, 4),  // 4x4 (16 cards)
            new Vector2Int(5, 6)   // 5x6 (30 cards)
        };

        if (gridIndex >= 0 && gridIndex < gridSizes.Length)
        {
            Vector2Int selectedSize = gridSizes[gridIndex];

            if (isInGameSettings)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetGridSize(selectedSize);
                    Debug.Log($"🎯 In-game grid changed to: {selectedSize.x}x{selectedSize.y} (applied immediately)");
                }
            }
            else
            {
                pendingGridSize = selectedSize;
                Debug.Log($"📋 Main menu grid set to: {selectedSize.x}x{selectedSize.y} (will apply on Play)");
            }
        }
    }

    private void UpdateGridSizeButtonVisuals()
    {
        // Reset all button colors to normal
        ResetButtonColor(gridSize2x2Button);
        ResetButtonColor(gridSize3x4Button);
        ResetButtonColor(gridSize4x4Button);
        ResetButtonColor(gridSize5x6Button);

        // Highlight the selected button
        Button selectedButton = null;
        switch (selectedGridIndex)
        {
            case 0: selectedButton = gridSize2x2Button; break;
            case 1: selectedButton = gridSize3x4Button; break;
            case 2: selectedButton = gridSize4x4Button; break;
            case 3: selectedButton = gridSize5x6Button; break;
        }

        if (selectedButton != null)
        {
            HighlightButton(selectedButton);
        }
    }

    private void ResetButtonColor(Button button)
    {
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.selectedColor = Color.white;
            button.colors = colors;
        }
    }

    private void HighlightButton(Button button)
    {
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.yellow;
            colors.selectedColor = Color.yellow;
            button.colors = colors;
        }
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

    // FIXED: Comprehensive continue button logic
    private void UpdateContinueButton()
    {
        if (continueButton == null) return;

        bool hasSave = false;
        
        // Check if save manager exists and has save file
        if (JsonSaveLoadManager.Instance != null)
        {
            hasSave = JsonSaveLoadManager.Instance.HasSaveFile();
        }

        // CRITICAL: Only show continue button in main menu AND if save exists
        bool shouldShowContinue = (currentState == GameState.MainMenu) && hasSave;
        
        continueButton.gameObject.SetActive(shouldShowContinue);
        continueButton.interactable = shouldShowContinue;

        if (shouldShowContinue)
        {
            Debug.Log("✅ Continue button shown - save file exists");
        }
        else if (currentState == GameState.MainMenu)
        {
            Debug.Log("📂 Continue button hidden - no save file");
        }
        else
        {
            Debug.Log($"📂 Continue button hidden - not in main menu (current: {currentState})");
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
            if (timerText != null) timerText.text = $"{minutes:00}:{seconds:00}";
        }

        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Gameplay) PauseGame();
            else if (currentState == GameState.Paused) ResumeGame();
        }
    }

    public void ShowMainMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearAllCards();
        }

        SetActivePanel(GameState.MainMenu);
        Time.timeScale = 1f;
        isPaused = false;
        isInGameSettings = false;

        GameInputManager.SetPaused(false);

        // FIXED: Always update continue button when showing main menu
        UpdateContinueButton();

        PlayButtonSound();
        Debug.Log("📋 Returned to Main Menu - cards cleared");
    }

    public void ShowGameplay()
    {
        SetActivePanel(GameState.Gameplay);
        Time.timeScale = 1f;
        isPaused = false;

        GameInputManager.SetPaused(false);

        UpdateScoreDisplay(0);
        UpdateMovesDisplay(0);
        UpdateComboDisplay(0);
    }

    public void ShowGameOver(ScoreManager.GameStats stats)
    {
        SetActivePanel(GameState.GameOver);

        if (finalScoreText) finalScoreText.text = $"Final Score: {stats.finalScore:N0}";
        if (finalMovesText) finalMovesText.text = $"Moves: {stats.totalMoves}";

        int minutes = Mathf.FloorToInt(stats.totalTime / 60f);
        int seconds = Mathf.FloorToInt(stats.totalTime % 60f);
        if (finalTimeText) finalTimeText.text = $"Time: {minutes:00}:{seconds:00}";

        if (perfectGameText)
        {
            perfectGameText.text = stats.perfectGame ? "PERFECT GAME!" : "";
            perfectGameText.gameObject.SetActive(stats.perfectGame);
        }

        // FIXED: Update continue button after game over (should be hidden since we're not in main menu)
        UpdateContinueButton();
    }

    public void ShowSettings()
    {
        previousState = currentState;
        SetActivePanel(GameState.Settings);
        UpdateGridSizeButtonVisuals();
        PlayButtonSound();
    }

    private void StartNewGame()
    {
        PlayButtonSound();

        // Delete save file when starting new game
        if (JsonSaveLoadManager.Instance != null)
        {
            JsonSaveLoadManager.Instance.DeleteSave();
        }

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

        GameInputManager.SetPaused(true);
    }

    private void ResumeGame()
    {
        PlayButtonSound();
        SetActivePanel(GameState.Gameplay);
        Time.timeScale = 1f;
        isPaused = false;

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

    private void SetActivePanel(GameState newState)
    {
        // Hide all panels
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (gameplayPanel) gameplayPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);

        UpdateBackgroundsForState(newState);

        bool shouldBlockInput = (newState == GameState.Paused || newState == GameState.Settings || newState == GameState.GameOver);

        if (overlayBlocker) overlayBlocker.SetActive(shouldBlockInput);

        if (shouldBlockInput) GameInputManager.SetPaused(true);
        else if (newState == GameState.Gameplay) GameInputManager.SetPaused(false);

        switch (newState)
        {
            case GameState.MainMenu:
                if (mainMenuPanel) mainMenuPanel.SetActive(true);
                break;
            case GameState.Gameplay:
                if (gameplayPanel) gameplayPanel.SetActive(true);
                break;
            case GameState.GameOver:
                if (gameOverPanel) gameOverPanel.SetActive(true);
                break;
            case GameState.Settings:
                if (settingsPanel) settingsPanel.SetActive(true);
                break;
            case GameState.Paused:
                if (pausePanel) pausePanel.SetActive(true);
                break;
        }

        currentState = newState;

        // FIXED: Update continue button whenever state changes
        UpdateContinueButton();

        Debug.Log($"🎯 UI State: {newState}, Input Blocked: {shouldBlockInput}");
    }

    private void UpdateBackgroundsForState(GameState newState)
    {
        bool showMainMenuBg = (newState == GameState.MainMenu) || (newState == GameState.Settings && !isInGameSettings);
        bool showGameplayBg = (newState == GameState.Gameplay) || (newState == GameState.Paused) || (newState == GameState.GameOver) || (newState == GameState.Settings && isInGameSettings);

        if (mainMenuBackground) mainMenuBackground.SetActive(showMainMenuBg);
        if (gameplayBackground) gameplayBackground.SetActive(showGameplayBg);
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearAllCards();
        }

        // Delete save file on game completion
        if (JsonSaveLoadManager.Instance != null)
        {
            JsonSaveLoadManager.Instance.DeleteSave();
        }

        ShowGameOver(stats);
        Debug.Log("🎉 Game completed - cards cleared, save deleted");
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
