using System;
using System.Reflection;
using Backfire;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class LazyButtonReassignUIManager : MonoBehaviour
{
    [Tooltip("Name of a parameterless void method on GameManager.Instance")]
    public string NameOfFunctionInUIManager;
    
    private Button _button;
    private MethodInfo _methodInfo;

    void Start()
    {
        _button = GetComponent<Button>();
        
        _methodInfo = typeof(UIManager)
            .GetMethod(NameOfFunctionInUIManager, BindingFlags.Public | BindingFlags.Instance);
        if (_methodInfo == null)
            Debug.LogError($"[{nameof(LazyButtonReassignUIManager)}] “{NameOfFunctionInUIManager}” not found on GameManager.");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_button == null || _methodInfo == null) return;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => _methodInfo.Invoke(NewUIManager.Instance, null));
    }
}