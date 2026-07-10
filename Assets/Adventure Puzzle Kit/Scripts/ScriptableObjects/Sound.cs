using UnityEngine;
using UnityEngine.Audio;

namespace AdventurePuzzleKit
{
    public enum SoundCategory
    {
        General,
        Keypad,
        Phone,
        FuseBox,
        Chess,
        Note,
        Safe,
        Valve,
        ThemedKey,
        Padlock,
        Lever, 
        Generator,
        GasMask,
        Flashlight,
        Examine,
        Keycard,
        Uncategorized
    }

    [CreateAssetMenu(fileName = "New Sound", menuName = "AdventurePuzzleKit/Sound", order = 0)]
    public class Sound : ScriptableObject
    {
        [Header("Sound Settings")]
        public SoundCategory category = SoundCategory.Uncategorized; // Default category

        [Space(10)]
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 0.1f;

        [Range(0f, 1f)]
        public float volumeVariance = 0.1f;

        [Range(0.1f, 3f)]
        public float pitch = 1f;

        [Range(0f, 1f)]
        public float pitchVariance = 0.1f;

        [Header("Playback Settings")]
        public bool loop = false;
        public bool isPlaying = false;
        public AudioMixerGroup mixerGroup;

        [HideInInspector]
        public AudioSource source;
    }
}
