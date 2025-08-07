using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "LevelStuff", menuName = "LevelData", order = 1)]
public sealed class LevelData : ScriptableObject
{
    [Header("Level Status")]
    public string LevelID;

    public bool isUnlockedByDefault;
    
    public SceneField scene;

    public GameObject levelButton;

    public string LevelName;
    private const string HighScoreKeyFormat = "HighScore_{0}";
    private const string GlobalDeathsFormat = " Deaths_{0}";
    
    public float HighScore => PlayerPrefs.GetFloat(GetPrefsKey(), float.MaxValue);
    public float Deaths { get =>  PlayerPrefs.GetFloat(GetDeathKey(), 0); set =>  PlayerPrefs.SetFloat(GetDeathKey(), value); }

    /// <summary>
    /// Only updates score if it's higher than the current high score.
    /// Returns if the score was updated.
    /// </summary>
    public bool RegisterScore(float score)
    {
        
        if (score > HighScore) return false;
        PlayerPrefs.SetFloat(GetPrefsKey(), score);
        PlayerPrefs.Save();
        return true;
    }
    
    public void ResetScore()
    {
        PlayerPrefs.DeleteKey(GetDeathKey());
        PlayerPrefs.DeleteKey(GetPrefsKey());
        PlayerPrefs.Save();
    }

    private string GetPrefsKey()
    {
        return string.Format(HighScoreKeyFormat, LevelID);
    }

    private string GetDeathKey()
    {
        return string.Format(GlobalDeathsFormat, LevelID);
    }
}




[System.Serializable]
public class SceneField
{
    [SerializeField]
    private Object m_SceneAsset;

    [SerializeField]
    private string m_SceneName = "";
    public string SceneName
    {
        get { return m_SceneName; }
    }

    // makes it work with the existing Unity methods (LoadLevel/LoadScene)
    public static implicit operator string( SceneField sceneField )
    {
        return sceneField.SceneName;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneField))]
public class SceneFieldPropertyDrawer : PropertyDrawer 
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        EditorGUI.BeginProperty(_position, GUIContent.none, _property);
        SerializedProperty sceneAsset = _property.FindPropertyRelative("m_SceneAsset");
        SerializedProperty sceneName = _property.FindPropertyRelative("m_SceneName");
        _position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
        if (sceneAsset != null)
        {
            sceneAsset.objectReferenceValue = EditorGUI.ObjectField(_position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false); 

            if( sceneAsset.objectReferenceValue != null )
            {
                sceneName.stringValue = (sceneAsset.objectReferenceValue as SceneAsset).name;
            }
        }
        EditorGUI.EndProperty( );
    }
}
#endif