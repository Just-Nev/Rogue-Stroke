using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeBouncePhysics : MonoBehaviour
{
    public float bounceDampening = 0.98f; // Lose a little speed on bounce
    public float stopThreshold = 0.05f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude < stopThreshold)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contactCount == 0) return;

        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;

        Vector3 incomingVelocity = rb.linearVelocity;
        Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, normal);

        rb.linearVelocity = reflectedVelocity * bounceDampening;
    }
}

