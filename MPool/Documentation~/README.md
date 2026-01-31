# MPool — 对象池插件

MPool 提供两套对象池：**RefPool**（引用类型池）与 **GOPool**（GameObject 池），可单独或组合使用，适合作为项目内插件或后续打包为 UPM 包。

---

## 1. RefPool（引用类型池）

用于 C# 引用类型（class）的池化，减少 GC。

### 使用步骤

1. 类型实现 `IRefPoolable`：

```csharp
using MPool.Runtime.RefPool;

public class MyItem : IRefPoolable
{
    public void OnAcquireFromPool() { /* 从池取出时 */ }
    public void OnReturnToPool()   { /* 归还到池时 */ }
}
```

2. 通过 `RefPoolMgr` 获取/归还：

```csharp
var obj = RefPoolMgr.Acquire<MyItem>();
// 使用 obj ...
RefPoolMgr.Release(obj);
```

### 常用 API

| 方法 | 说明 |
|------|------|
| `RefPoolMgr.Acquire<T>()` | 从池中取一个 T，无则新建 |
| `RefPoolMgr.Release(IRefPoolable)` | 归还到池 |
| `RefPoolMgr.Expand<T>(count)` | 预创建 count 个到池 |
| `RefPoolMgr.Shrink<T>(count)` | 从池中移除 count 个空闲对象 |
| `RefPoolMgr.Clear()` | 清空所有 RefPool |
| `RefPoolMgr.GetAllPoolInfos()` | 获取所有池的统计信息 |

---

## 2. GOPool（GameObject 池）

用于预制体的池化，避免频繁 Instantiate/Destroy。

### 使用步骤

1. 预制体上的脚本（可选）实现 `IGameObjectPoolable`：

```csharp
using MPool.Runtime.GOPool;

public class Bullet : MonoBehaviour, IGameObjectPoolable
{
    public void OnSpawnFromPool() { /* 从池取出时 */ }
    public void OnReturnToPool() { /* 归还到池时 */ }
}
```

2. 通过场景中的 `GameObjectPoolManager.Instance` 获取/归还：

```csharp
var bullet = GameObjectPoolManager.Instance.Get(bulletPrefab, position, rotation);
// 使用 bullet ...
GameObjectPoolManager.Instance.Return(bulletPrefab, bullet);
```

3. 可选：在 **Create > MPool > GameObject Pool Config** 创建配置资源，设置容量、父节点、DontDestroyOnLoad 等，再通过 `SetPoolConfig(prefab, config)` 应用。

### 依赖

- `GameObjectPoolManager` 继承自项目中的 `PersistentSingleton<T>`，需保证场景或代码中会创建该单例（例如挂到常驻场景）。

---

## 3. 编辑器

- **菜单：MPool > Open Pool Viewer**  
  打开池查看器窗口。在 **运行态** 下可查看 RefPool / GOPool 的实时统计（数量、获取/归还次数等）。

---
