# MFSM

层次状态机（Hierarchical FSM），并支持多层级并行（Layered FSM）。

- **核心**：`MFSMMachine<TContext>`，`RegisterState` / `AddTransition` / `Start` / `Update`；或 `MFSMBuilder<TContext>` 链式构建。
- **MonoBehaviour 驱动**：继承 `MFSMRunner<TContext>`，实现 `SetupMachine`、`GetContext`。
- **任意状态转换**：`AddAnyTransition(toStateId, condition, priority)`。
- **分层状态机**：多台机器按固定顺序每帧更新，每层运行后将当前状态 Id 写回上下文，供下一层条件读取（如 Posture / Locomotion / Armed / Focus）。
  - **上下文**：`IMFSMLayeredContext`（`SetLayerOutput` / `GetLayerOutput`）；可选基类 `MFSMLayeredContextBase`。
  - **机器**：`MFSMLayeredMachine<TContext>`，`AddLayer(layerId, machine)` / `Start(context, initialStates)` / `Update(deltaTime)`。
  - **驱动**：继承 `MFSMLayeredRunner<TContext>`，实现 `SetupLayeredMachine`、`GetContext`、`GetInitialStatePerLayer`。
- **编辑器**：菜单 **MFramework → MFSM Viewer**；`MFSMRunnerDebug` 游戏内显示状态与路径。

目录：`Runtime/Core`（上下文、状态、转换、机器、分层机器、配置），`Runtime/Runner`（驱动与调试），`Editor`。  
命名空间：`MFSM.Runtime`、`MFSM.Editor`。
