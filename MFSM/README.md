# MFSM

层次状态机（Hierarchical FSM）。

- **核心**：`MFSMMachine<TContext>`，`RegisterState` / `AddTransition` / `Start` / `Update`；或 `MFSMBuilder<TContext>` 链式构建。
- **MonoBehaviour 驱动**：继承 `MFSMRunner<TContext>`，实现 `SetupMachine`、`GetContext`。
- **任意状态转换**：`AddAnyTransition(toStateId, condition, priority)`。
- **编辑器**：菜单 **MFramework → MFSM Viewer**；`MFSMRunnerDebug` 游戏内显示状态与路径。

目录：`Runtime/Core`（上下文、状态、转换、机器、配置），`Runtime/Runner`（驱动与调试），`Editor`。  
命名空间：`MFSM.Runtime`、`MFSM.Editor`。
