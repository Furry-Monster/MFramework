using System.Collections.Generic;
using UnityEngine;

namespace MAudio.Runtime
{
    /// <summary>
    /// MAudio 操作句柄
    /// </summary>
    [RequireComponent(typeof(AudioPoolManager))]
    public class AudioHandler : MonoBehaviour
    {
        public static AudioHandler Instance { get; private set; }

        [Header("Volume")] [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 1f;
        [Range(0f, 1f)] public float effectVolume = 1f;
        [Range(0f, 1f)] public float uiVolume = 1f;
        [Range(0f, 1f)] public float voiceVolume = 1f;
        [Range(0f, 1f)] public float ambientVolume = 1f;

        [Header("Clip Source")] [Tooltip("为空则使用 Resources 下 Audio 目录")] [SerializeField]
        private string resourcesPath = "Audio";

        private IAudioClipProvider _clipProvider;
        private readonly Dictionary<string, AudioClip> _runtimeClips = new();
        private AudioPoolManager _pool;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[MAudio] Duplicate AudioHandler, destroying.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _pool = GetComponent<AudioPoolManager>();
            if (_pool == null)
                _pool = GetComponentInChildren<AudioPoolManager>();

            _clipProvider = new ResourcesAudioClipProvider(resourcesPath);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>设置自定义 Clip 提供方（如 Addressables）；null 则恢复默认 Resources。</summary>
        public void SetClipProvider(IAudioClipProvider provider)
        {
            _clipProvider = provider ?? new ResourcesAudioClipProvider(resourcesPath);
        }

        #region 播放

        /// <summary>按名称播放。</summary>
        public void Play(string clipName, SourceType sourceType = SourceType.Effect, float volume = 1f,
            float pitch = 1f, bool loop = false)
        {
            if (!TryGetClip(clipName, out var clip))
                return;
            PlayInternal(clip, sourceType, volume, pitch, loop, null);
        }

        /// <summary>直接播放 Clip。</summary>
        public void Play(AudioClip clip, SourceType sourceType = SourceType.Effect, float volume = 1f,
            float pitch = 1f, bool loop = false)
        {
            if (clip == null)
            {
                Debug.LogWarning("[MAudio] Cannot play null clip");
                return;
            }

            PlayInternal(clip, sourceType, volume, pitch, loop, null);
        }

        /// <summary>在指定位置播放（3D）。</summary>
        public void PlayAtPosition(string clipName, Vector3 position, SourceType sourceType = SourceType.Effect,
            float volume = 1f, float pitch = 1f, bool loop = false)
        {
            if (!TryGetClip(clipName, out var clip))
                return;
            PlayInternal(clip, sourceType, volume, pitch, loop, position);
        }

        /// <summary>在指定位置播放 Clip（3D）。</summary>
        public void PlayAtPosition(AudioClip clip, Vector3 position, SourceType sourceType = SourceType.Effect,
            float volume = 1f, float pitch = 1f, bool loop = false)
        {
            if (clip == null)
            {
                Debug.LogWarning("[MAudio] Cannot play null clip");
                return;
            }

            PlayInternal(clip, sourceType, volume, pitch, loop, position);
        }

        /// <summary>停止指定类型下所有正在播放的音频。</summary>
        public void StopAll(SourceType sourceType)
        {
            _pool?.StopAllAudioOfType(sourceType);
        }

        /// <summary>停止当前所有音乐（SourceType.Music）。</summary>
        public void StopMusic()
        {
            StopAll(SourceType.Music);
        }

        /// <summary>各类型池状态（只读），用于调试或 UI。</summary>
        public IReadOnlyList<AudioPoolTypeInfo> GetPoolInfo()
        {
            if (_pool == null) return new List<AudioPoolTypeInfo>();
            return _pool.GetPoolInfo();
        }

        private void PlayInternal(AudioClip clip, SourceType sourceType, float volume, float pitch, bool loop,
            Vector3? position)
        {
            if (_pool == null)
            {
                Debug.LogWarning("[MAudio] Pool not available");
                return;
            }

            var finalVolume = volume * GetVolumeForType(sourceType);
            if (position.HasValue)
                _pool.PlayAudioClipAtPosition(clip, position.Value, sourceType, finalVolume, pitch, loop);
            else
                _pool.PlayAudioClip(clip, sourceType, finalVolume, pitch, loop);
        }

        private bool TryGetClip(string clipName, out AudioClip clip)
        {
            clip = null;
            if (string.IsNullOrEmpty(clipName))
                return false;
            if (_runtimeClips.TryGetValue(clipName, out clip))
                return true;
            if (_clipProvider != null)
                clip = _clipProvider.GetClip(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"[MAudio] Clip '{clipName}' not found");
                return false;
            }

            return true;
        }

        #endregion

        #region 音量

        private float GetVolumeForType(SourceType sourceType)
        {
            var ch = sourceType switch
            {
                SourceType.Music => musicVolume,
                SourceType.Effect => effectVolume,
                SourceType.UI => uiVolume,
                SourceType.Voice => voiceVolume,
                SourceType.Ambient => ambientVolume,
                _ => 1f
            };
            return ch * masterVolume;
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            SetMixerParameter("MasterVolume", masterVolume);
        }

        public void SetMusicVolume(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            SetMixerParameter("MusicVolume", musicVolume);
        }

        public void SetEffectVolume(float v)
        {
            effectVolume = Mathf.Clamp01(v);
            SetMixerParameter("EffectVolume", effectVolume);
        }

        public void SetUIVolume(float v)
        {
            uiVolume = Mathf.Clamp01(v);
            SetMixerParameter("UIVolume", uiVolume);
        }

        public void SetVoiceVolume(float v)
        {
            voiceVolume = Mathf.Clamp01(v);
            SetMixerParameter("VoiceVolume", voiceVolume);
        }

        public void SetAmbientVolume(float v)
        {
            ambientVolume = Mathf.Clamp01(v);
            SetMixerParameter("AmbientVolume", ambientVolume);
        }

        private void SetMixerParameter(string param, float volume)
        {
            var mixer = _pool?.GetMasterMixerGroup()?.audioMixer;
            if (mixer == null) return;
            var db = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
            mixer.SetFloat(param, db);
        }

        #endregion

        #region 资源

        public bool HasClip(string clipName)
        {
            if (string.IsNullOrEmpty(clipName)) return false;
            return _runtimeClips.ContainsKey(clipName) || (_clipProvider != null && _clipProvider.HasClip(clipName));
        }

        public AudioClip GetClip(string clipName)
        {
            if (string.IsNullOrEmpty(clipName)) return null;
            if (_runtimeClips.TryGetValue(clipName, out var clip)) return clip;
            return _clipProvider?.GetClip(clipName);
        }

        /// <summary>运行时注册 Clip；key 或 clip 为空则忽略。</summary>
        public void RegisterClip(string key, AudioClip clip)
        {
            if (string.IsNullOrEmpty(key) || clip == null) return;
            _runtimeClips[key] = clip;
        }

        /// <summary>运行时移除已注册的 Clip。</summary>
        public void UnregisterClip(string key)
        {
            if (!string.IsNullOrEmpty(key)) _runtimeClips.Remove(key);
        }

        public IReadOnlyList<string> GetClipNames()
        {
            var set = new HashSet<string>();
            foreach (var k in _runtimeClips.Keys) set.Add(k);
            if (_clipProvider != null)
                foreach (var n in _clipProvider.GetClipNames())
                    set.Add(n);
            var list = new List<string>(set);
            return list;
        }

        #endregion
    }
}