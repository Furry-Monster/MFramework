namespace MFSM.Runtime.Core
{
    /// <summary>状态接口：Id、Enter/Update/Exit，可选复合（InitialSubStateId）。</summary>
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