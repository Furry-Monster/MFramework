using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MAudio.Runtime
{
    public enum SourceType
    {
        Ambient,
        Music,
        UI,
        Effect,
        Voice
    }

    /// <summary>单类型池状态，用于查询或 Editor 显示。</summary>
    public readonly struct AudioPoolTypeInfo
    {
        public SourceType Type { get; }
        public int IdleCount { get; }
        public int UsedCount { get; }
        public int TotalCount { get; }

        public AudioPoolTypeInfo(SourceType type, int idle, int used, int total)
        {
            Type = type;
            IdleCount = idle;
            UsedCount = used;
            TotalCount = total;
        }
    }

    [Serializable]
    public class AudioPoolConfig
    {
        [Header("Pool Settings")] public int defaultPoolSize = 10;
        public int maxSourcesPerType = 50;
        public bool autoExpand = true;

        [Header("Audio Source Settings")] public float defaultVolume = 1.0f;
        public float defaultPitch = 1.0f;
        public bool defaultLoop = false;
        public AudioRolloffMode defaultRolloffMode = AudioRolloffMode.Logarithmic;
        public float defaultMinDistance = 1.0f;
        public float defaultMaxDistance = 500.0f;
    }

    public class AudioSourcePool : IDisposable
    {
        private readonly AudioPoolConfig config;
        private readonly Dictionary<SourceType, Queue<AudioSource>> idleSources = new();
        private readonly Dictionary<SourceType, HashSet<AudioSource>> usedSources = new();
        private readonly Dictionary<SourceType, int> sourceCounts = new();
        private readonly GameObject poolParent;

        public AudioSourcePool(AudioPoolConfig config = null)
        {
            this.config = config ?? new AudioPoolConfig();

            poolParent = new GameObject("AudioSourcePool");
            Object.DontDestroyOnLoad(poolParent);

            InitializePools();
        }

        private void InitializePools()
        {
            try
            {
                foreach (SourceType type in Enum.GetValues(typeof(SourceType)))
                {
                    idleSources[type] = new Queue<AudioSource>();
                    usedSources[type] = new HashSet<AudioSource>();
                    sourceCounts[type] = 0;

                    for (var i = 0; i < config.defaultPoolSize; i++)
                        CreateNewSource(type);
                }

                Debug.Log($"[AudioSourcePool] Initialized with {config.defaultPoolSize} sources per type");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AudioSourcePool] Failed to initialize: {e.Message}");
            }
        }

        public AudioSource GetSource(SourceType sourceType)
        {
            if (!idleSources.ContainsKey(sourceType))
            {
                Debug.LogError($"[AudioSourcePool] Unknown source type: {sourceType}");
                return null;
            }

            AudioSource source = null;

            if (idleSources[sourceType].Count > 0)
            {
                source = idleSources[sourceType].Dequeue();
            }
            else if (config.autoExpand && sourceCounts[sourceType] < config.maxSourcesPerType)
            {
                source = CreateNewSource(sourceType);
            }
            else if (usedSources[sourceType].Count >= config.maxSourcesPerType)
            {
                Debug.LogWarning(
                    $"[AudioSourcePool] Maximum sources reached for {sourceType}. Consider increasing maxSourcesPerType.");
                return null;
            }

            if (source != null)
            {
                usedSources[sourceType].Add(source);
                source.gameObject.SetActive(true);
            }

            return source;
        }

        public void ReturnSource(AudioSource source, SourceType sourceType)
        {
            if (source == null)
            {
                Debug.LogWarning("[AudioSourcePool] Attempted to return null source");
                return;
            }

            if (!usedSources.ContainsKey(sourceType) || !usedSources[sourceType].Contains(source))
            {
                Debug.LogWarning($"[AudioSourcePool] Source {source.name} is not in used pool for type {sourceType}");
                return;
            }

            ResetAudioSource(source);

            usedSources[sourceType].Remove(source);
            idleSources[sourceType].Enqueue(source);

            source.gameObject.SetActive(false);
        }

        /// <summary>停止并归还指定类型下所有正在使用的源。</summary>
        public void StopAllOfType(SourceType sourceType)
        {
            if (!usedSources.ContainsKey(sourceType))
                return;
            var list = new List<AudioSource>(usedSources[sourceType]);
            foreach (var source in list)
                if (source != null)
                    ReturnSource(source, sourceType);
        }

        /// <summary>各类型池状态（只读）。</summary>
        public IReadOnlyList<AudioPoolTypeInfo> GetPoolInfo()
        {
            var list = new List<AudioPoolTypeInfo>();
            foreach (SourceType type in Enum.GetValues(typeof(SourceType)))
            {
                if (!idleSources.ContainsKey(type)) continue;
                list.Add(new AudioPoolTypeInfo(type,
                    idleSources[type].Count,
                    usedSources[type].Count,
                    sourceCounts[type]));
            }

            return list;
        }

        private AudioSource CreateNewSource(SourceType sourceType)
        {
            var sourceObj = new GameObject($"AudioSource_{sourceType}_{sourceCounts[sourceType]}");
            sourceObj.transform.SetParent(poolParent.transform);

            var audioSource = sourceObj.AddComponent<AudioSource>();
            ConfigureAudioSource(audioSource, sourceType);

            idleSources[sourceType].Enqueue(audioSource);
            sourceCounts[sourceType]++;

            sourceObj.SetActive(false);

            return audioSource;
        }

        private void ConfigureAudioSource(AudioSource audioSource, SourceType sourceType)
        {
            audioSource.volume = config.defaultVolume;
            audioSource.pitch = config.defaultPitch;
            audioSource.loop = config.defaultLoop;
            audioSource.rolloffMode = config.defaultRolloffMode;
            audioSource.minDistance = config.defaultMinDistance;
            audioSource.maxDistance = config.defaultMaxDistance;

            switch (sourceType)
            {
                case SourceType.Music:
                    audioSource.priority = 0; // Highest priority
                    audioSource.loop = true;
                    break;
                case SourceType.Voice:
                    audioSource.priority = 1;
                    break;
                case SourceType.UI:
                    audioSource.priority = 2;
                    audioSource.spatialBlend = 0f; // 2D sound
                    break;
                case SourceType.Effect:
                    audioSource.priority = 3;
                    break;
                case SourceType.Ambient:
                    audioSource.priority = 4;
                    audioSource.loop = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceType), sourceType, null);
            }
        }

        private void ResetAudioSource(AudioSource audioSource)
        {
            audioSource.clip = null;
            audioSource.volume = config.defaultVolume;
            audioSource.pitch = config.defaultPitch;
            audioSource.loop = config.defaultLoop;
            audioSource.time = 0f;
            audioSource.Stop();
        }

        public void ClearPool()
        {
            foreach (var type in Enum.GetValues(typeof(SourceType)).Cast<SourceType>())
            {
                while (idleSources[type].Count > 0)
                {
                    var source = idleSources[type].Dequeue();
                    if (source != null)
                        Object.Destroy(source.gameObject);
                }

                foreach (var source in usedSources[type].ToList().Where(source => source is not null))
                    Object.Destroy(source.gameObject);

                usedSources[type].Clear();

                sourceCounts[type] = 0;
            }
        }

        public void Dispose()
        {
            ClearPool();
            if (poolParent != null)
                Object.Destroy(poolParent);
        }

        public void LogPoolStatus()
        {
            Debug.Log("=== AudioSourcePool Status ===");
            foreach (var type in Enum.GetValues(typeof(SourceType)).Cast<SourceType>())
                Debug.Log(
                    $"{type}: Idle={idleSources[type].Count}, Used={usedSources[type].Count}, Total={sourceCounts[type]}");
        }
    }
}