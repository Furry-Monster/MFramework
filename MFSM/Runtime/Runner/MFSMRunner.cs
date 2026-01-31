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

        #region IMFSMRunner

        public string CurrentStateId => Machine?.CurrentLeaf?.Id ?? string.Empty;
        public string GetCurrentPathString() => Machine?.GetCurrentPathString() ?? string.Empty;

        #endregion

        #region 抽象与虚方法

        protected abstract void SetupMachine();
        protected abstract TContext GetContext();

        /// <summary>非空则从该状态启动，null 则从根启动。</summary>
        protected virtual string GetInitialStateId() => null;

        #endregion

        #region 配置

        protected bool RunInUpdate
        {
            get => runInUpdate;
            set => runInUpdate = value;
        }

        protected bool RunInFixedUpdate
        {
            get => runInFixedUpdate;
            set => runInFixedUpdate = value;
        }

        #endregion

        #region Unity 生命周期

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
            if (Context == null)
                return;
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

        #endregion

        #region 公共 API

        public void TransitionTo(string stateId) => Machine?.TransitionTo(stateId);

        public bool IsInStateOrDescendant(string stateId) =>
            Machine != null && Machine.IsInStateOrDescendant(stateId);

        public void PauseMachine()
        {
            if (Machine != null) Machine.Pause();
        }

        public void ResumeMachine()
        {
            if (Machine != null) Machine.Resume();
        }

        #endregion
    }
}