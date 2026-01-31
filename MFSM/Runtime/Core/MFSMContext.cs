using System.Collections.Generic;

namespace MFSM.Runtime.Core
{
    #region 基础上下文

    /// <summary>状态机共享上下文。推荐 struct 减分配。</summary>
    public interface IMFSMContext
    {
    }

    public readonly struct MFSMEmptyContext : IMFSMContext
    {
    }

    #endregion

    #region 分层上下文

    /// <summary>分层机上下文：每层运行后写回当前状态 Id，供下一层条件读取。</summary>
    public interface IMFSMLayeredContext : IMFSMContext
    {
        void SetLayerOutput(string layerId, string stateId);
        string GetLayerOutput(string layerId);
    }

    /// <summary>分层上下文基类，字典存每层输出。</summary>
    public class MFSMLayeredContextBase : IMFSMLayeredContext
    {
        private readonly Dictionary<string, string> _layerOutputs = new();

        public void SetLayerOutput(string layerId, string stateId)
        {
            if (string.IsNullOrEmpty(layerId))
                return;
            _layerOutputs[layerId] = stateId ?? string.Empty;
        }

        public string GetLayerOutput(string layerId)
        {
            if (string.IsNullOrEmpty(layerId))
                return string.Empty;
            return _layerOutputs.TryGetValue(layerId, out var id) ? id : string.Empty;
        }
    }

    #endregion
}