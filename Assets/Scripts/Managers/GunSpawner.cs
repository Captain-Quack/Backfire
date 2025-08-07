using System;
using System.Collections.Generic;
using System.Linq;
using Backfire;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using FMODUnity;

[RequireComponent(typeof(HorizontalLayoutGroup))]
public sealed class GunSpawner : MonoBehaviour
{

    public static GunSpawner Instance { get; private set; }
    public GameObject gunButtonPrefab;
    [SerializeField] public GunData curentGunSelected;
    
    [SerializeField] private List<GunData> guns = new();
    private HorizontalLayoutGroup _layoutGroup;
    private InfomationPanel _infomationPanel;

    void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        _layoutGroup = GetComponent<HorizontalLayoutGroup>();
        _infomationPanel = new InfomationPanel(); 
        
        FetchAllGuns();
        SpawnGunButtons();
        
        _infomationPanel.SetupInfoPanel();
        _infomationPanel.ToggleInfoPanel(false);
        
        curentGunSelected = Resources.Load<GunData>("00 Guns/Glock 17");
        
    }

    public void FetchAllGuns()
    {
        guns.AddRange(Resources.LoadAll<GunData>("Guns"));
        guns.Where(x => x.isUnlockedByDefault).ToList().ForEach(x => x.Unlock());
    }

    private void SpawnGunButtons()
    {
        foreach (GunData gunData in guns)
        {
            var button = Instantiate(gunButtonPrefab, _layoutGroup.transform).transform;
            var image = button.GetChild(0).GetComponent<Image>();
            image.sprite = gunData.sprite;
            if (gunData.IsUnlocked)
            {
                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    curentGunSelected = gunData;
                    RuntimeManager.PlayOneShot("event:/SFX/UI/Loadout");
                });
            }
            else
            {
                image.color = Color.gray;
            }

            AssignEventsToButton(button, gunData);
        }
    }

    private void AssignEventsToButton(Transform button, GunData gunData)
    {
        var eventTrigger = button.gameObject.AddComponent<EventTrigger>();
        var pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };

        pointerEnter.callback.AddListener((_) =>
        {
            RuntimeManager.PlayOneShot("event:/SFX/UI/Mouse Over");
            _infomationPanel.ToggleInfoPanel(true, gunData);
        });
        pointerExit.callback.AddListener((_) =>
        {
            _infomationPanel.ToggleInfoPanel(false);
        });
            
        eventTrigger.triggers.Add(pointerEnter);
        eventTrigger.triggers.Add(pointerExit);
    }
    public void Respawn()
    {
        foreach (Transform child in _layoutGroup.transform)
        {
            Destroy(child.gameObject);
        }

        guns.Clear();
        FetchAllGuns();
        curentGunSelected = Resources.Load<GunData>("00 Guns/Glock 17");
        SpawnGunButtons();
        _infomationPanel.ToggleInfoPanel(false);
    }


    [Serializable]
    private class InfomationPanel
    {
        private GameObject _infomationPanel;
        private Image _gunImage;
        private TextMeshProUGUI _gunName;
        private TextMeshProUGUI _cooldown;
        private TextMeshProUGUI _action;
        private TextMeshProUGUI _bulletName; 
        private TextMeshProUGUI _velocity;
        
        private const string InfoPanelName = "Info";
        private const string GunImageName = "GunImage";
        private const string GunNameName = "Gunname";
        private const string CooldownName = "Cooldown";
        private const string ActionName = "Action";
        private const string BulletName = "BulletName"; 
        private const string VelocityName = "Velocity";

        public void SetupInfoPanel()
        {
            _infomationPanel = GameObject.Find(InfoPanelName);
            _gunImage = GameObject.Find(GunImageName).GetComponent<Image>();
            _gunName = GameObject.Find(GunNameName)?.GetComponent<TextMeshProUGUI>();
            _cooldown = GameObject.Find(CooldownName)?.GetComponent<TextMeshProUGUI>();
            _action = GameObject.Find(ActionName)?.GetComponent<TextMeshProUGUI>();
            _bulletName = GameObject.Find(BulletName)?.GetComponent<TextMeshProUGUI>(); 
            _velocity = GameObject.Find(VelocityName)?.GetComponent<TextMeshProUGUI>();
        }
        
        public void ToggleInfoPanel(bool show, GunData gunData = null)
        {
            if (!_infomationPanel) return;

            gunData ??= Instance.curentGunSelected;

            if (gunData == null)
            {
                _infomationPanel.SetActive(false);
                return;
            }
            
            _gunImage.sprite = gunData.sprite;
            _gunName.text = gunData.name;

            if (gunData.IsUnlocked)
            {
                _cooldown.text = $"Cooldown\t{gunData.cooldown:F}";
                _action.text = $"Action\t{gunData.action}";
                _bulletName.text = $"Bullet\t{gunData.bulletData.name}";
                _velocity.text = $"Velocity\t{gunData.muzzleVelocityFPS:F}";
            }
            else
            {
                _cooldown.text = gunData.lockText;
                _action.text = "";
                _bulletName.text = "";
                _velocity.text = "";
            }
        }
    }

}
