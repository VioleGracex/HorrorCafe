using UnityEngine;
using Ouiki.FPS;
using Ouiki.Interfaces;

namespace Ouiki.Items
{
    public class Speaker : MonoBehaviour, ILabel, IInteractable, ICommand
    {
        [Header("Speaker Settings")]
        public string label = "Speaker";
        public AudioSource musicSource;
        public AudioClip[] playlist;

        private int currentTrack = 0;
        private bool isPlaying = false;

        public string Label => label;

        public bool IsInteractable => true;

        public string ActionName
        {
            get
            {
                if (musicSource == null || playlist == null || playlist.Length == 0)
                    return "[E] No Music";
                return isPlaying ? "[E] Turn Off Speaker" : "[E] Turn On Speaker";
            }
        }

        public void OnInteract(PlayerInteractionController controller)
        {
            if (musicSource == null || playlist == null || playlist.Length == 0)
                return;

            if (isPlaying)
                TurnOff();
            else
                TurnOn();
        }

        public void SetInteractable(bool interactable) { }

        public void TurnOn()
        {
            if (musicSource == null || playlist == null || playlist.Length == 0)
                return;

            musicSource.clip = playlist[currentTrack];
            musicSource.mute = false;
            if (!musicSource.isPlaying)
                musicSource.Play();
            isPlaying = true;
            StartCoroutine(TrackLoopRoutine());
        }

        public void TurnOff()
        {
            if (musicSource == null)
                return;

            musicSource.mute = true;
            isPlaying = false;
            StopAllCoroutines();
        }

        public void NextTrack()
        {
            if (playlist == null || playlist.Length == 0) return;
            currentTrack = (currentTrack + 1) % playlist.Length;
            if (isPlaying) TurnOn();
        }

        public void PreviousTrack()
        {
            if (playlist == null || playlist.Length == 0) return;
            currentTrack = (currentTrack - 1 + playlist.Length) % playlist.Length;
            if (isPlaying) TurnOn();
        }

        private System.Collections.IEnumerator TrackLoopRoutine()
        {
            while (isPlaying && musicSource != null && musicSource.clip != null)
            {
                yield return null;
                if (!musicSource.isPlaying && isPlaying)
                {
                    NextTrack();
                    yield break;
                }
            }
        }
    }
}