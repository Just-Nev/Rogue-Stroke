using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Scene Settings")]
    public float sceneChangeDelay = 1.5f;
    public GameObject Fade;

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
        for (int i = 0; i < allCardPrefabs.Length; i++) indices.Add(i);

        for (int i = 0; i < 3; i++)
        {
            int rand = Random.Range(0, indices.Count);
            RectTransform prefab = allCardPrefabs[indices[rand]];
            int cardID = indices[rand];
            indices.RemoveAt(rand);

            RectTransform card = Instantiate(prefab, cardParent);
            card.anchoredPosition = targetPositions[i];
            card.localScale = Vector3.zero;

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

        // Save selected card to RunData
        if (RunData.Instance != null)
        {
            RunData.Instance.selectedCardIDs.Add(id);
            Debug.Log($"Saved Card ID {id} to RunData.");
        }

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


        // Find selected card
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

        // Animate selected card
        StartCoroutine(AnimateSelectedCard(selectedCard));

        // Shrink the other two
        foreach (var card in activeCards)
        {
            if (card != selectedCard)
                StartCoroutine(ShrinkAndDisable(card));
        }

        // Cleanup + transition
        StartCoroutine(ClearActiveCardsAfterDelay(sceneChangeDelay));
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
        Fade.SetActive(true);
        yield return new WaitForSeconds(delay);
        
        foreach (var card in activeCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        activeCards.Clear();
        if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            SceneManager.LoadScene(0); // Load Scene 1 when current scene is 3
        }
        // Load next scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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




