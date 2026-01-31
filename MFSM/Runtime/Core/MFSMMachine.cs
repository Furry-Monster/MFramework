using System;
using System.Collections.Generic;

namespace MFSM.Runtime.Core
{
    /// <summary>层次状态机。支持状态树、复合状态、条件转换与任意状态转换。</summary>
    public sealed class MFSMMachine<TContext> where TContext : IMFSMContext
    {
        private readonly Dictionary<string, IMFSMState<TContext>> _states = new();
        private readonly Dictionary<string, string> _parentId = new();
        private readonly List<MFSMTransitionEntry> _transitions = new();
        private readonly List<IMFSMState<TContext>> _currentPath = new();
        private string _rootId;
        private TContext _context;

        /// <summary>当前叶子状态；未启动或路径为空为 null。</summary>
        public IMFSMState<TContext> CurrentLeaf => _currentPath.Count > 0 ? _currentPath[^1] : null;

        /// <summary>从根到当前叶子的路径。</summary>
        public IReadOnlyList<IMFSMState<TContext>> CurrentPath => _currentPath;

        /// <summary>是否已启动。</summary>
        public bool IsStarted { get; private set; }

        /// <summary>是否暂停（Update 不执行）。</summary>
        public bool IsPaused { get; private set; }

        /// <summary>进入状态时触发，参数为状态 Id。</summary>
        public event Action<string> OnStateEntered;

        /// <summary>离开状态时触发，参数为状态 Id。</summary>
        public event Action<string> OnStateExited;

        /// <summary>设置共享上下文。</summary>
        public void SetContext(TContext context)
        {
            _context = context;
        }

        /// <summary>注册状态。同 Id 覆盖。根状态传 parentStateId=null 且 isRoot=true。</summary>
        public void RegisterState(IMFSMState<TContext> state, string parentStateId = null, bool isRoot = false)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (string.IsNullOrEmpty(state.Id))
                throw new ArgumentException("State Id must be non-empty.", nameof(state));
            if (state.Id == MFSMConstants.AnyStateId)
                throw new ArgumentException($"State Id must not be reserved: {MFSMConstants.AnyStateId}",
                    nameof(state));

            _states[state.Id] = state;
            _parentId[state.Id] = parentStateId;

            if (isRoot && string.IsNullOrEmpty(parentStateId) && string.IsNullOrEmpty(_rootId))
                _rootId = state.Id;
        }

