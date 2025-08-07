using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FMODUnity;

public class ShieldEnemy : MonoBehaviour
{
    private static readonly int Shield = Animator.StringToHash("Shield");

    [Header("The Shield")]
    [SerializeField] private GameObject shieldPrefab;

    private static readonly HashSet<GameObject> sShieldedEnemies = new(capacity: 32);

    [Header("Shield Logic")]
    [SerializeField, Min(0f)] private float _detectionRadius = 4f;
    [SerializeField, Min(0f)] private float _cooldown = 8f;
    [SerializeField] private LayerMask enemyLayer;

    private readonly HashSet<GameObject> _myShields = new();
    private readonly HashSet<GameObject> _myAllies = new();
    private Animator _animator;
    private float _elapsed = 0f;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        _elapsed += Time.deltaTime;
        if (_elapsed < _cooldown) return;
        _elapsed = 0f;

        TryCastShield();
    }

    private void TryCastShield()
    {
        if (shieldPrefab == null)
        {
            Debug.LogError($"{name}: Shield prefab is missing.");
            return;
        }

        var freshAllies = Physics2D
            .OverlapCircleAll(transform.position, _detectionRadius, enemyLayer)
            .Select(c => c.gameObject)
            .Where(go => !sShieldedEnemies.Contains(go))
            .ToList();

        if (freshAllies.Count == 0) return;

        _animator.SetTrigger(Shield);
        RuntimeManager.PlayOneShot("event:/SFX/Game/Shielder Shield");

        foreach (var ally in freshAllies)
        {
            var shield = Instantiate(shieldPrefab, ally.transform.position, Quaternion.identity, ally.transform);
            _myShields.Add(shield);
            _myAllies.Add(ally);
            sShieldedEnemies.Add(ally);
        }
    }

    private void OnDestroy()
    {
        foreach (var shield in _myShields)
            if (shield != null)
                Destroy(shield);

        _myShields.Clear();
        sShieldedEnemies.ExceptWith(_myAllies);
        _myAllies.Clear();
    }
}
