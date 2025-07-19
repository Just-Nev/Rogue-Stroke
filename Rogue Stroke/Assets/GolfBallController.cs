using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class GolfBallController : MonoBehaviour
{
    [Header("Drag Settings")]
    public float maxForce = 15f;
    public float maxDragDistance = 3f;

    [Header("Aiming Line")]
    public LineRenderer aimLine;
    public float lineYOffset = -0.05f;

    [Header("Ground Detection")]
    public LayerMask groundMask;

    [Header("RenderTexture Input (Optional)")]
    public RectTransform renderTextureUI;
    public Camera renderCamera;

    private Rigidbody rb;
    private Vector3 dragStartWorld;
    private Vector2 dragStartScreen;
    private Vector3 dragCurrentWorld;
    private bool isDragging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (aimLine != null)
        {
            aimLine.positionCount = 2;
            aimLine.enabled = false;
        }
    }

    void Update()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            dragStartScreen = Input.mousePosition;
            dragStartWorld = GetWorldPointFromScreen(dragStartScreen);
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 currentScreen = Input.mousePosition;
            Vector2 dragDelta = currentScreen - dragStartScreen;

            // Convert drag delta into a world-space vector
            Vector3 dragDir = new Vector3(dragDelta.x, 0, dragDelta.y).normalized;
            float dragMag = Mathf.Min(dragDelta.magnitude * 0.01f, maxDragDistance); // scaling

            Vector3 pullVector = dragDir * dragMag;

            if (aimLine != null)
            {
                aimLine.enabled = true;
                Vector3 offset = Vector3.up * lineYOffset;
                aimLine.SetPosition(0, transform.position + offset);
                aimLine.SetPosition(1, transform.position + offset - pullVector);
            }

            dragCurrentWorld = dragStartWorld + dragDir * dragMag;
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            if (aimLine != null) aimLine.enabled = false;

            Vector3 force = (transform.position - dragCurrentWorld);
            force = Vector3.ClampMagnitude(force, maxDragDistance);
            rb.AddForce(force * maxForce, ForceMode.Impulse);
        }
    }

    Vector3 GetWorldPointFromScreen(Vector2 screenPos)
    {
        Camera cam = renderCamera != null ? renderCamera : Camera.main;
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            return hit.point;
        }

        return transform.position;
    }
}








