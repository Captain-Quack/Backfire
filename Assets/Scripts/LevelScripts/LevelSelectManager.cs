using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Backfire;
using FMODUnity;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class LevelSelectManager : MonoBehaviour
{
    [Header("Scene References")] 
    public Transform levelParent;

    public GameObject levelButtonPrefab;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;
    public Image background; 

    [Header("Content")] 
    public AreaData areaData;
    private readonly HashSet<string> _unlocked = new();
    private readonly List<Button> _buttons = new();
    private Camera _cam;
    

    public static LevelSelectManager Instance { get; internal set; }

    public  LevelData CurrentLevel; 

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        if (SceneManager.GetActiveScene().name.Contains("Level "))
        {
            Match match =  Regex.Match(SceneManager.GetActiveScene().name, @"Level.*(\d+)");
            int number =  int.Parse(match.Groups[1].Value);
            CurrentLevel = areaData.Levels[number - 1];
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _cam = Camera.main;
        GunSpawner.Instance.curentGunSelected ??= Resources.Load<GunData>("Guns/00 Glock 17");
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        if (!scene.name.Contains("LevelSelect")) return;
        
        print("loaded...");

        levelParent = GameObject.Find("Layout").transform;
        Debug.Assert(levelParent.childCount == 8);  

        levelText = GameObject.Find("worldText")?.GetComponent<TextMeshProUGUI>()  ?? FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.InstanceID)[1];
        scoreText = GameObject.Find("timeText")?.GetComponent<TextMeshProUGUI>() ?? FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.InstanceID)[0];
        // background = GameObject.Find("Background")?.GetComponent<Image>();
        // if(background is not null) background.sprite = areaData.background;
            
        GameObject.Find("RESET")?.GetComponent<Button>().onClick.AddListener(ResetUnlock);
        background = FindAnyObjectByType<Image>();
        CacheUnlockedLevels();  
        OnLevelHovered(null);
    }

    public void Start()
    {
        CacheUnlockedLevels();
        SpawnButtons();

        OnLevelHovered(CurrentLevel);
    }
    
    public void UnlockLevel(string levelID)
    {
        if (areaData == null)
        {
            Debug.LogError("AreaData is null!");
            return;
        }

        if (_buttons.Count == 0)
        {
            Debug.LogWarning("Buttons not spawned yet, attempting to spawn...");
            SpawnButtons();
        }

        if (!_unlocked.Add(levelID)) return;

        PlayerPrefs.SetInt($"LevelUnlocked_{levelID}", 1);
        PlayerPrefs.Save();

        for (int i = 0; i < areaData.Levels.Count; i++)
        {
            var lvl = areaData.Levels[i];
            if (lvl.LevelID != levelID) continue;

            if (i >= _buttons.Count)
            {
                Debug.LogError($"No button at index {i} for level {levelID}.");
                return;
            }

            var btn = _buttons[i];
            if (btn == null)
            {
                Debug.LogError($"Button is null at index {i} for level {levelID}.");
                return;
            }

            var levelButton = btn.GetComponent<LevelButton>();
            if (levelButton == null)
            {
                Debug.LogError($"Button for {levelID} is missing LevelButton component.");
                return;
            }

            btn.interactable = true;
            levelButton.Initialize(lvl, true);
            break;
        }
    }


    void CacheUnlockedLevels()
    {
        foreach (var lvl in areaData.Levels.Where(lvl => lvl.isUnlockedByDefault
                                                         || PlayerPrefs.GetInt($"LevelUnlocked_{lvl.LevelID}", 0) == 1))
        {
            _unlocked.Add(lvl.LevelID);
        }
    }


        void SpawnButtons()
        {
            _buttons.Clear();
            for (var index = 0; index < areaData.Levels.Count; index++)
            {
                var lvl = areaData.Levels[index];
                var lvlTransformSpot = levelParent.GetChild(index); 
                
                var go = Instantiate(levelButtonPrefab, lvlTransformSpot);
                
                _buttons.Add(go.GetComponent<Button>());
                lvl.levelButton = go;
                go.name = lvl.LevelID;
                go.GetComponent<LevelButton>().Initialize(lvl, _unlocked.Contains(lvl.LevelID));
            }
        }

    void SelectFirstUnlocked()
    {
        /*foreach (var b in _buttons)
            if (b.interactable)
            {
                EventSystem.current.SetSelectedGameObject(b.gameObject);
                OnLevelHovered(areaData.Levels[_buttons.IndexOf(b)]);
                break;
            }*/
        foreach (var b in _buttons)
        {
            b.interactable = true;
            {
                EventSystem.current.SetSelectedGameObject(b.gameObject);
                OnLevelHovered(areaData.Levels[_buttons.IndexOf(b)]);
            }
        }
    }

    public void OnLevelHovered([CanBeNull] LevelData lvl)
    {
        if (levelText)
        {
            
            levelText.text = lvl?.LevelName ?? areaData.AreaName;
            CurrentLevel = lvl ?? CurrentLevel;
            
            if (lvl is null) scoreText.text = areaData.AreaTime.ToString("F2", CultureInfo.InvariantCulture);
            else if (Mathf.Approximately(lvl.HighScore, float.MaxValue)) scoreText.text = "--";
            else scoreText.text = lvl.HighScore.ToString("F2", CultureInfo.InvariantCulture);
        }
        else
        {
            // something might be missing...
            levelParent = GameObject.Find("Layout").transform;
            levelText = GameObject.Find("worldText")?.GetComponent<TextMeshProUGUI>()  ?? FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.InstanceID)[1];
            scoreText = GameObject.Find("timeText")?.GetComponent<TextMeshProUGUI>() ?? FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.InstanceID)[0];
            background = GameObject.Find("Background")?.GetComponent<Image>();
        }
    }
    
    public void ResetUnlock()
    {
        Debug.Log("Resetting unlocks...");

        if (areaData == null)
        {
            Debug.LogError("AreaData is null. Cannot reset.");
            return;
        }

        if (levelParent == null)
        {
            Debug.LogError("LevelParent is null. Cannot reset.");
            return;
        }
        _unlocked.Clear();
        _buttons.Clear();

        foreach (Transform child in levelParent)
            Destroy(child.transform.GetChild(0).gameObject);
        
        foreach (var level in areaData.Levels)
        {
            PlayerPrefs.SetInt($"LevelUnlocked_{level.LevelID}", 0);
            level.ResetScore();
        }
        
        foreach (var gun in Resources.LoadAll<GunData>("Guns").Where(g => !g.isUnlockedByDefault))
        {
            gun.Lock();
        }

        if (GunSpawner.Instance != null)
            GunSpawner.Instance.Respawn();
        else
            Debug.LogWarning("GunSpawner.Instance is null. Cannot respawn gun.");

        PlayerPrefs.Save();
        CacheUnlockedLevels();
        SpawnButtons();
        SelectFirstUnlocked();

        Debug.Log("Unlocks reset successfully.");
    }


}
