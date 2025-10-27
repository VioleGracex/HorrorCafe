using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    public AudioSource chaseMusicSource; 
    public AudioClip chaseMusicClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayChaseMusic()
    {
        if (chaseMusicSource == null || chaseMusicClip == null) return;
        if (chaseMusicSource.isPlaying) return;

        chaseMusicSource.clip = chaseMusicClip;
        chaseMusicSource.spatialBlend = 0f; 
        chaseMusicSource.loop = true;
        chaseMusicSource.Play();
    }

    public void StopChaseMusic()
    {
        if (chaseMusicSource != null)
            chaseMusicSource.Stop();
    }
}