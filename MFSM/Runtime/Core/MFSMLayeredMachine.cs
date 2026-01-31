using System;
using System.Collections.Generic;

namespace MFSM.Runtime.Core
{
    /// <summary>分层状态机：多台机器按序每帧更新，每层将当前状态 Id 写回上下文供下一层读取。</summary>
    public sealed class MFSMLayeredMachine<TContext> where TContext : IMFSMLayeredContext
    {
        private readonly List<(string LayerId, MFSMMachine<TContext> Machine)> _layers = new();

        private TContext _context;

        #region 属性与查询

        public bool IsStarted { get; private set; }
        public bool IsPaused { get; set; }

        public MFSMMachine<TContext> GetMachine(string layerId)
        {
            if (string.IsNullOrEmpty(layerId))
                return null;
            foreach (var (id, machine) in _layers)
                if (id == layerId)
                    return machine;
            return null;
        }

        public string GetLayerCurrentStateId(string layerId)
        {
            var machine = GetMachine(layerId);
            return machine?.CurrentLeaf?.Id ?? string.Empty;
        }

        /// <summary>各层状态摘要，格式 "LayerId:StateId|..."。</summary>
        public string GetCurrentLayersString()
        {
            if (_layers.Count == 0)
                return string.Empty;
            var parts = new string[_layers.Count];
            for (var i = 0; i < _layers.Count; i++)
            {
                var (layerId, machine) = _layers[i];
                var stateId = machine.CurrentLeaf?.Id ?? string.Empty;
                parts[i] = $"{layerId}:{stateId}";
            }

            return string.Join("|", parts);
        }

        #endregion

        #region 上下文与层注册

        public void SetContext(TContext context) => _context = context;

        /// <summary>按序注册一层。同 layerId 覆盖。</summary>
        public void AddLayer(string layerId, MFSMMachine<TContext> machine)
        {
            if (string.IsNullOrEmpty(layerId))
                throw new ArgumentException("Layer id must be non-empty.", nameof(layerId));
            if (machine == null)
                throw new ArgumentNullException(nameof(machine));

            for (var i = 0; i < _layers.Count; i++)
            {
                if (_layers[i].LayerId == layerId)
                {
                    _layers[i] = (layerId, machine);
                    return;
                }
            }

            _layers.Add((layerId, machine));
        }

        public void RemoveLayer(string layerId)
        {
            if (string.IsNullOrEmpty(layerId))
                return;
            for (var i = _layers.Count - 1; i >= 0; i--)
            {
                if (_layers[i].LayerId == layerId)
                {
                    _layers.RemoveAt(i);
                    return;
                }
            }
        }

        #endregion

        #region 启动与驱动

        /// <summary>按序启动每层，每层进入 initialStates[layerId]。无层或缺少某层初始状态会抛错。</summary>
        public void Start(TContext context, IReadOnlyDictionary<string, string> initialStates)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (initialStates == null)
                throw new ArgumentNullException(nameof(initialStates));
            if (_layers.Count == 0)
                throw new InvalidOperationException(
                    "MFSMLayeredMachine: No layers registered. Call AddLayer before Start.");

            _context = context;
            foreach (var (layerId, machine) in _layers)
            {
                if (!initialStates.TryGetValue(layerId, out var initialStateId) || string.IsNullOrEmpty(initialStateId))
                    throw new InvalidOperationException(
                        $"MFSMLayeredMachine: No initial state for layer \"{layerId}\".");
                machine.SetContext(context);
                machine.Start(context, initialStateId);
            }

            IsStarted = true;
        }

        /// <summary>按序更新每层，每层更新后写回当前状态 Id 到上下文。</summary>
        public void Update(float deltaTime)
        {
            if (!IsStarted || IsPaused || deltaTime < 0f)
                return;

            foreach (var (layerId, machine) in _layers)
            {
                machine.SetContext(_context);
                machine.Update(deltaTime);
                _context.SetLayerOutput(layerId, machine.CurrentLeaf?.Id ?? string.Empty);
            }
        }

        #endregion

        #region 转换与重置

        public void TransitionLayerTo(string layerId, string toStateId) =>
            GetMachine(layerId)?.TransitionTo(toStateId);

        public void Reset()
        {
            foreach (var (_, machine) in _layers)
                machine.Reset();
            IsStarted = false;
            IsPaused = false;
        }

        #endregion
    }
}