using MFSM.Runtime.Core;
using UnityEngine;

namespace MFSM.Runtime.Runner
{
    /// <summary>MonoBehaviour 驱动 MFSM。子类实现 SetupMachine、GetContext。</summary>
    public abstract class MFSMRunner<TContext> : MonoBehaviour, IMFSMRunner where TContext : IMFSMContext
    {
        [SerializeField] private bool runInUpdate = true;
        [SerializeField] private bool runInFixedUpdate;

        protected MFSMMachine<TContext> Machine { get; private set; }
        protected TContext Context { get; private set; }

        /// <summary>是否在 Update 中驱动。</summary>
        protected bool RunInUpdate
        {
            get => runInUpdate;
            set => runInUpdate = value;
        }

        /// <summary>是否在 FixedUpdate 中驱动。</summary>
        protected bool RunInFixedUpdate
        {
            get => runInFixedUpdate;
            set => runInFixedUpdate = value;
        }

        /// <summary>子类在此注册状态与转换。</summary>
        protected abstract void SetupMachine();

        /// <summary>子类在此提供每帧上下文。</summary>
        protected abstract TContext GetContext();

        /// <summary>非空则从该状态启动，null 则从根启动。</summary>
        protected virtual string GetInitialStateId()
        {
            return null;
        }

        protected virtual void Awake()
        {
            Machine = new MFSMMachine<TContext>();
            SetupMachine();
        }

        protected virtual void Start()
        {
            if (Machine == null)
                return;
            Context = GetContext();
            Machine.SetContext(Context);
            var initial = GetInitialStateId();
            if (!string.IsNullOrEmpty(initial))
                Machine.Start(Context, initial);
            else
                Machine.StartWithRoot(Context);
        }

        protected virtual void Update()
        {
            if (!runInUpdate || Machine == null)
                return;
            Context = GetContext();
            Machine.SetContext(Context);
            Machine.Update(Time.deltaTime);
        }

        protected virtual void FixedUpdate()
        {
            if (!runInFixedUpdate || Machine == null)
                return;
            Context = GetContext();
            Machine.SetContext(Context);
            Machine.Update(Time.fixedDeltaTime);
        }

        /// <summary>强制切换到某状态（不检查条件）。</summary>
        public void TransitionTo(string stateId)
        {
            Machine?.TransitionTo(stateId);
        }

        /// <summary>当前叶子状态 Id。</summary>
        public string CurrentStateId => Machine?.CurrentLeaf?.Id;

        /// <summary>当前路径 Id 序列。</summary>
        public string GetCurrentPathString()
        {
            return Machine?.GetCurrentPathString() ?? string.Empty;
        }

        /// <summary>当前是否处于该状态或其子状态。</summary>
        public bool IsInStateOrDescendant(string stateId)
        {
            return Machine != null && Machine.IsInStateOrDescendant(stateId);
        }
    }
}