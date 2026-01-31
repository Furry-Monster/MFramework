using System;

namespace MFSM.Runtime.Core
{
    /// <summary>链式构建 MFSMMachine。</summary>
    public sealed class MFSMBuilder<TContext> where TContext : IMFSMContext
    {
        private readonly MFSMMachine<TContext> _machine = new();
        private string _rootId;

        public MFSMBuilder<TContext> AddRootState(IMFSMState<TContext> state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            _machine.RegisterState(state, null, true);
            if (string.IsNullOrEmpty(_rootId))
                _rootId = state.Id;
            return this;
        }

        public MFSMBuilder<TContext> AddState(IMFSMState<TContext> state, string parentStateId)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            _machine.RegisterState(state, parentStateId);
            return this;
        }

        public MFSMBuilder<TContext> AddTransition(string fromStateId, string toStateId,
            IMFSMTransitionCondition<TContext> condition = null, int priority = 0)
        {
            _machine.AddTransition(fromStateId, toStateId, condition, priority);
            return this;
        }

        public MFSMBuilder<TContext> AddTransition(string fromStateId, string toStateId,
            Func<TContext, bool> condition, int priority = 0)
        {
            _machine.AddTransition(fromStateId, toStateId, condition, priority);
            return this;
        }

        public MFSMBuilder<TContext> AddAnyTransition(string toStateId,
            IMFSMTransitionCondition<TContext> condition = null, int priority = 0)
        {
            _machine.AddTransition(MFSMConstants.AnyStateId, toStateId, condition, priority);
            return this;
        }

        public MFSMBuilder<TContext> AddAnyTransition(string toStateId, Func<TContext, bool> condition,
            int priority = 0)
        {
            _machine.AddTransition(MFSMConstants.AnyStateId, toStateId, condition, priority);
            return this;
        }

        public MFSMBuilder<TContext> SetRoot(string rootStateId)
        {
            _rootId = rootStateId;
            return this;
        }

        /// <summary>构建机器。若调过 SetRoot 会同步根状态。</summary>
        public MFSMMachine<TContext> Build()
        {
            if (!string.IsNullOrEmpty(_rootId))
                _machine.SetRoot(_rootId);
            return _machine;
        }
    }
}