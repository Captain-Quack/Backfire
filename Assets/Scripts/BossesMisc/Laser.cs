using UnityEngine;

public class DrawLine : MonoBehaviour
{
    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();

        
        line.positionCount = 2;
        line.SetPosition(0, new Vector3(0, 0, 0));
        line.SetPosition(1, new Vector3(1, 1, 0));
    }
}
