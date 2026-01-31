namespace MFSM.Runtime.Runner
{
    /// <summary>Runner 非泛型接口，供调试等不依赖 TContext 的场景使用。</summary>
    public interface IMFSMRunner
    {
        string CurrentStateId { get; }
        string GetCurrentPathString();
    }
}