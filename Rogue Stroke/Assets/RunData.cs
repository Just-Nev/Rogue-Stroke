using System.Collections.Generic;
using UnityEngine;

public class RunData : MonoBehaviour
{
    public static RunData Instance;

    [Header("Runtime Card Data")]
    public List<int> selectedCardIDs = new List<int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Stays alive between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetRun()
    {
        selectedCardIDs.Clear();
    }
}
