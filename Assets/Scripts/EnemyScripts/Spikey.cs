using System;
using System.Collections;
using UnityEngine;

public sealed class Spikey : MonoBehaviour
{
    public float distance = 1f;
    public float moveSpeed = 2f;

    private Vector3 direction;
    private Vector3 startPosition;
    private float timer;
    private float dissapear = 5f;

    void Start()
    {
        StartCoroutine(Move());
        
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > dissapear)
        {
            Destroy(gameObject);
        }
    }
    private IEnumerator Move()
    {
        float angle = transform.eulerAngles.z;
        direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f).normalized;
        startPosition = transform.position;
        Vector3 targetPosition = startPosition + direction * distance;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
    }
    public IEnumerator Launch()
    {
        Debug.Log("???");
        float angle = transform.eulerAngles.z;
        direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f).normalized;
        startPosition = transform.position;
        Vector3 targetPosition = startPosition + direction * 1000f;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 50f * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
    }
}