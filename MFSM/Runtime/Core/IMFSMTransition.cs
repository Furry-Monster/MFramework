using System;

namespace MFSM.Runtime.Core
{
    /// <summary>转换条件：Evaluate 为 true 时触发。未 Start 时 context 可能为 default。</summary>
    public interface IMFSMTransitionCondition<in TContext> where TContext : IMFSMContext
    {
        bool Evaluate(TContext context);
    }

    public sealed class MFSMTransitionCondition<TContext> : IMFSMTransitionCondition<TContext>
        where TContext : IMFSMContext
    {
        private readonly Func<TContext, bool> _evaluate;

        public MFSMTransitionCondition(Func<TContext, bool> evaluate)
        {
            _evaluate = evaluate ?? throw new ArgumentNullException(nameof(evaluate));
        }

        public bool Evaluate(TContext context)
        {
            return _evaluate(context);
        }
    }
}