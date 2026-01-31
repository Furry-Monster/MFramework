# MAudio

音频加载与播放，基于 AudioSource 池减少创建/销毁。

- **入口**：场景中挂 `AudioHandler`（单例），通过 `AudioHandler.Instance` 播放。
- **Clip 来源**：默认从 `Resources/Audio` 加载，可配置路径或实现 `IAudioClipProvider`。
- **编辑器**：菜单 **MFramework → MAudio Viewer**，Play 模式下查看已加载 Clip 与池状态。

命名空间：`MAudio.Runtime`、`MAudio.Editor`。
