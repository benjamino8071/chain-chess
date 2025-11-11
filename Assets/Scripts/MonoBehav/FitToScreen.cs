using System;
using UnityEngine;

public class FitToScreen : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Camera _cam;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _cam = Camera.main;

        FitToCamera();
    }

    private void Update()
    {
        FitToCamera();
    }

    void FitToCamera()
    {
        // Camera dimensions
        float camHeight = _cam.orthographicSize * 2f;
        float camWidth = camHeight * _cam.aspect;

        // Sprite dimensions (in world units)
        Bounds spriteBounds = _spriteRenderer.sprite.bounds;
        float spriteWidth = spriteBounds.size.x;
        float spriteHeight = spriteBounds.size.y;

        // Scaling factors
        float scaleX = camWidth / spriteWidth;
        float scaleY = camHeight / spriteHeight;

        // Apply scale so it fits the camera
        transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }
}