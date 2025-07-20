using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardClickManager : MonoBehaviour
{
    public List<RectTransform> allCardPrefabs;
    public Transform cardParent;
    public Vector2[] targetPositions = new Vector2[3]
    {
        new Vector2(-32f, -12f),
        new Vector2(417f, -12f),
        new Vector2(417f, -12f)
    };

    public List<RectTransform> activeCards = new();

    public void ShowRandomCards()
    {
        ClearPreviousCards();

        List<int> selectedIndices = GetThreeRandomCardIndices();

        for (int i = 0; i < selectedIndices.Count; i++)
        {
            RectTransform card = Instantiate(allCardPrefabs[selectedIndices[i]], cardParent);
            card.anchoredPosition = targetPositions[i];
            card.localScale = Vector3.zero;

            var cardComponent = card.GetComponent<BoonCardClick>();
            cardComponent.Initialize(selectedIndices[i], this);

            StartCoroutine(ScaleIn(card));
            activeCards.Add(card);
        }
    }

    public void OnCardSelected(int cardID, RectTransform selectedCard)
    {
        Debug.Log("Selected card ID: " + cardID);
        StartCoroutine(HandleCardSelectionFeedback(selectedCard));
    }

    IEnumerator HandleCardSelectionFeedback(RectTransform selectedCard)
    {
        yield return StartCoroutine(ShakeCard(selectedCard, 0.3f, 10f));

        foreach (var card in activeCards)
        {
            StartCoroutine(ShrinkCard(card, 0.3f));
        }

        yield return new WaitForSeconds(0.35f);

        foreach (var card in activeCards)
        {
            if (card != null)
                card.gameObject.SetActive(false);
        }

        activeCards.Clear();
    }

    List<int> GetThreeRandomCardIndices()
    {
        List<int> pool = new List<int>();
        for (int i = 0; i < allCardPrefabs.Count; i++)
            pool.Add(i);

        List<int> selected = new List<int>();
        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            selected.Add(pool[index]);
            pool.RemoveAt(index);
        }
        return selected;
    }

    void ClearPreviousCards()
    {
        foreach (var card in activeCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }
        activeCards.Clear();
    }

    IEnumerator ScaleIn(RectTransform card)
    {
        float t = 0f;
        float duration = 0.3f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            card.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }
        card.localScale = Vector3.one;
    }

    IEnumerator ShrinkCard(RectTransform card, float duration)
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

    IEnumerator ShakeCard(RectTransform card, float duration, float magnitude)
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
