using UnityEngine;

namespace Backfire
{
    [System.Serializable]

    [CreateAssetMenu(fileName = "GunData", menuName = "Gun Data")]

    public sealed class GunData : ScriptableObject
    {
        public float cooldown = 0.01f;

        public new string name;

        public GunAction action;

        public Sprite sprite;

        public BulletData bulletData;

        public float muzzleVelocityFPS; // for some reason the standard unit for this is feet/sec

        // https://smallarmsreview.com/le6920-colts-law-enforcement-carbine/
        public string sfxPath;

        public bool isUnlockedByDefault = false;
        public string lockText = "Locked...";
        public bool IsUnlocked => PlayerPrefs.GetInt($"{name}_Unlocked", 0) == 1;
        public void Unlock() => PlayerPrefs.SetInt($"{name}_Unlocked", 1);
        public void Lock() => PlayerPrefs.SetInt($"{name}_Unlocked", 0);
    }
}