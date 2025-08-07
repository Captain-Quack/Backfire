using UnityEngine;

public class Linerer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false; 
    }
    void Update()
    {
        
    }
}
