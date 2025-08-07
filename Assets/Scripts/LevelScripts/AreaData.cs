using System.Collections.Generic;
using System.Linq;
using Backfire;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "AreaData", menuName = "AreaData", order = 1)]

public sealed class AreaData : ScriptableObject
{
    
    public string AreaName;
    public List<LevelData> Levels = new();
    [CanBeNull] public AreaData NextArea;
    public Sprite background;
    public float AreaTime => Levels.Select(static level => level.HighScore).Where(static score => score < float.MaxValue / 10).Sum();
    public const float EightMinutes = 60f * 8f;

    public LevelData NextLevel(string levelId)
    {
        int currentIndex = Levels.FindIndex(l => l.LevelID == levelId);
        if (currentIndex < Levels.Count - 1)
        {

            Debug.Log("The next level is " + Levels[currentIndex + 1].LevelID);
            return Levels[currentIndex + 1];
        }
        Resources.Load<GunData>("Guns/Sniper").Unlock();
        if (AreaTime > EightMinutes)
        {
            Resources.Load<GunData>("Guns/M72 LAW").Unlock();
            
        }
        Debug.LogWarning($"Level {levelId} not found in area {AreaName}, Proceeding to next area.");
        return NextArea?.Levels[0];
    }
    
    
    
    
}
