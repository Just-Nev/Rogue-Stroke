using UnityEngine.EventSystems;
using UnityEngine;

public class BoonCardClick : MonoBehaviour, IPointerClickHandler
{
    public int cardID;
    public GolfHole hole;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (hole != null)
        {
            hole.HideAllCardsAndMarkUsed(cardID);
        }
    }
}
