using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private HashSet<GameObject> portalObjects = new HashSet<GameObject>();
    [SerializeField] private Transform destination;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (portalObjects.Contains(other.gameObject))
        {
            return;
        }

        if (destination.TryGetComponent(out Portal destinationportal))
        {
            destinationportal.portalObjects.Add(other.gameObject);
        }
        other.transform.position = destination.position;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        portalObjects.Remove(other.gameObject);
    }
}
