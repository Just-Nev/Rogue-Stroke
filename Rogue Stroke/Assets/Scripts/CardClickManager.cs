using UnityEngine;

public class CardClickManager : MonoBehaviour
{
    public GolfHole golfHole;  // Assigned via inspector or dynamically

    public void OnCardClicked(int cardID)
    {
        Debug.Log($"Card with ID {cardID} clicked!");

        // Process logic for card effect
        switch (cardID)
        {
            case 0:
                Debug.Log("Speed Boost selected!");
                break;
            case 1:
                Debug.Log("Extra Shot selected!");
                break;
            case 2:
                Debug.Log("Teleport selected!");
                break;
            case 3:
                Debug.Log("Batman");
                break;
            case 4:
                Debug.Log("Picard");
                break;
            case 5:
                Debug.Log("Riker");
                break;
        }

        // Tell GolfHole to clear cards and mark this ID as used
        if (golfHole != null)
            golfHole.HideAllCardsAndMarkUsed(cardID);
    }
}
