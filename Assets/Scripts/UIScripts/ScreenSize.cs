using UnityEngine;
using TMPro;
public class ScreenSize : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public void SetExclusiveFullScreen()
    {
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
    }

    public void SetBorderlessFullScreen()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    }

    public void HandleInputData(int val)
    {
        if(val == 0)
        {
            Debug.Log("Exclusive Fullscreen");
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else if(val == 1)
        {
            Debug.Log("Borderless Fullscreen");
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            
        }
        else if (val == 2)
        {
            Debug.Log("Windowed");
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.fullScreen = false;
            Screen.SetResolution(1280, 720, false);
        }
    }
}