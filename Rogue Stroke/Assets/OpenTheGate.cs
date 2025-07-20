using UnityEngine;

public class OpenTheGate : MonoBehaviour
{
    public GameObject Gate;
    public GameObject GatesAnim;
    public GameObject Player;

    private void Update()
    {
        if (Player != null && !Player.activeInHierarchy)
        {
            // Player is NOT active
            // Do something here (e.g., turn off the gates)
            if (Gate != null) Gate.SetActive(false);
            if (GatesAnim != null) GatesAnim.SetActive(true);
        }
    }
}
