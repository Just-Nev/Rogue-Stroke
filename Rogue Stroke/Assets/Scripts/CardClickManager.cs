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

    [Header("Shake Settings")]
    public float shakeDuration = 0.3f;
    public float shakeSpeed = 40f;
    public float shakeAmount = 10f;

    public List<RectTransform> activeCards = new List<RectTransform>();
    private bool cardAlreadyChosen = false;

    public void ShowRandomCards()
    {
        ClearCards();
        cardAlreadyChosen = false;

        // Categorize cards by rarity
        List<RectTransform> commons = new();
        List<RectTransform> rares = new();
        List<RectTransform> epics = new();

        foreach (var prefab in allCardPrefabs)
        {
            CardData data = prefab.GetComponent<CardData>();
            if (data == null) continue;

            switch (data.rarity)
            {
                case CardRarity.Common: commons.Add(prefab); break;
                case CardRarity.Rare: rares.Add(prefab); break;
                case CardRarity.Epic: epics.Add(prefab); break;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            RectTransform chosen = GetRandomCardWithRarity(commons, rares, epics);
            if (chosen == null) continue;

            RectTransform card = Instantiate(chosen, cardParent);
            card.anchoredPosition = targetPositions[i];
            card.localScale = Vector3.zero;

            BoonCardClick clickScript = card.GetComponent<BoonCardClick>();
            CardData data = chosen.GetComponent<CardData>();

            if (clickScript == null) clickScript = card.gameObject.AddComponent<BoonCardClick>();
            clickScript.cardID = data.cardID;
            clickScript.manager = this;

            activeCards.Add(card);
            StartCoroutine(ScaleCardUp(card));
        }
    }

    RectTransform GetRandomCardWithRarity(
        List<RectTransform> commons,
        List<RectTransform> rares,
        List<RectTransform> epics)
    {
        float roll = Random.value;

        if (roll < 0.6f && commons.Count > 0)
            return commons[Random.Range(0, commons.Count)];
        if (roll < 0.9f && rares.Count > 0)
            return rares[Random.Range(0, rares.Count)];
        if (epics.Count > 0)
            return epics[Random.Range(0, epics.Count)];

        // fallback if nothing matches
        if (commons.Count > 0) return commons[Random.Range(0, commons.Count)];
        if (rares.Count > 0) return rares[Random.Range(0, rares.Count)];

        return null;
    }

    public void OnCardClicked(int id)
    {
        if (cardAlreadyChosen) return;
        cardAlreadyChosen = true;

        Debug.Log($"Card with ID {id} clicked!");

        // Apply effect based on ID
        switch (id)
        {
            case 0: Debug.Log("Shot Tax"); break;
            case 1: Debug.Log("1 Less Card"); break;
            case 2: Debug.Log("Countdown Clock"); break;
            case 3: Debug.Log("Fake Walls"); break;
            case 4: Debug.Log("Shot Reversal"); break;
            case 5: Debug.Log("More Bounces"); break;
            case 6: Debug.Log("Re Roll Cards"); break;
            case 7: Debug.Log("Can Nudge The Ball After It Stops"); break;
            case 8: Debug.Log("Magnetic Pocket"); break;
            case 9: Debug.Log("5% Longer Shot Line"); break;
            case 10: Debug.Log("10% More Money"); break;
            case 11: Debug.Log("10% Shot Speed Increase"); break;
            case 12: Debug.Log("Patience Metre Increase"); break;
            case 13: Debug.Log("10% Longer Shot Line"); break;
            case 14: Debug.Log("Rewind"); break;
            case 15: Debug.Log("Portal Creator"); break;
            case 16: Debug.Log("Second Life"); break;
            case 17: Debug.Log("Handbreak"); break;
            default: Debug.Log("Unhandled ID"); break;
        }

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
            Debug.LogError("Selected card not found.");
            return;
        }

        StartCoroutine(AnimateSelectedCard(selectedCard));

        foreach (var card in activeCards)
        {
            if (card != selectedCard)
                StartCoroutine(ShrinkAndDisable(card));
        }

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

    IEnumerator ClearActiveCardsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        activeCards.Clear();
    }

    public void ClearCards()
    {
        foreach (var card in activeCards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        activeCards.Clear();
    }
}



