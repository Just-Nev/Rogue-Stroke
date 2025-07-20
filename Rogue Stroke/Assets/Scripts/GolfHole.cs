using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfHole : MonoBehaviour
{
    public string ballTag = "Player";
    public CardClickManager cardManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ballTag))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
                other.GetComponent<Collider>().enabled = false;
            }

            StartCoroutine(SinkBall(other.transform));
        }
    }

    private IEnumerator SinkBall(Transform ball)
    {
        Vector3 startPos = ball.position;
        Vector3 targetPos = startPos - new Vector3(0, 0.4f, 0);
        Vector3 startScale = ball.localScale;
        Vector3 targetScale = startScale * 0.6f;

        float t = 0f;
        float duration = 0.4f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            ball.position = Vector3.Lerp(startPos, targetPos, t);
            ball.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        ball.gameObject.SetActive(false);
        cardManager.ShowRandomCards();
    }
}







