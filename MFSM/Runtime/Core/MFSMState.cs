namespace MFSM.Runtime.Core
{
    /// <summary>状态基类。可覆写 OnEnter/OnUpdate/OnExit，可选复合状态。</summary>
    public abstract class MFSMState<TContext> : IMFSMState<TContext> where TContext : IMFSMContext
    {
        public abstract string Id { get; }

        public virtual bool IsCompound => false;
        public virtual string InitialSubStateId => null;

        public virtual void OnEnter(TContext context)
        {
        }

        public virtual void OnUpdate(TContext context, float deltaTime)
        {
        }

        public virtual void OnExit(TContext context)
        {
        }
    }

    /// <summary>复合状态。子状态由机器注册，InitialSubStateId 为进入时的默认子状态。</summary>
    public abstract class MFSMCompoundState<TContext> : MFSMState<TContext> where TContext : IMFSMContext
    {
        public override bool IsCompound => true;
        public abstract override string InitialSubStateId { get; }
    }
}