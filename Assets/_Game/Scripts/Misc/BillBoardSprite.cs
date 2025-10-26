using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    private bool isVisible = true;

    void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnBecameVisible()
    {
        isVisible = true;
    }

    void OnBecameInvisible()
    {
        isVisible = false;
    }

    void LateUpdate()
    {
        if (!isVisible)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera == null || spriteRenderer == null)
            return;

        Vector3 camPos = mainCamera.transform.position;
        Vector3 direction = camPos - transform.position;
        direction.y = 0f;
        transform.forward = -direction.normalized;
    }
}