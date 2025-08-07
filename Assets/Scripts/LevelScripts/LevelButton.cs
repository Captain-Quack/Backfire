using System.Collections;
using Backfire;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

[RequireComponent(typeof(Button), typeof(Image))]
public sealed class LevelButton : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler,
    ISelectHandler, IDeselectHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual Settings")]
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private float popScale = 1.08f;
    [SerializeField] private float animTime = 0.08f;
    [SerializeField] private Color lockedColor = new(0.5f, 0.5f, 0.5f);

    private LevelData _level;
    private Button _button;
    private Image _image;
    private Vector3 _baseScale;
    private Coroutine _scaleCoroutine;
    private bool _isInitialized;

    public LevelData Level => _level;
    public bool IsUnlocked => _button.interactable;

    void Awake()
    {
        CacheComponents();
    }

    void CacheComponents()
    {
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();
        _baseScale = transform.localScale;
    }

    public void Initialize(LevelData level, bool unlocked)
    {
        if (_isInitialized) return;
        
        _level = level;
        _isInitialized = true;
        label.SetText(level.LevelID);
        SetUnlockedState(unlocked);
        _button.onClick.AddListener(LoadLevel);
    }

    public void SetUnlockedState(bool unlocked)
    {
        _image.color = unlocked ? Color.white : lockedColor;

        _button.interactable = unlocked;
    }

    #region Event Handlers
    public void OnPointerDown(PointerEventData _)
    {
        TriggerPop();
        RuntimeManager.PlayOneShot("event:/SFX/UI/Level Select");
    }
    public void OnPointerUp(PointerEventData _) => TriggerUnPop();

    public void OnSelect(BaseEventData _) => HandleHoverStart();
    public void OnDeselect(BaseEventData _) => HandleHoverEnd();

    public void OnPointerEnter(PointerEventData _) => HandleHoverStart();
    public void OnPointerExit(PointerEventData _) => HandleHoverEnd();

    private void HandleHoverStart()
    {
        NotifyLevelHovered(_level);
        if (_button.interactable) TriggerPop();
    }

    private void HandleHoverEnd()
    {
        NotifyLevelHovered(null);
        TriggerUnPop();
    }
    #endregion

    #region Animation
    private void TriggerPop()
    {
        AnimateToScale(_baseScale * popScale);
    }

    private void TriggerUnPop()
    {
        AnimateToScale(_baseScale);
    }

    private void AnimateToScale(Vector3 targetScale)
    {
        if (_scaleCoroutine != null)
            StopCoroutine(_scaleCoroutine);
        
        _scaleCoroutine = StartCoroutine(ScaleToCoroutine(targetScale));
    }

    private IEnumerator ScaleToCoroutine(Vector3 target)
    {
        var startScale = transform.localScale;
        var elapsed = 0f;

        while (elapsed < animTime)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = elapsed / animTime;
            transform.localScale = Vector3.Lerp(startScale, target, progress);
            yield return null;
        }

        transform.localScale = target;
        _scaleCoroutine = null;
    }
    #endregion

    #region Level Management
    private void LoadLevel()
    {
        if (_level == null) return;
        Debug.Log($"Loading level {_level.LevelID}");
        var manager = LevelSelectManager.Instance;
        if (manager != null)
            manager.CurrentLevel = _level;
        StartCoroutine(NewUIManager.Instance.LoadSceneAsync(_level.scene));
        ConfigureUIForLevelStart();
        NewUIManager.Instance.UpdateInstructionScreen();
    }

    private void ConfigureUIForLevelStart()
    {
        var uiManager = NewUIManager.Instance;
        if (!uiManager) return;

        uiManager.ShowAll(false);
        uiManager.gameScreen.SetActive(true);
        uiManager.instructionScreen.SetActive(true);
        uiManager.gameObject.SetActive(true);
    }

    private void NotifyLevelHovered(LevelData level)
    {
        LevelSelectManager.Instance?.OnLevelHovered(level);
        if (_button.interactable)
        {
            RuntimeManager.PlayOneShot("event:/SFX/UI/Mouse Over");
        }
    }
    #endregion

    void OnDestroy()
    {
        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = null;
        }
    }
}
