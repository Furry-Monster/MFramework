using UnityEngine;

namespace MAudio.Runtime
{
    [CreateAssetMenu(fileName = "AudioPoolConfig", menuName = "MAudio/Audio Pool Config", order = 1)]
    public class AudioPoolConfigAsset : ScriptableObject
    {
        [Header("Pool Settings")] [Tooltip("Default number of audio sources to create for each type")]
        public int defaultPoolSize = 10;

        [Tooltip("Maximum number of audio sources allowed per type")]
        public int maxSourcesPerType = 50;

        [Tooltip("Whether to create new sources when pool is empty")]
        public bool expandPoolWhenNeeded = true;

        [Header("Audio Source Default Settings")] [Range(0f, 1f)] [Tooltip("Default volume for audio sources")]
        public float defaultVolume = 1.0f;

        [Range(0.1f, 3f)] [Tooltip("Default pitch for audio sources")]
        public float defaultPitch = 1.0f;

        [Tooltip("Default loop setting for audio sources")]
        public bool defaultLoop = false;

        [Tooltip("Default rolloff mode for 3D audio")]
        public AudioRolloffMode defaultRolloffMode = AudioRolloffMode.Logarithmic;

        [Tooltip("Default minimum distance for 3D audio")]
        public float defaultMinDistance = 1.0f;

        [Tooltip("Default maximum distance for 3D audio")]
        public float defaultMaxDistance = 500.0f;

        [Header("Type-Specific Settings")] [Tooltip("Volume multiplier for music sources")] [Range(0f, 2f)]
        public float musicVolumeMultiplier = 1.0f;

        [Tooltip("Volume multiplier for effect sources")] [Range(0f, 2f)]
        public float effectVolumeMultiplier = 1.0f;

        [Tooltip("Volume multiplier for UI sources")] [Range(0f, 2f)]
        public float uiVolumeMultiplier = 1.0f;

        [Tooltip("Volume multiplier for voice sources")] [Range(0f, 2f)]
        public float voiceVolumeMultiplier = 1.0f;

        [Tooltip("Volume multiplier for ambient sources")] [Range(0f, 2f)]
        public float ambientVolumeMultiplier = 1.0f;

        /// <summary>
        /// Converts this ScriptableObject to an AudioPoolConfig instance
        /// </summary>
        public AudioPoolConfig ToAudioPoolConfig()
        {
            return new AudioPoolConfig
            {
                defaultPoolSize = defaultPoolSize,
                maxSourcesPerType = maxSourcesPerType,
                autoExpand = expandPoolWhenNeeded,
                defaultVolume = defaultVolume,
                defaultPitch = defaultPitch,
                defaultLoop = defaultLoop,
                defaultRolloffMode = defaultRolloffMode,
                defaultMinDistance = defaultMinDistance,
                defaultMaxDistance = defaultMaxDistance
            };
        }

        /// <summary>
        /// Gets the volume multiplier for a specific source type
        /// </summary>
        public float GetVolumeMultiplier(SourceType sourceType)
        {
            return sourceType switch
            {
                SourceType.Music => musicVolumeMultiplier,
                SourceType.Effect => effectVolumeMultiplier,
                SourceType.UI => uiVolumeMultiplier,
                SourceType.Voice => voiceVolumeMultiplier,
                SourceType.Ambient => ambientVolumeMultiplier,
                _ => 1.0f
            };
        }

        private void OnValidate()
        {
            defaultPoolSize = Mathf.Max(1, defaultPoolSize);
            maxSourcesPerType = Mathf.Max(defaultPoolSize, maxSourcesPerType);
            defaultMinDistance = Mathf.Max(0.1f, defaultMinDistance);
            defaultMaxDistance = Mathf.Max(defaultMinDistance, defaultMaxDistance);
        }
    }
}