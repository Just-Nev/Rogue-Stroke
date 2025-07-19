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

    [Header("Hole Detection")]
    public float holeSnapDistance = 0.5f;
    public string holeTag = "Hole";

    private Rigidbody rb;
    private bool isDragging = false;
    private Vector2 dragStartScreen;
    private List<Vector3> bouncePath = new List<Vector3>();
    private bool isMoving = false;

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
    }

    void Update()
    {
        if (isMoving) return;

        if (Input.GetMouseButtonDown(0))
        {
            dragStartScreen = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 dragCurrent = Input.mousePosition;
            Vector2 dragDelta = dragCurrent - dragStartScreen;

            Vector3 pullDir = new Vector3(dragDelta.x, 0, dragDelta.y).normalized;
            float dragMagnitude = Mathf.Pow(dragDelta.magnitude, 0.85f) * 0.01f;
            float pathLength = Mathf.Clamp(dragMagnitude, 0f, pathMaxDistance);

            bouncePath = GenerateBouncePath(transform.position, pullDir, pathLength);

            float powerPercent = Mathf.Clamp01(pathLength / pathMaxDistance);
            Color currentColor = Color.Lerp(minPowerColor, maxPowerColor, powerPercent);
            aimLine.startColor = currentColor;
            aimLine.endColor = currentColor;

            DrawLine(bouncePath);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            if (aimLine != null) aimLine.enabled = false;

            if (bouncePath.Count > 1)
                StartCoroutine(FollowPath(bouncePath, pathFollowSpeed));
        }
    }

    List<Vector3> GenerateBouncePath(Vector3 startPos, Vector3 direction, float totalDistance)
    {
        List<Vector3> points = new List<Vector3> { startPos };
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
            }
            else
            {
                points.Add(currentPos + currentDir * remainingDistance);
                break;
            }
        }

        return points;
    }

    float CalculatePathLength(List<Vector3> path)
    {
        float length = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            length += Vector3.Distance(path[i], path[i + 1]);
        }
        return length;
    }

    void DrawLine(List<Vector3> fullPath)
    {
        if (aimLine == null || fullPath.Count < 2) return;

        float totalLength = CalculatePathLength(fullPath);
        float visibleLength = totalLength * Mathf.Clamp01(aimLineVisiblePercent);

        List<Vector3> visiblePoints = new List<Vector3> { fullPath[0] };
        float accumulated = 0f;

        for (int i = 1; i < fullPath.Count; i++)
        {
            float segmentLength = Vector3.Distance(fullPath[i - 1], fullPath[i]);

            if (accumulated + segmentLength > visibleLength)
            {
                float remaining = visibleLength - accumulated;
                Vector3 dir = (fullPath[i] - fullPath[i - 1]).normalized;
                Vector3 partialPoint = fullPath[i - 1] + dir * remaining;
                visiblePoints.Add(partialPoint);
                break;
            }

            visiblePoints.Add(fullPath[i]);
            accumulated += segmentLength;
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

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = path[i];
            Vector3 end = path[i + 1];
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);
            float duration = distance / speed;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            finalDir = direction;
        }

        GameObject nearestHole = FindNearestHole(transform.position);
        if (nearestHole && Vector3.Distance(transform.position, nearestHole.transform.position) <= holeSnapDistance)
        {
            yield return StartCoroutine(MoveToHole(nearestHole.transform.position));
            yield break;
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

    GameObject FindNearestHole(Vector3 position)
    {
        GameObject[] holes = GameObject.FindGameObjectsWithTag(holeTag);
        GameObject closest = null;
        float closestDist = Mathf.Infinity;

        foreach (GameObject hole in holes)
        {
            float dist = Vector3.Distance(position, hole.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = hole;
            }
        }

        return closest;
    }

    IEnumerator MoveToHole(Vector3 holePosition)
    {
        Vector3 dropTarget = holePosition - Vector3.up * 0.1f;
        float dropDuration = 0.3f;
        Vector3 startPos = transform.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / dropDuration;
            transform.position = Vector3.Lerp(startPos, dropTarget, t);
            yield return null;
        }

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.detectCollisions = false;
        isMoving = false;
        Debug.Log("Ball dropped into hole.");
    }
}




















