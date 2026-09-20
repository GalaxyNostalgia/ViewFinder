using UnityEngine;

// One music track that keeps playing across scene loads. The first one to wake up wins,
// and any copy sitting in a scene loaded later sees it and gets out of the way, so the
// track does not restart every time you finish a level.
[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    [Tooltip("Track to loop. Drop a clip from Assets/Audio here. Nothing plays while this is empty.")]
    public AudioClip music;

    [Range(0f, 1f)]
    public float volume = 0.5f;

    static MusicPlayer instance;

    AudioSource source;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // DontDestroyOnLoad only works on a root object.
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        source.clip = music;
        source.loop = true;
        source.volume = volume;
        source.playOnAwake = false;
        source.spatialBlend = 0f;

        if (music != null)
            source.Play();
        else
            Debug.LogWarning("MusicPlayer has no clip, so nothing will play. See Assets/Audio/README.md.", this);
    }

    // Lets you drag the volume slider while the game is running.
    void OnValidate()
    {
        volume = Mathf.Clamp01(volume);

        if (source != null)
            source.volume = volume;
    }
}
