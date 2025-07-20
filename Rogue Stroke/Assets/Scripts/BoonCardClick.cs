using UnityEngine.EventSystems;
using UnityEngine;

public class BoonCardClick : MonoBehaviour, IPointerClickHandler
{
    public int cardID;
    private CardClickManager manager;

    public void Initialize(int id, CardClickManager mgr)
    {
        cardID = id;
        manager = mgr;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        manager.OnCardSelected(cardID, transform as RectTransform);
    }
}
