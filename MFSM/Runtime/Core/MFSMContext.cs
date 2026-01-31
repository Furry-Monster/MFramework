namespace MFSM.Runtime.Core
{
    /// <summary>状态机共享上下文。推荐 struct 以减少分配。</summary>
    public interface IMFSMContext
    {
    }

    /// <summary>无上下文占位。</summary>
    public readonly struct MFSMEmptyContext : IMFSMContext
    {
    }
}