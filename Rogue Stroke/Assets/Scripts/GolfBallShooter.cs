using UnityEngine;
using UnityEngine.UI;

public class GolfBallShooter : MonoBehaviour
{
    [Header("Physics")]
    public float maxPower = 10f;
    public Rigidbody2D rb;

    [Header("UI")]
    public Slider powerBar;

    [Header("Aiming")]
    public LineRenderer aimLine;
    public int predictionSteps = 30;
    public float timeStep = 0.05f;

    private Vector2 dragStartPos;
    private bool isDragging = false;
    private bool isMoving = false;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (aimLine != null) aimLine.positionCount = 0;
        if (powerBar != null) powerBar.value = 0f;
    }

    void Update()
    {
        if (isMoving) return;

        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 currentDragPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            ShowAimLine(currentDragPos);
            ShowPowerBar(dragStartPos, currentDragPos);
            DrawPrediction(dragStartPos - currentDragPos);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Vector2 dragEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 force = dragStartPos - dragEndPos;
            Shoot(force);

            ClearAimLine();
            if (powerBar != null) powerBar.value = 0f;
            isDragging = false;
        }
    }

    void Shoot(Vector2 force)
    {
        force = Vector2.ClampMagnitude(force, maxPower);
        rb.AddForce(force * 100f); // Adjust multiplier for feel
        isMoving = true;
    }

    void FixedUpdate()
    {
        if (isMoving && rb.linearVelocity.magnitude < 0.05f)
        {
            rb.linearVelocity = Vector2.zero;
            isMoving = false;
        }
    }

    void ShowAimLine(Vector2 currentPos)
    {
        if (aimLine == null) return;

        aimLine.positionCount = 2;
        aimLine.SetPosition(0, transform.position);
        aimLine.SetPosition(1, currentPos);
    }

    void ClearAimLine()
    {
        if (aimLine == null) return;
        aimLine.positionCount = 0;
    }

    void ShowPowerBar(Vector2 start, Vector2 end)
    {
        if (powerBar == null) return;

        float power = (start - end).magnitude;
        float percent = Mathf.Clamp01(power / maxPower);
        powerBar.value = percent;
    }

    void DrawPrediction(Vector2 force)
    {
        if (aimLine == null) return;

        Vector2 pos = rb.position;
        Vector2 velocity = force * 100f / rb.mass;
        aimLine.positionCount = predictionSteps;

        for (int i = 0; i < predictionSteps; i++)
        {
            aimLine.SetPosition(i, pos);
            velocity *= 1f - (Time.fixedDeltaTime * rb.linearDamping); // simulate drag
            pos += velocity * timeStep;
        }
    }
}


