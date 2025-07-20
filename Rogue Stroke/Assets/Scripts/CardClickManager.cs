using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardClickManager : MonoBehaviour
{
    [Header("Card Settings")]
    public RectTransform[] allCardPrefabs;
    public RectTransform cardParent;
    public Vector2[] targetPositions;
    public float scaleUpDuration = 0.6f;

    [Header("Runtime Data")]
    public List<RectTransform> activeCards = new List<RectTransform>();

    [Header("Shake Settings")]
    public float shakeDuration = 0.3f;
    public float shakeSpeed = 40f;
    public float shakeAmount = 10f;

    private bool cardAlreadyChosen = false;

    public void ShowRandomCards()
    {
        ClearCards();
        cardAlreadyChosen = false;

        if (allCardPrefabs.Length < 3)
        {
            Debug.LogWarning("Not enough card prefabs.");
            return;
        }

        List<int> indices = new List<int>();
        for (int i = 0; i < allCardPrefabs.Length; i++)
            indices.Add(i);

        for (int i = 0; i < 3; i++)
        {
            int rand = Random.Range(0, indices.Count);
            int cardID = indices[rand]; //Correct ID
            RectTransform prefab = allCardPrefabs[cardID];
            indices.RemoveAt(rand);

            RectTransform card = Instantiate(prefab, cardParent);
            card.anchoredPosition = targetPositions[i];
            card.localScale = Vector3.zero;

            // Set up click handler
            BoonCardClick clickScript = card.GetComponent<BoonCardClick>();
            if (clickScript == null) clickScript = card.gameObject.AddComponent<BoonCardClick>();
            clickScript.cardID = cardID;
            clickScript.manager = this;

            activeCards.Add(card);
            StartCoroutine(ScaleCardUp(card));
        }
    }

    public void OnCardClicked(int id)
    {
        if (cardAlreadyChosen) return;
        cardAlreadyChosen = true;

        Debug.Log($"Card with ID {id} clicked!");

        // Switch: Your card effect logic
        switch (id)
        {
            case 0:
                Debug.Log("Shot Tax selected!");
                break;
            case 1:
                Debug.Log("1 Less Card selected!");
                break;
            case 2:
                Debug.Log("Countdown Clock selected!");
                break;
            case 3:
                Debug.Log("Fake Walls selected!");
                break;
            case 4:
                Debug.Log("Shot Reversal selected!");
                break;
            case 5:
                Debug.Log("More Bounces selected!");
                break;
            default:
                Debug.LogWarning("Unhandled card ID: " + id);
                break;
        }

        // Find and animate selected card
        RectTransform selectedCard = null;
        foreach (var card in activeCards)
        {
            BoonCardClick click = card.GetComponent<BoonCardClick>();
            if (click != null && click.cardID == id)
            {
                selectedCard = card;
                break;
            }
        }

        if (selectedCard == null)
        {
            Debug.LogError("Selected card not found in activeCards.");
            return;
        }

        StartCoroutine(AnimateSelectedCard(selectedCard));

        // Shrink other cards
        foreach (var card in activeCards)
        {
            if (card != selectedCard)
                StartCoroutine(ShrinkAndDisable(card));
        }

        // Clear after delay
        StartCoroutine(ClearActiveCardsAfterDelay(1f));
    }

    IEnumerator AnimateSelectedCard(RectTransform card)
    {
        Vector2 originalPos = card.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Mathf.Sin(elapsed * shakeSpeed) * shakeAmount;
            card.anchoredPosition = originalPos + new Vector2(offsetX, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        card.anchoredPosition = originalPos;
        yield return StartCoroutine(ShrinkAndDisable(card));
    }

    IEnumerator ShrinkAndDisable(RectTransform card)
    {
        float shrinkTime = 0.3f;
        Vector3 start = card.localScale;
        Vector3 end = Vector3.zero;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / shrinkTime;
            card.localScale = Vector3.Lerp(start, end, t);
            yield return null;
        }

        card.localScale = Vector3.zero;
        card.gameObject.SetActive(false);
    }

    IEnumerator ClearActiveCardsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        activeCards.Clear();
    }

    IEnumerator ScaleCardUp(RectTransform card)
    {
        float t = 0f;
        Vector3 targetScale = Vector3.one;

        while (t < 1f)
        {
            t += Time.deltaTime / scaleUpDuration;
            card.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }

        card.localScale = targetScale;
    }

    public void ClearCards()
    {
        foreach (var card in activeCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }
        activeCards.Clear();
    }
}


