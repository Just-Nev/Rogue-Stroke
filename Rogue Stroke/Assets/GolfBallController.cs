using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GolfBallController : MonoBehaviour
{
    [Header("Shot Settings")]
    public int maxBounces = 5;
    public float pathFollowSpeed = 1.5f;
    public float pathMaxDistance = 20f;

    [Header("Power Settings")]
    public float baseDragScale = 0.02f;
    public float powerShotMultiplier = 1.0f;
    public float precisionShotMultiplier = 0.3f;
    public float tapShotMultiplier = 0.1f;

    [Header("Aiming Line")]
    public LineRenderer aimLine;
    public float aimLineYOffset = 0.05f;
    [Range(0f, 1f)] public float aimLineVisiblePercent = 1f;
    public Color minPowerColor = Color.white;
    public Color maxPowerColor = Color.red;

    [Header("Collision Settings")]
    public LayerMask collisionMask;

    [Header("Stop Settings")]
    public float glideSpeed = 2f;
    public float stopThreshold = 0.1f;
    public float linearDamping = 1.5f;
    public float requiredLowSpeedDuration = 0.2f;

    [Header("Shot Counter")]
    public int shotCount = 0;
    public GameObject spriteLevel1;
    public GameObject spriteLevel2;
    public GameObject spriteLevel3;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shotSound;
    public AudioClip bounceSound;

    private Rigidbody rb;
    private bool isDragging = false;
    private Vector2 dragStartScreen;
    private List<Vector3> bouncePath = new();
    private bool isMoving = false;
    private float currentPowerMultiplier = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.linearDamping = linearDamping;
        rb.angularDamping = 0.05f;

        if (aimLine != null)
        {
            aimLine.positionCount = 0;
            aimLine.enabled = false;
        }

        if (spriteLevel1) spriteLevel1.SetActive(true);
        if (spriteLevel2) spriteLevel2.SetActive(false);
        if (spriteLevel3) spriteLevel3.SetActive(false);
    }

    void Update()
    {
        if (isMoving) return;

        // Determine which shot mode is used
        if (Input.GetMouseButtonDown(0)) StartDrag(powerShotMultiplier);
        else if (Input.GetMouseButtonDown(1)) StartDrag(precisionShotMultiplier);
        else if (Input.GetMouseButtonDown(2)) StartDrag(tapShotMultiplier);

        else if ((Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)) && isDragging)
        {
            Vector2 dragDelta = (Vector2)Input.mousePosition - dragStartScreen;
            Vector3 pullDir = new Vector3(dragDelta.x, 0, dragDelta.y).normalized;

            float dragMagnitude = dragDelta.magnitude * baseDragScale * currentPowerMultiplier;
            float pathLength = Mathf.Clamp(dragMagnitude, 0f, pathMaxDistance);

            bouncePath = GenerateBouncePath(transform.position, pullDir, pathLength);

            float powerPercent = Mathf.Clamp01(pathLength / pathMaxDistance);
            Color currentColor = Color.Lerp(minPowerColor, maxPowerColor, powerPercent);
            aimLine.startColor = currentColor;
            aimLine.endColor = currentColor;

            DrawLine(bouncePath);
        }
        else if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)) && isDragging)
        {
            isDragging = false;
            aimLine.enabled = false;

            if (bouncePath.Count > 1)
            {
                shotCount++;
                UpdateShotSprites();

                if (audioSource && shotSound)
                    audioSource.PlayOneShot(shotSound);

                StartCoroutine(FollowPath(bouncePath, pathFollowSpeed));
            }
        }
    }

    void StartDrag(float powerMultiplier)
    {
        dragStartScreen = Input.mousePosition;
        currentPowerMultiplier = powerMultiplier;
        isDragging = true;
    }

    void UpdateShotSprites()
    {
        if (spriteLevel1) spriteLevel1.SetActive(false);
        if (spriteLevel2) spriteLevel2.SetActive(false);
        if (spriteLevel3) spriteLevel3.SetActive(false);

        if (shotCount < 3 && spriteLevel1) spriteLevel1.SetActive(true);
        else if (shotCount < 5 && spriteLevel2) spriteLevel2.SetActive(true);
        else if (spriteLevel3) spriteLevel3.SetActive(true);
    }

    List<Vector3> GenerateBouncePath(Vector3 startPos, Vector3 direction, float totalDistance)
    {
        List<Vector3> points = new() { startPos };
        Vector3 currentPos = startPos;
        Vector3 currentDir = direction;
        float remaining = Mathf.Min(totalDistance, pathMaxDistance);

        for (int i = 0; i < maxBounces && remaining > 0f; i++)
        {
            if (Physics.Raycast(currentPos, currentDir, out RaycastHit hit, remaining, collisionMask))
            {
                points.Add(hit.point);
                float traveled = Vector3.Distance(currentPos, hit.point);
                remaining -= traveled;
                currentPos = hit.point;
                currentDir = Vector3.Reflect(currentDir, hit.normal);
            }
            else
            {
                points.Add(currentPos + currentDir * remaining);
                break;
            }
        }

        return points;
    }

    void DrawLine(List<Vector3> path)
    {
        if (aimLine == null || path.Count < 2) return;

        float totalLength = 0f;
        for (int i = 1; i < path.Count; i++)
            totalLength += Vector3.Distance(path[i - 1], path[i]);

        float visibleLength = totalLength * Mathf.Clamp01(aimLineVisiblePercent);
        float accumulated = 0f;
        List<Vector3> visiblePoints = new() { path[0] };

        for (int i = 1; i < path.Count; i++)
        {
            float segment = Vector3.Distance(path[i - 1], path[i]);
            if (accumulated + segment > visibleLength)
            {
                float remain = visibleLength - accumulated;
                Vector3 dir = (path[i] - path[i - 1]).normalized;
                visiblePoints.Add(path[i - 1] + dir * remain);
                break;
            }

            visiblePoints.Add(path[i]);
            accumulated += segment;
        }

        aimLine.enabled = true;
        aimLine.positionCount = visiblePoints.Count;
        for (int i = 0; i < visiblePoints.Count; i++)
        {
            aimLine.SetPosition(i, visiblePoints[i] + Vector3.up * aimLineYOffset);
        }
    }

    IEnumerator FollowPath(List<Vector3> path, float speed)
    {
        isMoving = true;
        rb.isKinematic = true;

        Vector3 finalDir = Vector3.zero;
        Vector3 lastDir = Vector3.zero;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = path[i];
            Vector3 end = path[i + 1];
            Vector3 dir = (end - start).normalized;
            float distance = Vector3.Distance(start, end);
            float duration = distance / speed;

            if (i > 0 && Vector3.Angle(lastDir, dir) > 10f && audioSource && bounceSound)
                audioSource.PlayOneShot(bounceSound);

            lastDir = dir;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            finalDir = dir;
        }

        rb.isKinematic = false;
        rb.linearDamping = linearDamping;
        rb.linearVelocity = finalDir * glideSpeed;

        float lowSpeedTime = 0f;
        while (true)
        {
            if (rb.linearVelocity.magnitude < stopThreshold)
            {
                lowSpeedTime += Time.deltaTime;
                if (lowSpeedTime >= requiredLowSpeedDuration)
                {
                    rb.linearVelocity = Vector3.zero;
                    break;
                }
            }
            else
            {
                lowSpeedTime = 0f;
            }

            yield return null;
        }

        isMoving = false;
    }
}









