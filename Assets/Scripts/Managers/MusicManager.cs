using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public sealed class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private EventInstance menuMusic;
    private EventInstance mainMusic;
    private EventInstance bossMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;

        menuMusic = RuntimeManager.CreateInstance("event:/Music/Menu");
        mainMusic = RuntimeManager.CreateInstance("event:/Music/Main");
        // bossMusic = RuntimeManager.CreateInstance("event:/Music/Boss");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        menuMusic.release();
        mainMusic.release();
        bossMusic.release();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        if (sceneName.Contains("Title") || sceneName.Contains("Config") || sceneName.Contains("LevelSelect"))
        {
            PlayIfStopped(menuMusic);
            StopIfPlaying(mainMusic);
            StopIfPlaying(bossMusic);
        }
        else if (sceneName.Contains("Level 8")) 
        {
            PlayIfStopped(bossMusic);
            StopIfPlaying(menuMusic);
            StopIfPlaying(mainMusic);
        }
        else if (sceneName.Contains("Level ")) 
        {
            PlayIfStopped(mainMusic);
            StopIfPlaying(menuMusic);
            StopIfPlaying(bossMusic);
        }
    }

    private void PlayIfStopped(EventInstance instance)
    {
        instance.getPlaybackState(out var state);
        if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STARTING)
        {
            instance.start();
        }
    }

    private void StopIfPlaying(EventInstance instance)
    {
        instance.getPlaybackState(out var state);
        if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING)
        {
            instance.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}
