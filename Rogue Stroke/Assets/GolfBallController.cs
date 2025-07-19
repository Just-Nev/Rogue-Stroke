using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GolfBallController : MonoBehaviour
{
    [Header("Shot Settings")]
    public float maxForce = 15f;
    public float maxDragDistance = 3f;
    public int maxBounces = 5;
    public float pathFollowSpeed = 10f;
    public float pathMaxDistance = 20f;

    [Header("Aiming Line")]
    public LineRenderer aimLine;
    public float aimLineYOffset = 0.05f;

    [Header("Collision Settings")]
    public LayerMask collisionMask;

    [Header("Aiming Line Visibility")]
    [Range(0f, 1f)]
    public float aimLineVisiblePercent = 1f; // 0 = hidden, 1 = full path shown

    private Rigidbody rb;
    private bool isDragging = false;
    private Vector2 dragStartScreen;
    private Vector3 dragWorldTarget;
    private List<Vector3> bouncePath = new List<Vector3>();
    private bool isMoving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.linearDamping = 0.3f;
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
            float pullStrength = Mathf.Min(dragDelta.magnitude * 0.01f, maxDragDistance);
            Vector3 pullVector = pullDir * pullStrength;

            dragWorldTarget = transform.position + pullVector;

            Vector3 shotDir = (dragWorldTarget - transform.position).normalized;
            float shotPower = Mathf.Min(Vector3.Distance(transform.position, dragWorldTarget), maxDragDistance);

            float totalPathLength = shotPower * maxForce;
            bouncePath = GenerateBouncePath(transform.position, shotDir, totalPathLength);

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
        List<Vector3> points = new List<Vector3>();
        points.Add(startPos);

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

        List<Vector3> visiblePoints = new List<Vector3>();
        visiblePoints.Add(fullPath[0]);
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

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = path[i];
            Vector3 end = path[i + 1];
            float distance = Vector3.Distance(start, end);
            float duration = distance / speed;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }
        }

        rb.isKinematic = false;
        isMoving = false;
    }
}















