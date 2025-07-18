using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // The ball
    public float smoothSpeed = 5f;    // Follow smoothness
    public Vector3 offset;            // Offset from the ball

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z); // lock Z
    }
}

