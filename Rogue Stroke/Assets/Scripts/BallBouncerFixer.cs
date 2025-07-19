using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallBounceFixer : MonoBehaviour
{
    [Range(0f, 1f)] public float bounceDampening = 0.95f;
    public float shallowAngleThreshold = 0.3f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (rb == null || collision.contactCount == 0)
            return;

        ContactPoint contact = collision.contacts[0];
        Vector3 velocity = rb.linearVelocity;
        Vector3 normal = contact.normal;

        float angle = Vector3.Dot(velocity.normalized, -normal);

        if (angle < shallowAngleThreshold)
        {
            Vector3 reflected = Vector3.Reflect(velocity, normal) * bounceDampening;
            rb.linearVelocity = reflected;
        }
    }
}

