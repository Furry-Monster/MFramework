using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MAudio.Runtime
{
    [RequireComponent(typeof(AudioHandler))]
    public class AudioPoolManager : MonoBehaviour
    {
        public static AudioPoolManager Instance { get; private set; }

        [Header("Pool Configuration")] [SerializeField]
        private AudioPoolConfig defaultPoolConfig;

        [SerializeField] private AudioPoolConfigAsset poolConfigOverride;

        [Header("Audio Mixer Groups")] [SerializeField]
        private AudioMixerGroup masterMixerGroup;

        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private AudioMixerGroup effectMixerGroup;
        [SerializeField] private AudioMixerGroup uiMixerGroup;
        [SerializeField] private AudioMixerGroup voiceMixerGroup;
        [SerializeField] private AudioMixerGroup ambientMixerGroup;

        private AudioSourcePool audioSourcePool;
        private bool isInitialized = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[MAudio] Duplicate AudioPoolManager, destroying.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }

        private void InitializePool()
        {
            if (isInitialized)
                return;

            try
            {
                if (poolConfigOverride is not null) defaultPoolConfig = poolConfigOverride.ToAudioPoolConfig();

                if (defaultPoolConfig == null)
                {
                    defaultPoolConfig = new AudioPoolConfig();
                    Debug.LogWarning("[AudioPoolManager] No pool config assigned, using default settings");
                }

                audioSourcePool = new AudioSourcePool(defaultPoolConfig);
                isInitialized = true;

                Debug.Log("[AudioPoolManager] Audio source pool initialized successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AudioPoolManager] Failed to initialize audio pool: {e.Message}");
            }
        }

        public AudioSource GetAudioSource(SourceType sourceType)
        {
            if (!isInitialized)
            {
                Debug.LogError("[AudioPoolManager] Pool not initialized");
                return null;
            }

            return audioSourcePool.GetSource(sourceType);
        }

        public void ReturnAudioSource(AudioSource source, SourceType sourceType)
        {
            if (!isInitialized)
            {
                Debug.LogError("[AudioPoolManager] Pool not initialized");
                return;
            }

            audioSourcePool.ReturnSource(source, sourceType);
        }

        public void PlayAudioClip(AudioClip clip, SourceType sourceType, float volume = 1f, float pitch = 1f,
            bool loop = false)
        {
            if (clip is null)
            {
                Debug.LogWarning("[AudioPoolManager] Cannot play null audio clip");
                return;
            }

            var source = GetAudioSource(sourceType);
            if (source is null)
            {
                Debug.LogWarning($"[AudioPoolManager] Could not get audio source for type {sourceType}");
                return;
            }

            var mixerGroup = GetMixerGroup(sourceType);
            source.outputAudioMixerGroup = mixerGroup;

            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.loop = loop;

            source.Play();

            if (!loop) StartCoroutine(ReturnSourceWhenFinished(source, sourceType, clip.length / pitch));
        }

        public void PlayAudioClipAtPosition(AudioClip clip, Vector3 position, SourceType sourceType, float volume = 1f,
            float pitch = 1f, bool loop = false)
        {
            if (clip is null)
            {
                Debug.LogWarning("[AudioPoolManager] Cannot play null audio clip");
                return;
            }

            var source = GetAudioSource(sourceType);
            if (source is null)
            {
                Debug.LogWarning($"[AudioPoolManager] Could not get audio source for type {sourceType}");
                return;
            }

            var mixerGroup = GetMixerGroup(sourceType);
            source.outputAudioMixerGroup = mixerGroup;

            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.loop = loop;
            source.transform.position = position;
            source.spatialBlend = 1f; // 3D sound

            source.Play();

            if (!loop) StartCoroutine(ReturnSourceWhenFinished(source, sourceType, clip.length / pitch));
        }

        private IEnumerator ReturnSourceWhenFinished(AudioSource source, SourceType sourceType, float duration)
        {
            yield return new WaitForSeconds(duration);
            ReturnAudioSource(source, sourceType);
        }

        private AudioMixerGroup GetMixerGroup(SourceType sourceType)
        {
            return sourceType switch
            {
                SourceType.Music => musicMixerGroup,
                SourceType.Effect => effectMixerGroup,
                SourceType.UI => uiMixerGroup,
                SourceType.Voice => voiceMixerGroup,
                SourceType.Ambient => ambientMixerGroup,
                _ => masterMixerGroup
            };
        }

        public AudioMixerGroup GetMasterMixerGroup()
        {
            return masterMixerGroup;
        }

        /// <summary>停止指定类型下所有正在播放的音频并归还到池。</summary>
        public void StopAllAudioOfType(SourceType sourceType)
        {
            if (!isInitialized) return;
            audioSourcePool.StopAllOfType(sourceType);
        }

        /// <summary>各类型池状态（只读）。</summary>
        public IReadOnlyList<AudioPoolTypeInfo> GetPoolInfo()
        {
            if (!isInitialized) return Array.Empty<AudioPoolTypeInfo>();
            return audioSourcePool.GetPoolInfo();
        }

        public void SetPoolConfig(AudioPoolConfig newConfig)
        {
            if (isInitialized)
            {
                Debug.LogWarning("[AudioPoolManager] Cannot change pool config after initialization");
                return;
            }

            defaultPoolConfig = newConfig;
        }

        public void LogPoolStatus()
        {
            if (isInitialized)
                audioSourcePool.LogPoolStatus();
            else
                Debug.LogWarning("[AudioPoolManager] Pool not initialized");
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            audioSourcePool?.Dispose();
        }

        [ContextMenu("Log Pool Status")]
        private void LogPoolStatusEditor()
        {
            LogPoolStatus();
        }
    }
}