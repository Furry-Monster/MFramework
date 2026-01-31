using System.Collections.Generic;
using UnityEngine;

namespace MAudio.Runtime
{
    /// <summary>
    /// 从 Resources 目录加载 AudioClip 的默认提供方。
    /// </summary>
    public class ResourcesAudioClipProvider : IAudioClipProvider
    {
        private readonly string _resourcesPath;
        private readonly Dictionary<string, AudioClip> _clips = new();

        public ResourcesAudioClipProvider(string resourcesPath = "Audio")
        {
            _resourcesPath = string.IsNullOrEmpty(resourcesPath) ? "Audio" : resourcesPath;
            LoadAll();
        }

        private void LoadAll()
        {
            _clips.Clear();
            var loaded = Resources.LoadAll<AudioClip>(_resourcesPath);
            if (loaded == null)
                return;
            foreach (var clip in loaded)
                if (clip != null && !string.IsNullOrEmpty(clip.name))
                    _clips[clip.name] = clip;
        }

        public bool HasClip(string clipName)
        {
            return !string.IsNullOrEmpty(clipName) && _clips.ContainsKey(clipName);
        }

        public AudioClip GetClip(string clipName)
        {
            if (string.IsNullOrEmpty(clipName))
                return null;
            _clips.TryGetValue(clipName, out var clip);
            return clip;
        }

        public IReadOnlyList<string> GetClipNames()
        {
            var list = new List<string>(_clips.Count);
            foreach (var k in _clips.Keys)
                list.Add(k);
            return list;
        }

        /// <summary>重新从 Resources 加载（例如热更新后）。</summary>
        public void Reload()
        {
            LoadAll();
        }
    }
}