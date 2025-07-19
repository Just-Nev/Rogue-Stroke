using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GolfHole : MonoBehaviour
{
    [Header("Sink Settings")]
    public float sinkDuration = 0.4f;
    public float sinkDepth = 0.4f;
    public float scaleFactor = 0.6f;

    [Header("Tag Settings")]
    public string ballTag = "Player"; // or "Ball"

    [Header("UI Display")]
    public RectTransform[] imagePool; // Drag disabled images into this
    public Vector2[] targetPositions = new Vector2[3]
    {
        new Vector2(-32f, -12f),
        new Vector2(417f, -12f),
        new Vector2(417f, -12f)
    };

    private RectTransform[] activeImages = new RectTransform[3];
    private bool[] boons = new bool[3];

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
                StartCoroutine(SinkBall(other.transform));
            }
        }
    }

    private IEnumerator SinkBall(Transform ball)
    {
        Vector3 startPos = ball.position;
        Vector3 targetPos = new Vector3(transform.position.x, startPos.y - sinkDepth, transform.position.z);
        Vector3 startScale = ball.localScale;
        Vector3 targetScale = startScale * scaleFactor;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / sinkDuration;
            ball.position = Vector3.Lerp(startPos, targetPos, t);
            ball.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        ball.gameObject.SetActive(false);
        ShowRandomImages();
    }

    private void ShowRandomImages()
    {
        if (imagePool.Length < 3)
        {
            Debug.LogWarning("Not enough images in the array!");
            return;
        }

        // Build list of available indices to prevent duplicates
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < imagePool.Length; i++)
            availableIndices.Add(i);

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            RectTransform chosenImage = imagePool[availableIndices[randomIndex]];
            availableIndices.RemoveAt(randomIndex);

            chosenImage.gameObject.SetActive(true);
            chosenImage.anchoredPosition = targetPositions[i];
            chosenImage.localScale = Vector3.zero;
            StartCoroutine(ScaleIn(chosenImage));

            BoonCardClick clickScript = chosenImage.GetComponent<BoonCardClick>();
            if (clickScript == null)
                clickScript = chosenImage.gameObject.AddComponent<BoonCardClick>();

            clickScript.cardIndex = i;
            clickScript.holeReference = this;

            activeImages[i] = chosenImage;
            boons[i] = false;
        }
    }

    public void OnCardClicked(int index)
    {
        if (index >= 0 && index < boons.Length)
        {
            boons[index] = true;
            Debug.Log($"Card {index + 1} clicked. boons[{index}] = true");
        }
    }

    private IEnumerator ScaleIn(Transform image)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            image.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        image.localScale = Vector3.one;
    }
}




