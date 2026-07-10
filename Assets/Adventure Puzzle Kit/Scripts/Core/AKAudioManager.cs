using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

namespace AdventurePuzzleKit
{
    public class AKAudioManager : MonoBehaviour
    {
        [Header("Should persist?")]
        [Tooltip("If true, this Audio Manager will stay active between scenes.")]
        [SerializeField] private bool persistAcrossScenes = true;

        [Header("List of Sound Effect SO's")]
        [Tooltip("Add your Sound ScriptableObjects here.")]
        [SerializeField] private Sound[] sounds = null;

        [Header("Sound Mixer Group")]
        [Tooltip("Optional: Set an Audio Mixer Group to route all sound through.")]
        [SerializeField] private AudioMixerGroup mixerGroup = null;

        public static AKAudioManager instance;

        void Awake()
        {
            // Make sure there's only one instance of the AudioManager
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;

                // Keep this object between scenes if needed
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }

            // Create audio sources for each sound
            foreach (Sound s in sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.loop = s.loop;
                s.source.outputAudioMixerGroup = mixerGroup;
            }
        }

        public void Play(Sound sound)
        {
            // Find the matching sound
            Sound s = sounds.FirstOrDefault(item => item == sound);

            if (s == null)
            {
                Debug.LogWarning("Sound: " + sound + " not found!");
                return;
            }

            // Add variation to volume and pitch
            s.source.volume = s.volume * (1f + Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
            s.source.pitch = s.pitch * (1f + Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

            s.source.Play();
        }

        public void StopPlaying(Sound sound)
        {
            // Find and stop the sound
            Sound s = sounds.FirstOrDefault(item => item == sound);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }

            s.source.volume = s.volume * (1f + Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
            s.source.pitch = s.pitch * (1f + Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
            s.source.Stop();
        }

        public bool IsPlaying(Sound sound)
        {
            // Check if a sound is currently playing
            Sound s = sounds.FirstOrDefault(item => item == sound);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return false;
            }

            return s.source.isPlaying;
        }

        public void PausePlaying(Sound sound)
        {
            // Pause the sound
            Sound s = sounds.FirstOrDefault(item => item == sound);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }

            s.source.volume = s.volume * (1f + Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
            s.source.pitch = s.pitch * (1f + Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
            s.source.Pause();
        }

        public void StopAll()
        {
            // Stop all sounds
            foreach (Sound s in sounds)
            {
                s.source.Stop();
            }
        }

        public IEnumerator FadeOut(Sound sound, float fadeTime)
        {
            // Gradually reduce volume to zero, then stop the sound
            Sound s = sounds.FirstOrDefault(item => item == sound);

            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                yield break;
            }

            float startVolume = s.source.volume;

            while (s.source.volume > 0)
            {
                s.source.volume -= startVolume * Time.deltaTime / fadeTime;
                yield return null;
            }

            s.source.Stop();
            s.source.volume = startVolume;
        }
    }
}

