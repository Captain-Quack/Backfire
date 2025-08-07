using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveManager : MonoBehaviour
{
    public static ShockwaveManager Instance { get; private set; }

    [SerializeField] private float shockWaveTime = 1.4f;
    [SerializeField] private float shockWaveMaxRadius = 10f;
    [SerializeField] private LayerMask enemyLayer;

    Coroutine _shockWaveRoutine;
    private Material _material;
    private SpriteRenderer _spriteRenderer;
    

    private static readonly int WaveDistanceID  = Shader.PropertyToID("_WaveDistance");
    Vector2 _originWs;
    
    readonly Collider2D[] _hitCache = new Collider2D[32];
    readonly HashSet<Collider2D> _alreadyHit = new(); 

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;

        _spriteRenderer.enabled = false;
        _material.SetFloat(WaveDistanceID, -0.1f);
    }
    public void CallShockWave(Vector2 origin)
    {
        _originWs = origin;
        _spriteRenderer.enabled = true;
        _alreadyHit.Clear();                    

        if (_shockWaveRoutine != null) StopCoroutine(_shockWaveRoutine);
        _shockWaveRoutine = StartCoroutine(WaveRoutine());
    }

    IEnumerator WaveRoutine()
    {
        float elapsed = 0f;
        _material.SetFloat(WaveDistanceID, -0.1f);

        while (elapsed < shockWaveTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shockWaveTime;
            float shaderT = Mathf.Lerp(-0.1f, 1f, t); 
            float currentRadius = Mathf.Lerp(0, shockWaveMaxRadius, t);

            _material.SetFloat(WaveDistanceID, shaderT);
            
            int n = Physics2D.OverlapCircleNonAlloc(_originWs, currentRadius, _hitCache, enemyLayer);

            for (int i = 0; i < n; ++i)
            {
                var col = _hitCache[i];
                if (!_alreadyHit.Add(col)) continue;
                if (col.TryGetComponent(out EnemyController enemy))
                {
                   Destroy(enemy.gameObject);                
                }
            }
            yield return null;
        }
        _spriteRenderer.enabled = false;
        _material.SetFloat(WaveDistanceID, -0.1f);
    }
}