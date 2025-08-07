using System;
using System.Collections;
using System.Collections.Generic;
using Backfire;
using UnityEngine;

public sealed class ArrowIndicator : MonoBehaviour
{
    private PlayerController player;
    public bool showing;
    public float maxMagnitude = 10f;
    public float scaleMultiplier = 0.06f;
    public SpriteRenderer spriteRenderer;
    public Transform arrowTransform;
    private Gradient velocityGradient;

    public void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        // add a new child with the arrow 
        var arrowObject = new GameObject("Arrow");
        arrowObject.transform.SetParent(transform);
        arrowObject.transform.localPosition = Vector3.zero;
        arrowObject.transform.localRotation = Quaternion.Inverse(Quaternion.identity);
        spriteRenderer = arrowObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Resources.Load<Sprite>("arrow-up-svgrepo-com");
        arrowTransform = arrowObject.transform;
        InitializeGradient();
        spriteRenderer.enabled = false;
    }

    public void Update()
    {
        transform.position = player.transform.position;
        if (player.currentState is not PlayingState { isSlowMotion: true } playingState || showing) return;
        showing = true;
        StartCoroutine(ShowVelocityArrow(playingState));
    }

    private IEnumerator ShowVelocityArrow(PlayingState playingState)
    {
        spriteRenderer.enabled  = true;
        while (playingState.isSlowMotion)
        {
            Vector2 vel = player.body.linearVelocity;
            Vector2 direction = vel.normalized;
            float magnitude = vel.magnitude;
            float angle = player.atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrowTransform.localRotation = Quaternion.Euler(0, 0, angle + 90f);
            float scaledMag = magnitude / maxMagnitude;
            Vector3 newScale = new Vector3(1f, scaledMag * scaleMultiplier, 1f);
            arrowTransform.localScale = newScale;
            spriteRenderer.color = Color.cyan;
            yield return null;
        }

        spriteRenderer.enabled  = false;
        showing = false;
    }
    
    private void InitializeGradient()
    {
        velocityGradient = new Gradient();

        var colorKeys = new GradientColorKey[4];
        colorKeys[0].color = new Color(0.7f, 0.9f, 1f); // light blue
        colorKeys[0].time = 0f;
        colorKeys[1].color = Color.blue;
        colorKeys[1].time = 0.33f;
        colorKeys[2].color = new Color(1f, 0.5f, 0f); // orange
        colorKeys[2].time = 0.66f;
        colorKeys[3].color = Color.red;
        colorKeys[3].time = 1f;

        var alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1f;
        alphaKeys[0].time = 0f;
        alphaKeys[1].alpha = 1f;
        alphaKeys[1].time = 1f;

        velocityGradient.SetKeys(colorKeys, alphaKeys);
    }


}
