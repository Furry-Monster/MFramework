namespace MFSM.Runtime.Core
{
    /// <summary>状态接口。Enter/Update/Exit，可选复合状态。</summary>
    public interface IMFSMState<in TContext> where TContext : IMFSMContext
    {
        string Id { get; }
        void OnEnter(TContext context);
        void OnUpdate(TContext context, float deltaTime);
        void OnExit(TContext context);
        bool IsCompound { get; }
        string InitialSubStateId { get; }
    }
}