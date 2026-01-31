using System.Collections.Generic;
using UnityEngine;

namespace MAudio.Runtime
{
    /// <summary>
    /// 音频 Clip 提供方：按名称加载/查询 Clip，便于接入 Resources、Addressables 等。
    /// </summary>
    public interface IAudioClipProvider
    {
        /// <summary>是否包含指定名称的 Clip。</summary>
        bool HasClip(string clipName);

        /// <summary>按名称获取 Clip，未找到返回 null。</summary>
        AudioClip GetClip(string clipName);

        /// <summary>已提供的 Clip 名称列表（只读）。</summary>
        IReadOnlyList<string> GetClipNames();
    }
}