namespace MFSM.Runtime.Core
{
    /// <summary>状态基类，可覆写 OnEnter/OnUpdate/OnExit。</summary>
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

    /// <summary>复合状态，进入时展开到 InitialSubStateId 子状态。</summary>
    public abstract class MFSMCompoundState<TContext> : MFSMState<TContext> where TContext : IMFSMContext
    {
        public override bool IsCompound => true;
        public abstract override string InitialSubStateId { get; }
    }
}