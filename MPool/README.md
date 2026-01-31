# MPool

对象池：**RefPool**（C# 引用类型）与 **GOPool**（GameObject）。

- **RefPool**：类型实现 `IRefPoolable`，通过 `RefPoolMgr.Acquire<T>()` / `RefPoolMgr.Release(obj)` 取还。
- **GOPool**：预制体可选实现 `IGameObjectPoolable`，通过 `GameObjectPoolManager.Instance.Get/Return` 取还；可建 GameObject Pool Config 配置容量等。
- **编辑器**：菜单 **MPool → Open Pool Viewer**，Play 模式下查看池统计。

命名空间：`MPool.Runtime.RefPool`、`MPool.Runtime.GOPool`、`MPool.Editor`。  
更多用法见 `Documentation~/README.md`。
