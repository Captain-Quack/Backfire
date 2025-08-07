using System;
using System.Collections;
using System.Collections.Generic;
using Backfire;
using FMOD.Studio;
using FMODUnity;
using FMODUnityResonance;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Backfire
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Player & UI")]
        [SerializeField] private PlayerController player;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Spawn & High Scores")]
        // [SerializeField] private Transform spawnPoint;
        [SerializeField] private float highScore1;
        [SerializeField] private float highScore2;
        [SerializeField] private float highScore3;

        [Header("High-Score UI")]
        [SerializeField] private TextMeshProUGUI firstScoreText;
        [SerializeField] private TextMeshProUGUI secondScoreText;
        [SerializeField] private TextMeshProUGUI thirdScoreText;

        private float _timeElapsed;
        private bool _winPending;

        public int numEnemies;
        public int numEnemiesKilled;

        public bool GameIsRunning { get; internal set; }

        private EventInstance lcSnapshotInst;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            CacheSceneReferences();
            if (SceneManager.GetActiveScene().name.Contains("Level ") || SceneManager.GetActiveScene().name.Contains("MasterScene")) 
                ResetGame();
            else if (SceneManager.GetActiveScene().name.Contains("Score"))
                ShowScoreScene();
            else
                ResetGame();

            numEnemies = 0;
            numEnemiesKilled = 0;
        }

        public void AddEnemy()
        {
            if (!SceneManager.GetActiveScene().name.Contains("Level ") &&
                !SceneManager.GetActiveScene().name.Contains("MasterScene")) return;
            numEnemies++;
            var go = GameObject.Find("gameEnemyText").GetComponentInChildren<TextMeshProUGUI>(true);
            go.text = $"Enemies: {numEnemiesKilled} / {numEnemies}";
        }

        public void AddEnemiesKilled()
        {
            numEnemiesKilled++;
            Debug.Log("Num enemies killed: " + numEnemiesKilled);
            var go = GameObject.Find("gameEnemyText").GetComponentInChildren<TextMeshProUGUI>();
            GameIsRunning = true;
            go.text = $"Enemies: {numEnemiesKilled} / {numEnemies}"; 

        }

        
        private void OnDestroy()
        {
            if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            CacheSceneReferences();
            ResetGame();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CacheSceneReferences();
            if (scene.name.Contains("Level ") || scene.name.Contains("MasterScene")) ResetGame();
            else if (scene.name.Contains("Score")) ShowScoreScene();
            else if (scene.name.Contains("Title")) RuntimeManager.StudioSystem.setParameterByName("Menu Progress", 1);
            else if (scene.name.Contains("Start")) RuntimeManager.StudioSystem.setParameterByName("Menu Progress", 0);
            else ResetGame();
        }

        private void CacheSceneReferences()
        {
            player ??= GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
            timerText ??= GameObject.FindWithTag("Timer")?.GetComponent<TextMeshProUGUI>();
        }

        public void ResetGame()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f; 
            _timeElapsed = 0f;
            GameIsRunning  = true;
            _winPending    = false;

            numEnemies = 0;
            numEnemiesKilled = 0;

            timerText?.SetText("Time: 0.00");
            player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        }

        private void ShowScoreScene()
        {
            GameIsRunning = false;
            float finalTime = _timeElapsed;
            timerText?.SetText($"Time: {finalTime:F2}");

            // Get the current level's high score from LevelData
            var lsm = LevelSelectManager.Instance;
            if (lsm && lsm.CurrentLevel != null)
            {
                float highScore = lsm.CurrentLevel.HighScore;
                firstScoreText?.SetText($"Best: {highScore:F2}");
                secondScoreText?.SetText($"Current: {finalTime:F2}");
            }
        }

        private void Update()
        {
            if (NewUIManager.Instance.isSettingsOpen) return;

            if (GameIsRunning)
            {
                _timeElapsed += Time.unscaledDeltaTime;
                timerText?.SetText($"Time: {_timeElapsed:F2}");
            }
            if (!_winPending) return;
            _winPending = false;
            NewUIManager.Instance.winScreen.SetActive(true);
            GameIsRunning = false;
        }

        private IEnumerator WaitAndReload(float delay)
        {
            StartCoroutine(NewUIManager.Instance.LoadSceneAsync(SceneManager.GetActiveScene().name));
            yield return null;
        }

        private void ReloadScene()
        {
            StartCoroutine(NewUIManager.Instance.LoadSceneAsync(SceneManager.GetActiveScene().name));
        }

        public void WinGame()
        {
            GameIsRunning = false;
            player.SetState(PlayerStateType.PreState);

            var lsm = LevelSelectManager.Instance;
            if (lsm?.CurrentLevel == null)
            {
                Debug.LogError("No current level set in LevelSelectManager");
                return;
            }
            var cur = lsm.CurrentLevel;
            bool newBest = cur.RegisterScore(_timeElapsed);

            LevelCompleteSnapshotStart();
            RuntimeManager.PlayOneShot(newBest ? "event:/Music/PB Jingle" : "event:/Music/Win Jingle");
            Debug.Log(newBest
                ? $"New best for {cur.LevelID}"
                : "Score not high enough.");

            var ui = NewUIManager.Instance;
            ui.winScreen.SetActive(true);
            
            var winText = GameObject.Find("winText").GetComponent<TextMeshProUGUI>();
            var winTimeText =  GameObject.Find("winTimeText").GetComponent<TextMeshProUGUI>(); 
            

            if (winText != null)
                winText.text = newBest 
                    ? "Level Complete with New Best!" 
                    : "Level Complete!";
            else
                Debug.LogWarning("winText reference missing in NewUIManager.");

            if (winTimeText != null)
            {
                winTimeText.text = $"Time: {_timeElapsed:F2}";
                if (!newBest)
                    winTimeText.text += $"\nBest: {cur.HighScore:F2}";
            }
            else
            {
                Debug.LogWarning("winTimeText reference missing in NewUIManager.");
            }
            
            var next = lsm.areaData?.NextLevel(cur.LevelID);
            if (next != null)
                lsm.UnlockLevel(next.LevelID);
        }

        public void LevelCompleteSnapshotStart()
        {
            lcSnapshotInst = RuntimeManager.CreateInstance("snapshot:/Level Complete");
            lcSnapshotInst.start();
        }
        public void LevelCompleteSnapshotEnd()
        {
            lcSnapshotInst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            lcSnapshotInst.release();
        }
    }
}
