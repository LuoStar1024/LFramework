## Context

当前飞机玩法位于 `Assets/GameScripts/GameLogic/Game`。`GameManager` 负责预加载角色资源、创建玩家、生成敌人、通过 `GoPoolObject` 对 `GameObject` 做对象池复用；`Player` 通过 `GameManager.GetBullet()` 获取 `PlayerBullet` 并调用 `Bullet.SetDirect(true, firePos)` 发射上行子弹；`Enemy` 目前只负责向下移动、越界回收和撞到玩家后回收。

项目已有 `Assets/GameResRaw/Actor/Role/EnemyBullet.prefab`，因此本变更优先补齐敌方子弹加载、发射和命中逻辑，不新增资源、不修改框架模块。实现必须遵守现有资源所有权：预制体由 `ResourceContainer` 加载和统一释放，运行时子弹实例纳入 `GameEntry.ObjectPool` 创建的对象池，回收时通过 `GameManager.HideGo()` 归还。

## Goals / Non-Goals

**Goals:**

- 让敌人在存活期间按固定间隔持续发射子弹。
- 按敌人类型区分发射行为：`Enemy_1` 不发射，`Enemy_2` 向下发射，`Enemy_Boss` 朝玩家当前位置发射。
- 复用现有 `EnemyBullet` 预制体和 `GoPoolObject` 对象池链路。
- 扩展子弹运动与命中目标判定，使玩家子弹和敌方子弹可以共用 `Bullet` 脚本。

**Non-Goals:**

- 不引入新的敌人配置表或 Luban 生成代码。
- 不新增弹幕模式、追踪弹、多发散射、伤害数值系统或玩家血量扣减。
- 不调整敌人生成概率、移动速度、分数规则、UI 或关卡流程。
- 不改变 LFramework Runtime、HybridCLR、YooAsset 或启动流程。

## Decisions

### 1. 敌人类型先基于对象池名称配置

`GameManager.CreateEnemy()` 已经按 `Enemy_1`、`Enemy_2`、`Enemy_Boss` 选择预制体并设置起始位置。实现时可以在获取 `Enemy` 组件后把敌人名称传入初始化方法，或由 `Enemy` 根据 `gameObject.name` 判断类型。

推荐采用显式传参：`SetStartPos(startPos, enemyName)` 或新增初始化方法。这样对象池复用时不依赖 Unity 实例名是否带 `(Clone)`，也避免 prefab 名称变化造成隐式行为漂移。

### 2. 子弹方向改为向量，阵营决定命中目标

当前 `Bullet.SetDirect(bool isUp, Vector3 startPos)` 只能表达上下方向，且碰撞只判断 `Enemy`。实现应保留玩家发射入口的兼容性或同步更新调用点，并新增基于 `Vector2 direction` 和目标 Tag 的初始化能力。

推荐模型：

```text
Bullet
  startPos
  direction
  targetTag
```

玩家子弹使用 `Vector2.up`，目标为 `Enemy`；敌方子弹使用 `Vector2.down` 或朝向玩家的归一化方向，目标为 `Player`。子弹回收时不销毁，继续使用 `GameManager.HideGo()` 归还对象池。

### 3. Boss 子弹只在生成时锁定方向

`Enemy_Boss` 发射时读取玩家当前 Transform 位置并计算方向。子弹生成后直线飞行，不在 `Update()` 中继续追踪玩家。这样实现简单、可预期，并符合“初始方向朝向玩家”的需求。

如果玩家不存在或已被销毁，Boss 本次发射跳过，避免生成零方向或异常子弹。

### 4. 发射计时绑定敌人存活生命周期

`Enemy.SetStartPos()` 负责重置 `_isRecycled`、位置和发射计时。`Update()` 中只有敌人未回收时才移动和累计发射计时；敌人越界、撞到玩家、游戏结束被回收后不再发射。

对象池重新 Spawn 同一个敌人实例时，必须重置发射计时，避免敌人刚出现时继承上一次回收前的计时状态。

### 5. 资源和对象池继续由 GameManager 统一管理

`GameManager.InitManager()` 预加载 `EnemyBullet`，并把预制体加入 `_prefabDict`。新增 `GetEnemyBullet()` 或泛化 `GetBullet(string bulletName)` 来复用对象池取出/注册流程。

不建议让 `Enemy` 自己加载资源或直接 Instantiate，因为这会分散资源所有权，并绕过当前 `ResourceContainer` 与对象池约定。

## Risks / Trade-offs

- [Prefab 物理配置不完整] `EnemyBullet.prefab` 当前需要确认 Tag、Layer、Collider/Rigidbody2D 是否能触发玩家命中。→ 实现后在 Unity Editor 中检查敌方子弹与玩家碰撞矩阵，必要时调整 prefab。
- [对象池名称和行为耦合] 敌人类型来自字符串名称，未来扩展敌人会继续增加分支。→ 当前功能范围小，先保持与现有 `GameManager` 生成逻辑一致；后续若敌人类型增多再引入配置表。
- [玩家被销毁后的瞄准] GameOver 延迟期间玩家可能不存在。→ Boss 发射前检查玩家引用有效，不存在时跳过本次发射。
- [子弹共用脚本回归] 修改 `Bullet` 可能影响玩家子弹命中敌人。→ 实现时保留玩家子弹上行与命中敌人的行为，并在验证中覆盖玩家子弹、敌方子弹两条路径。

## Migration Plan

1. 扩展 `GameManager` 预加载和获取 `EnemyBullet` 的能力。
2. 扩展 `Bullet` 初始化、移动方向和目标 Tag 判定。
3. 扩展 `Enemy` 按类型间隔发射敌方子弹。
4. 在 Unity Editor 中确认 `EnemyBullet.prefab` 的碰撞配置，并运行游戏流程验证。

如需回滚，移除敌人发射逻辑和 `EnemyBullet` 预加载即可恢复当前玩法。

## Open Questions

- 敌人发射间隔和首次发射延迟是否需要按类型分别配置，还是先使用统一 serialized 字段。
- 敌方子弹命中玩家后是否立即 GameOver，还是后续接入 `hp` 扣减逻辑；本变更按当前玩家撞敌即 GameOver 的玩法处理。
