using UnityEngine;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public List<AudioClip> musicTracks;  // Assign your music clips in the Inspector
    public AudioSource audioSource;
    public bool loopCurrentTrack = true;

    private const string VolumePrefKey = "MusicVolume";
    private int currentTrackIndex = 0;

    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Load saved volume or use default (e.g., 0.8f)
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 0.5f);
        SetVolume(savedVolume);
    }

    void Start()
    {
        if (musicTracks.Count == 0)
        {
            Debug.LogWarning("No music tracks assigned!");
            return;
        }

        PlayTrack(currentTrackIndex);
    }

    void Update()
    {
        if (!audioSource.isPlaying && !loopCurrentTrack)
        {
            PlayNextTrack();
        }
    }

    public void PlayTrack(int index)
    {
        if (index < 0 || index >= musicTracks.Count)
        {
            Debug.LogWarning("Track index out of range!");
            return;
        }

        currentTrackIndex = index;
        audioSource.clip = musicTracks[currentTrackIndex];
        audioSource.loop = loopCurrentTrack;
        audioSource.Play();
    }

    public void PlayNextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Count;
        PlayTrack(currentTrackIndex);
    }

    public void PlayPreviousTrack()
    {
        currentTrackIndex--;
        if (currentTrackIndex < 0) currentTrackIndex = musicTracks.Count - 1;
        PlayTrack(currentTrackIndex);
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(VolumePrefKey, audioSource.volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return audioSource.volume;
    }
}
