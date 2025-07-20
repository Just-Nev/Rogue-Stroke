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

    [Header("Precision Settings")]
    public float precisionPowerMultiplier = 0.2f;
    public Color precisionColor = Color.cyan;

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
    private bool isPrecisionShot = false;

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

        if (spriteLevel1 != null) spriteLevel1.SetActive(true);
        if (spriteLevel2 != null) spriteLevel2.SetActive(false);
        if (spriteLevel3 != null) spriteLevel3.SetActive(false);
    }

    void Update()
    {
        if (isMoving) return;

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            dragStartScreen = Input.mousePosition;
            isDragging = true;
            isPrecisionShot = Input.GetMouseButton(1); // right-click
            if (aimLine != null) aimLine.enabled = true;
        }
        else if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && isDragging)
        {
            Vector2 dragDelta = (Vector2)Input.mousePosition - dragStartScreen;
            Vector3 pullDir = new Vector3(dragDelta.x, 0, dragDelta.y).normalized;

            float dragMagnitude = Mathf.Pow(dragDelta.magnitude, 0.85f) * 0.01f;
            float multiplier = isPrecisionShot ? precisionPowerMultiplier : 1f;
            float pathLength = Mathf.Clamp(dragMagnitude * multiplier, 0f, pathMaxDistance);

            bouncePath = GenerateBouncePath(transform.position, pullDir, pathLength);

            float powerPercent = Mathf.Clamp01(pathLength / pathMaxDistance);
            Color currentColor = isPrecisionShot ? precisionColor : Color.Lerp(minPowerColor, maxPowerColor, powerPercent);

            aimLine.startColor = currentColor;
            aimLine.endColor = currentColor;

            DrawLine(bouncePath); // <--- draws the full line
        }
        else if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && isDragging)
        {
            isDragging = false;
            if (aimLine != null) aimLine.enabled = false;

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

    void UpdateShotSprites()
    {
        if (spriteLevel1 != null) spriteLevel1.SetActive(false);
        if (spriteLevel2 != null) spriteLevel2.SetActive(false);
        if (spriteLevel3 != null) spriteLevel3.SetActive(false);

        if (shotCount < 3 && spriteLevel1 != null) spriteLevel1.SetActive(true);
        else if (shotCount < 5 && spriteLevel2 != null) spriteLevel2.SetActive(true);
        else if (spriteLevel3 != null) spriteLevel3.SetActive(true);
    }

    List<Vector3> GenerateBouncePath(Vector3 startPos, Vector3 direction, float totalDistance)
    {
        List<Vector3> points = new() { startPos };
        Vector3 currentPos = startPos;
        Vector3 currentDir = direction;
        float remainingDistance = Mathf.Min(totalDistance, pathMaxDistance);

        for (int i = 0; i < maxBounces && remainingDistance > 0f; i++)
        {
            if (Physics.Raycast(currentPos, currentDir, out RaycastHit hit, remainingDistance, collisionMask))
            {
                points.Add(hit.point);
                float traveled = Vector3.Distance(currentPos, hit.point);
                remainingDistance -= traveled;
                currentPos = hit.point;
                currentDir = Vector3.Reflect(currentDir, hit.normal);

                if (isMoving && audioSource && bounceSound && traveled > 0.5f)
                    audioSource.PlayOneShot(bounceSound);
            }
            else
            {
                points.Add(currentPos + currentDir * remainingDistance);
                break;
            }
        }

        return points;
    }

    void DrawLine(List<Vector3> fullPath)
    {
        if (aimLine == null || fullPath.Count < 2) return;

        float totalLength = 0f;
        for (int i = 1; i < fullPath.Count; i++)
            totalLength += Vector3.Distance(fullPath[i - 1], fullPath[i]);

        float visibleLength = totalLength * Mathf.Clamp01(aimLineVisiblePercent);
        float accumulated = 0f;
        List<Vector3> visiblePoints = new() { fullPath[0] };

        for (int i = 1; i < fullPath.Count; i++)
        {
            float segment = Vector3.Distance(fullPath[i - 1], fullPath[i]);
            if (accumulated + segment > visibleLength)
            {
                float remaining = visibleLength - accumulated;
                Vector3 dir = (fullPath[i] - fullPath[i - 1]).normalized;
                visiblePoints.Add(fullPath[i - 1] + dir * remaining);
                break;
            }

            visiblePoints.Add(fullPath[i]);
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
        Vector3 lastDirection = Vector3.zero;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = path[i];
            Vector3 end = path[i + 1];
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);
            float duration = distance / speed;

            if (i > 0 && Vector3.Angle(lastDirection, direction) > 10f && audioSource && bounceSound)
                audioSource.PlayOneShot(bounceSound);

            lastDirection = direction;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            finalDir = direction;
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





