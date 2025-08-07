using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;

public class VolumeSlider : MonoBehaviour
{
    public Slider musicSlider1;
    public Slider sfxSlider1;
    public Slider musicSlider2;
    public Slider sfxSlider2;

    private Bus musicBus;
    private Bus sfxBus;

    public static VolumeSlider Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        musicBus = RuntimeManager.GetBus("bus:/MUS");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");

        musicSlider1.onValueChanged.AddListener(SetMusicVolume);
        musicSlider2.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider1.onValueChanged.AddListener(SetSFXVolume);
        sfxSlider2.onValueChanged.AddListener(SetSFXVolume);
        
        musicSlider1.SetValueWithoutNotify(5f);
        musicSlider2.SetValueWithoutNotify(5f);
        sfxSlider1.SetValueWithoutNotify(5f);
        sfxSlider2.SetValueWithoutNotify(5f);
        
        musicBus.setVolume(.5f);
        sfxBus.setVolume(.5f);
    }

    public void SetMusicVolume(float volume) // there is no easy way to have the ping correctly set for music 
    {
        Debug.Log($"Music volume set to {volume:0.00}");
    
        musicSlider1.SetValueWithoutNotify(volume);
        musicSlider2.SetValueWithoutNotify(volume);
        musicBus.setVolume(volume / 10);
    }

    public void SetSFXVolume(float volume)
    {
        Debug.Log($"SFX volume set to {volume:0.00}");
        sfxBus.setVolume(volume / 10);

        sfxSlider1.SetValueWithoutNotify(volume);
        sfxSlider2.SetValueWithoutNotify(volume);

        PlayTestPing();
    }
    
    public void PlayTestPing()
    {
        RuntimeManager.PlayOneShot("event:/SFX/UI/Test Ping");
    }
    public void Ping(float satisfyUnity)
    {
        RuntimeManager.PlayOneShot("event:/SFX/UI/Test Ping");
    }
}
