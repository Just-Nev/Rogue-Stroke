using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GolfHole : MonoBehaviour
{
    [Header("Sink Settings")]
    public float sinkDuration = 0.4f;
    public float sinkDepth = 0.4f;
    public float scaleFactor = 0.6f;

    [Header("Tag Settings")]
    public string ballTag = "Player";

    [Header("UI Display")]
    public RectTransform[] imagePool; // assign all cards in Inspector
    public Vector2[] targetPositions = new Vector2[3]
    {
        new Vector2(-32f, -12f),
        new Vector2(417f, -12f),
        new Vector2(417f, -12f)
    };

    private List<RectTransform> activeImages = new List<RectTransform>();
    private HashSet<int> usedCardIDs = new HashSet<int>();

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
        ShowRandomCards();
    }

    private void ShowRandomCards()
    {
        activeImages.Clear();

        if (imagePool.Length < 3)
        {
            Debug.LogWarning("Not enough cards assigned to imagePool!");
            return;
        }

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < imagePool.Length; i++)
        {
            if (!usedCardIDs.Contains(i))
                availableIndices.Add(i);
        }

        if (availableIndices.Count < 3)
        {
            Debug.LogWarning("Not enough unused cards left to show 3!");
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            int chosenIndex = availableIndices[randomIndex];
            availableIndices.RemoveAt(randomIndex);

            RectTransform card = imagePool[chosenIndex];
            card.gameObject.SetActive(true);
            card.anchoredPosition = targetPositions[i];
            card.localScale = Vector3.zero;

            BoonCardClick clickScript = card.GetComponent<BoonCardClick>();
            if (clickScript == null)
                clickScript = card.gameObject.AddComponent<BoonCardClick>();

            clickScript.cardID = chosenIndex;
            clickScript.hole = this;

            activeImages.Add(card);
            StartCoroutine(ScaleInCard(card));
        }
    }

    private IEnumerator ScaleInCard(RectTransform card)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            card.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        card.localScale = targetScale;
    }

    public void HideAllCardsAndMarkUsed(int clickedID)
    {
        Debug.Log("Card with ID " + clickedID + " clicked!");
        usedCardIDs.Add(clickedID);

        StartCoroutine(SmoothHideCards(clickedID));
    }

    private IEnumerator SmoothHideCards(int clickedID)
    {
        float shakeDuration = 0.3f;
        float shrinkDuration = 0.25f;

        // Step 1: Shake selected card
        RectTransform clickedCard = activeImages[clickedID];
        yield return StartCoroutine(ShakeCard(clickedCard, shakeDuration, 10f));

        // Step 2: Shrink all cards
        List<Coroutine> shrinkCoroutines = new List<Coroutine>();
        foreach (var card in activeImages)
        {
            shrinkCoroutines.Add(StartCoroutine(ShrinkCard(card, shrinkDuration)));
        }

        foreach (var c in shrinkCoroutines)
            yield return c;

        // Step 3: Deactivate all
        foreach (var card in activeImages)
            if (card != null)
                card.gameObject.SetActive(false);

        activeImages.Clear();
    }

    private IEnumerator ShrinkCard(RectTransform card, float duration)
    {
        Vector3 start = card.localScale;
        Vector3 end = Vector3.zero;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            card.localScale = Vector3.Lerp(start, end, t);
            yield return null;
        }

        card.localScale = end;
    }

    private IEnumerator ShakeCard(RectTransform card, float duration, float magnitude)
    {
        Vector3 originalPos = card.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            card.anchoredPosition = originalPos + new Vector3(offsetX, offsetY);
            elapsed += Time.deltaTime;
            yield return null;
        }

        card.anchoredPosition = originalPos;
    }
}







