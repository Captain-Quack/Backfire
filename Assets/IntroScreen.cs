using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroScreen : MonoBehaviour
{
    
    IEnumerator Start()
    {
        VideoPlayer videoPlayer = GetComponent<VideoPlayer>();
        AudioSource audioSource = GetComponent<AudioSource>();
        videoPlayer.EnableAudioTrack(0, true);
        
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
        yield return new WaitForSecondsRealtime(7f);
        SceneManager.LoadScene(1);
    }
}
