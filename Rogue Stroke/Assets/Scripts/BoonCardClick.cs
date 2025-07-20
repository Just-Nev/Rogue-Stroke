using UnityEngine.EventSystems;
using UnityEngine;

public class BoonCardClick : MonoBehaviour, IPointerClickHandler
{
    public int cardID;
    public CardClickManager manager;

    public void OnPointerClick(PointerEventData eventData)
    {
        manager?.OnCardClicked(cardID);
    }
}
