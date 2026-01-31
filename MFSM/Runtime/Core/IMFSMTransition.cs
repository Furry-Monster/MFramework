using System;

namespace MFSM.Runtime.Core
{
    /// <summary>转换条件。true 时触发转换。context 由 SetContext 提供，未 Start 可能为 default。</summary>
    public interface IMFSMTransitionCondition<in TContext> where TContext : IMFSMContext
    {
        bool Evaluate(TContext context);
    }

    /// <summary>委托形式的转换条件。</summary>
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