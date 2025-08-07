using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;
using Backfire;
using TMPro;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using FMODUnity;

public sealed class NewUIManager : MonoBehaviour
{
    public static NewUIManager Instance;

    public SceneField levelSelect;
    public SceneField configScene;
    public SceneField titleScene;

    public GameObject gameScreen;
    public GameObject instructionScreen;
    public GameObject settingsScreen;
    public GameObject winScreen;
    public GameObject titleScreen;
    public GameObject configScreen;
    public GameObject levelScreen;

    private float _timeScale;
    public bool isSettingsOpen;

    private enum MenuOrigin { Title, LevelSelect }
    private MenuOrigin _lastMenuOrigin = MenuOrigin.Title;
    private GameObject instructionText;


    private const string LevelScenePrefix = "Level ";
    private const string LevelSelectSceneKey = "LevelSelect";
    private const string TitleSceneKey = "Title";
    private const string ConfigSceneKey = "Config";


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (!gameScreen) gameScreen = GameObject.Find("GameScreen");
        if (!instructionScreen) instructionScreen = GameObject.Find("InstructionScreen");
        if (!settingsScreen) settingsScreen = GameObject.Find("SettingsScreen") ?? GameObject.Find("SettingScreen");
        if (!winScreen) winScreen = GameObject.Find("WinScreen");
        if (!titleScreen) titleScreen = GameObject.Find("TitleScreen");
        if (!configScreen) configScreen = GameObject.Find("ConfigScreen");
        if (!levelScreen) levelScreen = GameObject.Find("LevelScreen");
        if (!instructionText) instructionText = GameObject.Find("instructionText");
        if (!gameScreen || !instructionScreen || !settingsScreen || !winScreen || !titleScreen || !configScreen || !levelScreen)
        {
            Debug.LogWarning("NewUIManager: One or more UI screen objects not found. Ensure all UI screens are present in the scene.");
        }
    }

    private bool starting = true;
    private void Start()
    {
        isSettingsOpen = false;

        ShowAll(false);
        UpdateUIForScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
    }

    private void UpdateUIForScene(string sceneName)
    {
        if (sceneName.Contains(LevelSelectSceneKey))
        {
            ShowScreens(levelScreen);
        }
        else if (sceneName.Contains(LevelScenePrefix))
        {
            ShowScreens(gameScreen, instructionScreen);
            UpdateInstructionScreen();

            _timeScale = Time.timeScale;
            Time.timeScale = 0f;
            isSettingsOpen = true;
        }
        else if (sceneName.Contains(TitleSceneKey))
        {
            ShowScreens(titleScreen);
        }
        else if (sceneName.Contains(ConfigSceneKey))
        {
            ShowScreens(configScreen);
        }
        else
        {
            ShowAll(false);
        }

        if (sceneName.Contains(TitleSceneKey) || sceneName.Contains(LevelSelectSceneKey) || sceneName.Contains(ConfigSceneKey))
        {
            isSettingsOpen = false;
            Time.timeScale = 1f;
        }

        starting = false;
    }
    private void ShowScreens(params GameObject[] screensToShow)
    {
        ShowAll(false);
        foreach (GameObject screen in screensToShow)
        {
            if (screen)
            {
                screen.SetActive(true);
                StartCoroutine(DisableAndReenableButtons(screen, 0.1f));
            }
        }
    }


    private IEnumerator DisableAndReenableButtons(GameObject root, float delay)
    {
        var buttons = root.GetComponentsInChildren<Button>();
        var originalTexts = new Dictionary<Button, string>();

        foreach (var b in buttons)
        {
            b.interactable = false;
            TextMeshProUGUI tmp = b.GetComponentInChildren<TextMeshProUGUI>(true);
            //print(tmp.text);
             /*if (tmp != null)
            {
                originalTexts.Add(b, tmp.text);
                tmp.text = "Loading...";
                
            }*/
        }

        yield return new WaitForSecondsRealtime(0.2f);

        // float start = Time.realtimeSinceStartup;
        // while (Time.realtimeSinceStartup - start < delay)
        //     yield return null;

        foreach (var b in buttons.Where(b => b))
        {
            b.interactable = true;
            Debug.Log(b.name);
            
            TextMeshProUGUI tmp = b.GetComponentInChildren<TextMeshProUGUI>(true);

            if (tmp != null && originalTexts.TryGetValue(b, out var text))
                tmp.text = text;
        }
    }

    private void Update()
    {
        bool overlayActive = (instructionScreen.activeInHierarchy ||
                              settingsScreen.activeInHierarchy ||
                              winScreen.activeInHierarchy);
        if (Keyboard.current.escapeKey.wasPressedThisFrame && GameManager.Instance.GameIsRunning)
        {
            SettingsButton();
        }
        if (overlayActive && !isSettingsOpen)
        {
            _timeScale = Time.timeScale;
            isSettingsOpen = true;
            Time.timeScale = 0f;
            if (gameScreen.activeInHierarchy)
            {
                gameScreen.SetActive(false);
            }
        }
        else if (!overlayActive && isSettingsOpen)
        {
            isSettingsOpen = false;
            Time.timeScale = _timeScale;
            if (SceneManager.GetActiveScene().name.Contains(LevelScenePrefix))
            {
                gameScreen.SetActive(true);
            }
        }


    }

    private void StartGame()
    {
        titleScreen?.SetActive(false);
        gameScreen?.SetActive(true);
        instructionScreen?.SetActive(false);

    }

    public void ShowAll(bool yippee)
    {
        gameScreen.SetActive(yippee);
        instructionScreen.SetActive(yippee);
        settingsScreen.SetActive(yippee);
        winScreen.SetActive(yippee);
        titleScreen.SetActive(yippee);
        configScreen.SetActive(yippee);
        levelScreen.SetActive(yippee);
    }

    public void UpdateInstructionScreen()
    {
        Image gunImage = GameObject.Find("gunIcon")?.GetComponent<Image>();
        if (instructionText && gunImage && LevelSelectManager.Instance && GunSpawner.Instance)
        {
            bool noHighScore =
                Mathf.Approximately(LevelSelectManager.Instance.CurrentLevel.HighScore, float.MaxValue) ||
                Mathf.Approximately(LevelSelectManager.Instance.CurrentLevel.HighScore, 0f);
            string highScore =  noHighScore ? "--" : LevelSelectManager.Instance.CurrentLevel.HighScore.ToString("F2");
            string instructions = $"Personal Best: {highScore}\n" +
                                  $"Gun Selected: {GunSpawner.Instance.curentGunSelected.name ?? "The Inspector"}\n" +
                                  $"Deaths: {LevelSelectManager.Instance.CurrentLevel.Deaths}\n";
            instructionText.GetComponentInChildren<TextMeshProUGUI>().text = instructions;
            gunImage.sprite = GunSpawner.Instance.curentGunSelected.sprite;
        }
        else
        {
            Debug.LogWarning("UpdateInstructionScreen: Missing UI elements or manager instances for updating instructions.");
        }
    }
    public void ExitButton()
    {

        if (!SceneManager.GetActiveScene().name.Contains("Title"))
        {
            StartCoroutine(LoadSceneAsync("TitleScene"));
            return;
        }
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void LevelSelectButton()
    {
        Debug.Log("LevelSelectButton pressed: loading Level Select scene.");
        _timeScale = Time.timeScale;
        Time.timeScale = 0f;
        isSettingsOpen = true;
        ShowAll(false);
        StartCoroutine(LoadSceneAsync(levelSelect)); ;
        levelScreen?.SetActive(true);
    }
    public void SettingsButton()
    {
        ShowAll(false);
        settingsScreen?.SetActive(true);
        _timeScale = Time.timeScale;
        Time.timeScale = 0f;
        isSettingsOpen = true;
        if (FindAnyObjectByType<PlayerController>() is { currentState: PreState })
        {
            GameObject.Find("settingsResumeButton").transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Play";
            _timeScale = 1f;
        }
        else
        {
            GameObject.Find("settingsResumeButton").transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Resume";
        }
    }
    public void InstructionPlayButton()
    {
        Debug.Log("InstructionPlayButton pressed: closing instructions and starting level.");
        instructionScreen?.SetActive(false);
        isSettingsOpen = false;
        Time.timeScale = _timeScale > 0f ? _timeScale : 1f;
        if (SceneManager.GetActiveScene().name.Contains(LevelScenePrefix))
        {
            gameScreen?.SetActive(true);
            StartGame();
        }
    }
    public void ConfigButton()
    {
        Debug.Log("ConfigButton pressed: loading Config scene.");
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene.Contains(LevelSelectSceneKey))
            _lastMenuOrigin = MenuOrigin.LevelSelect;
        else
            _lastMenuOrigin = MenuOrigin.Title;
        ShowAll(false);
        configScreen?.SetActive(true);
        StartCoroutine(LoadSceneAsync(configScene));
    }
    public void ResumeButton()
    {
        
        isSettingsOpen = false;
        settingsScreen.SetActive(false);
        GameManager.Instance.GameIsRunning = true;
        gameScreen?.SetActive(true);

        Time.timeScale = _timeScale;
    }


    public void BackButton()
    {
        Debug.Log("BackButton pressed: returning to previous menu.");
        ShowAll(false);
        if (_lastMenuOrigin == MenuOrigin.Title)
        {
            StartCoroutine(LoadSceneAsync(titleScene));
        }
        else if (_lastMenuOrigin == MenuOrigin.LevelSelect)
        {
            StartCoroutine(LoadSceneAsync(levelSelect));
        }
    }

    public void ForceOpenConsole()
    {
#if UNITY_EDITOR

        System.Type consoleType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
        EditorWindow window = EditorWindow.GetWindow(consoleType);
        window.Show();
#endif
    }

   public void NextLevel()
{
    if (winScreen) winScreen.SetActive(false);
    var lsm = LevelSelectManager.Instance;
    if (lsm == null) { Debug.LogWarning("NextLevel: No LSM.");  return; }

    var next = lsm.areaData.NextLevel(lsm.CurrentLevel.LevelID);
    if (next == null) { Debug.LogWarning("NextLevel: Already at last level."); return; }

    lsm.UnlockLevel(next.LevelID);
    lsm.CurrentLevel = next;
    ShowScreens(gameScreen, instructionScreen);
    UpdateInstructionScreen();
    Time.timeScale = 0f;
    isSettingsOpen = true;

    Debug.Log($"Loading next level: {next.LevelID} – {next.LevelName}");
    StartCoroutine(LoadSceneAsync(next.scene));
}

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode modsce)
    {
        if (starting) return;
        UpdateUIForScene(scene.name);
        
        var go = GameObject.Find("gameEnemyText").GetComponentInChildren<TextMeshProUGUI>(true);

    }

    public IEnumerator LoadSceneAsync(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName);
    }

    public void Click()
    {
        RuntimeManager.PlayOneShot("event:/SFX/UI/Click");
    }
    public void Cancel()
    {
        RuntimeManager.PlayOneShot("event:/SFX/UI/Cancel");
    }
    public void SettingsClick()
    {
        RuntimeManager.PlayOneShot("event:/SFX/UI/Settings Click");
    }
    public void ResetClick()
    {
        RuntimeManager.PlayOneShot("event:/SFX/UI/Reset Click");
    }
}
