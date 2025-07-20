using UnityEngine;

public class PersistentMusic : MonoBehaviour
{
    private static PersistentMusic instance;

    [Header("Audio")]
    public AudioSource musicSource;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Prevent duplicate music players
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes

        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
}

