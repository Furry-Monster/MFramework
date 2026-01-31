using System.Collections.Generic;
using MFSM.Runtime.Core;
using UnityEngine;

namespace MFSM.Runtime.Runner
{
    /// <summary>MonoBehaviour 驱动分层状态机。子类实现 SetupLayeredMachine、GetContext、GetInitialStatePerLayer。</summary>
    public abstract class MFSMLayeredRunner<TContext> : MonoBehaviour, IMFSMRunner
        where TContext : class, IMFSMLayeredContext
    {
        [SerializeField] private bool runInUpdate = true;
        [SerializeField] private bool runInFixedUpdate;

        protected MFSMLayeredMachine<TContext> Machine { get; private set; }
        protected TContext Context { get; private set; }

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

        #region IMFSMRunner

        public string CurrentStateId => Machine?.GetCurrentLayersString() ?? string.Empty;
        public string GetCurrentPathString() => Machine?.GetCurrentLayersString() ?? string.Empty;

        #endregion

        #region 抽象接口

        protected abstract void SetupLayeredMachine();
        protected abstract TContext GetContext();
        protected abstract IReadOnlyDictionary<string, string> GetInitialStatePerLayer();

        #endregion

        #region Unity 生命周期

        protected virtual void Awake()
        {
            Machine = new MFSMLayeredMachine<TContext>();
            SetupLayeredMachine();
        }

        protected virtual void Start()
        {
            if (Machine == null)
                return;
            Context = GetContext();
            if (Context == null)
                return;
            var initial = GetInitialStatePerLayer();
            if (initial == null || initial.Count == 0)
                return;
            Machine.SetContext(Context);
            Machine.Start(Context, initial);
        }

        protected virtual void Update()
        {
            if (!runInUpdate || Machine == null)
                return;
            Context = GetContext();
            if (Context == null)
                return;
            Machine.SetContext(Context);
            Machine.Update(Time.deltaTime);
        }

        protected virtual void FixedUpdate()
        {
            if (!runInFixedUpdate || Machine == null)
                return;
            Context = GetContext();
            if (Context == null)
                return;
            Machine.SetContext(Context);
            Machine.Update(Time.fixedDeltaTime);
        }

        #endregion

        #region 公共 API

        public void TransitionLayerTo(string layerId, string stateId) => Machine?.TransitionLayerTo(layerId, stateId);

        public string GetLayerCurrentStateId(string layerId) =>
            Machine?.GetLayerCurrentStateId(layerId) ?? string.Empty;

        public void PauseMachine()
        {
            if (Machine != null) Machine.IsPaused = true;
        }

        public void ResumeMachine()
        {
            if (Machine != null) Machine.IsPaused = false;
        }

        #endregion
    }
}