        /// <summary>添加转换。from 为当前路径中某状态或其子状态时，condition 为 true 则转换。priority 大者先检查。</summary>
        public void AddTransition(string fromStateId, string toStateId,
            IMFSMTransitionCondition<TContext> condition = null, int priority = 0)
        {
            if (string.IsNullOrEmpty(fromStateId) || string.IsNullOrEmpty(toStateId))
                throw new ArgumentException("From and To state ids must be non-empty.");
            if (fromStateId != MFSMConstants.AnyStateId && !_states.ContainsKey(fromStateId))
                throw new ArgumentException($"State not found: {fromStateId}", nameof(fromStateId));
            if (!_states.ContainsKey(toStateId))
                throw new ArgumentException($"State not found: {toStateId}", nameof(toStateId));

            _transitions.Add(new MFSMTransitionEntry(fromStateId, toStateId, condition, priority));
            _transitions.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        /// <summary>使用委托添加转换。</summary>
        public void AddTransition(string fromStateId, string toStateId, Func<TContext, bool> condition,
            int priority = 0)
        {
            AddTransition(fromStateId, toStateId,
                condition != null ? new MFSMTransitionCondition<TContext>(condition) : null, priority);
        }

        /// <summary>任意状态转换（如死亡、受击）。priority 建议设大。</summary>
        public void AddAnyTransition(string toStateId,
            IMFSMTransitionCondition<TContext> condition = null, int priority = 0)
        {
            AddTransition(MFSMConstants.AnyStateId, toStateId, condition, priority);
        }

        /// <summary>使用委托添加任意状态转换。</summary>
        public void AddAnyTransition(string toStateId, Func<TContext, bool> condition, int priority = 0)
        {
            AddTransition(MFSMConstants.AnyStateId, toStateId, condition, priority);
        }

        /// <summary>启动并进入指定状态（复合状态会展开到叶子）。</summary>
        public void Start(TContext context, string initialStateId)
        {
            if (string.IsNullOrEmpty(initialStateId))
                throw new ArgumentException("Initial state id must be non-empty.", nameof(initialStateId));
            if (!_states.ContainsKey(initialStateId))
                throw new ArgumentException($"State not found: {initialStateId}", nameof(initialStateId));

            _context = context;
            _currentPath.Clear();
            var path = GetPathToLeaf(initialStateId);
            if (path == null || path.Count == 0)
                throw new InvalidOperationException(
                    $"MFSM: GetPathToLeaf(\"{initialStateId}\") returned empty. Check compound state InitialSubStateId and parent chain for cycles.");
            foreach (var s in path)
            {
                s.OnEnter(_context);
                _currentPath.Add(s);
                OnStateEntered?.Invoke(s.Id);
            }

            IsStarted = true;
        }

        /// <summary>从根状态启动（根为复合状态会展开到叶子）。</summary>
        public void StartWithRoot(TContext context)
        {
            if (string.IsNullOrEmpty(_rootId))
                throw new InvalidOperationException(
                    "MFSM: No root state registered. Register a state with isRoot: true or call SetRoot(id).");
            Start(context, _rootId);
        }

        /// <summary>设置根状态 Id。</summary>
        public void SetRoot(string rootStateId)
        {
            if (string.IsNullOrEmpty(rootStateId))
                throw new ArgumentException("Root state id must be non-empty.", nameof(rootStateId));
            if (!_states.ContainsKey(rootStateId))
                throw new ArgumentException($"State not found: {rootStateId}", nameof(rootStateId));
            _rootId = rootStateId;
        }

        /// <summary>暂停。</summary>
        public void Pause()
        {
            IsPaused = true;
        }

        /// <summary>恢复。</summary>
        public void Resume()
        {
            IsPaused = false;
        }

        /// <summary>每帧驱动：先检查转换再更新叶子。在 Update 中调用，deltaTime &gt;= 0。</summary>
        public void Update(float deltaTime)
        {
            if (!IsStarted || _currentPath.Count == 0)
                return;
            if (IsPaused || deltaTime < 0f)
                return;

            var currentIds = new HashSet<string> { MFSMConstants.AnyStateId };
            foreach (var s in _currentPath)
                currentIds.Add(s.Id);

            foreach (var t in _transitions)
            {
                if (!currentIds.Contains(t.FromStateId))
                    continue;
                if (t.Condition != null && !t.Condition.Evaluate(_context))
                    continue;

                PerformTransition(t.ToStateId);
                return;
            }

            var leaf = _currentPath[^1];
            leaf.OnUpdate(_context, deltaTime);
        }

        /// <summary>强制切换到目标状态（不检查条件）。</summary>
        public void TransitionTo(string toStateId)
        {
            if (!IsStarted)
                return;
            if (string.IsNullOrEmpty(toStateId) || !_states.ContainsKey(toStateId))
                return;
            PerformTransition(toStateId);
        }

        private void PerformTransition(string toStateId)
        {
            var targetPath = GetPathToLeaf(toStateId);
            if (targetPath == null || targetPath.Count == 0)
                return;

            var commonIndex = 0;
            var minLen = Math.Min(_currentPath.Count, targetPath.Count);
            while (commonIndex < minLen && _currentPath[commonIndex].Id == targetPath[commonIndex].Id)
                commonIndex++;

            for (var i = _currentPath.Count - 1; i >= commonIndex; i--)
            {
                var id = _currentPath[i].Id;
                _currentPath[i].OnExit(_context);
                OnStateExited?.Invoke(id);
            }

            _currentPath.Clear();
            for (var i = commonIndex; i < targetPath.Count; i++)
            {
                targetPath[i].OnEnter(_context);
                _currentPath.Add(targetPath[i]);
                OnStateEntered?.Invoke(targetPath[i].Id);
            }
        }

        /// <summary> 是否已注册指定 Id 的状态。 </summary>
        public bool HasState(string stateId)
        {
            return !string.IsNullOrEmpty(stateId) && _states.ContainsKey(stateId);
        }

        /// <summary> 当前叶子状态是否等于指定 Id。 </summary>
        public bool IsInState(string stateId)
        {
            if (string.IsNullOrEmpty(stateId) || _currentPath.Count == 0)
                return false;
            return _currentPath[^1].Id == stateId;
        }

        /// <summary>当前路径是否包含该状态或其子状态。</summary>
        public bool IsInStateOrDescendant(string stateId)
        {
            if (string.IsNullOrEmpty(stateId) || _currentPath.Count == 0)
                return false;
            foreach (var s in _currentPath)
                if (s.Id == stateId)
                    return true;

            return false;
        }

        /// <summary>从根到该状态的路径（不展开复合）。父链有环则返回空。</summary>
        public IReadOnlyList<IMFSMState<TContext>> GetPathToState(string stateId)
        {
            if (!_states.TryGetValue(stateId, out var start))
                return Array.Empty<IMFSMState<TContext>>();
            var path = new List<IMFSMState<TContext>>();
            var visited = new HashSet<string>();
            var s = start;
            var steps = 0;
            while (s != null && steps < MFSMConfig.MaxParentChainDepth)
            {
                if (!visited.Add(s.Id))
                    return Array.Empty<IMFSMState<TContext>>();
                path.Add(s);
                if (!_parentId.TryGetValue(s.Id, out var pid) || string.IsNullOrEmpty(pid))
                    break;
                s = _states.GetValueOrDefault(pid);
                steps++;
            }

            path.Reverse();
            return path;
        }

        /// <summary>从根到叶子的路径（复合状态展开到 InitialSubStateId）。防环与过深。</summary>
        public IReadOnlyList<IMFSMState<TContext>> GetPathToLeaf(string stateId)
        {
            var path = new List<IMFSMState<TContext>>();
            var pathToState = GetPathToState(stateId);
            if (pathToState.Count == 0)
                return path;
            path.AddRange(pathToState);
            var expanded = 0;
            while (path.Count > 0 && path[^1].IsCompound && expanded < MFSMConfig.MaxCompoundExpansionDepth)
            {
                var subId = path[^1].InitialSubStateId;
                if (string.IsNullOrEmpty(subId) || !_states.TryGetValue(subId, out var subState))
                    break;
                for (var i = 0; i < path.Count; i++)
                    if (path[i].Id == subId)
                        return path;

                path.Add(subState);
                expanded++;
            }

            return path;
        }

        /// <summary>获取已注册状态；不存在返回 null。</summary>
        public IMFSMState<TContext> GetState(string stateId)
        {
            return string.IsNullOrEmpty(stateId)
                ? null
                : _states.GetValueOrDefault(stateId);
        }

        /// <summary>重置：退出当前路径、清空并解除暂停。</summary>
        public void Reset()
        {
            IsPaused = false;
            for (var i = _currentPath.Count - 1; i >= 0; i--)
            {
                var id = _currentPath[i].Id;
                _currentPath[i].OnExit(_context);
                OnStateExited?.Invoke(id);
            }

            _currentPath.Clear();
            IsStarted = false;
        }

        /// <summary>当前路径的 Id 序列，如 "Root > Locomotion > Walk"。</summary>
        public string GetCurrentPathString()
        {
            if (_currentPath.Count == 0)
                return string.Empty;
            if (_currentPath.Count == 1)
                return _currentPath[0].Id;
            var sb = new System.Text.StringBuilder(_currentPath[0].Id);
            for (var i = 1; i < _currentPath.Count; i++)
            {
                sb.Append(" > ");
                sb.Append(_currentPath[i].Id);
            }

            return sb.ToString();
        }

        private readonly struct MFSMTransitionEntry
        {
            public readonly string FromStateId;
            public readonly string ToStateId;
            public readonly IMFSMTransitionCondition<TContext> Condition;
            public readonly int Priority;

            public MFSMTransitionEntry(string from, string to, IMFSMTransitionCondition<TContext> condition,
                int priority)
            {
                FromStateId = from;
                ToStateId = to;
                Condition = condition;
                Priority = priority;
            }
        }
    }
}