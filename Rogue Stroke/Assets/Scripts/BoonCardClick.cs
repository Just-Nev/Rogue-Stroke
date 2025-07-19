using UnityEngine;
using UnityEngine.EventSystems;

public class BoonCardClick : MonoBehaviour, IPointerClickHandler
{
    public int cardIndex; // Set this 0–2 when assigning the card
    public GolfHole holeReference;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (holeReference != null)
        {
            holeReference.OnCardClicked(cardIndex);
        }
    }
}
