using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GolfBallHeight : MonoBehaviour
{
    [Header("References")]
    public GameObject targetObject;        // The object to check height for (e.g. the ball)
    public GameObject objectToActivate;    // The object to activate when ball falls below height

    [Header("Settings")]
    public float heightThreshold = 2f;     // Y threshold
    public float delayBeforeSceneLoad = 1f;

    private bool triggered = false;

    void Update()
    {
        if (!triggered && targetObject != null && targetObject.transform.position.y < heightThreshold)
        {
            triggered = true;
            if (objectToActivate != null)
                objectToActivate.SetActive(true);

            StartCoroutine(LoadSceneAfterDelay());
        }
    }

    IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeSceneLoad);
        SceneManager.LoadScene(0);
    }
}
