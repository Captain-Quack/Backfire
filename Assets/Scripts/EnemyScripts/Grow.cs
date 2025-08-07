using UnityEngine;

public sealed class Grow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float scaleSpeed = 0.01f;
    public float transparencySpeed = 0.01f;
    private SpriteRenderer spriteRenderer;
    private float targetTransparency; 
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 scale = transform.localScale;
        scale.x += scaleSpeed*Time.deltaTime;
        transform.localScale = scale;
        Color color = spriteRenderer.color;
        color.a += transparencySpeed *Time.deltaTime;
        spriteRenderer.color = color;
    }
}
