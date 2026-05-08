## Why

当前敌人只有移动与碰撞行为，战斗压力主要来自敌机接触玩家，玩法层次较单一。增加敌人存活期间间隔发射子弹，可以让 `Enemy_2` 与 `Enemy_Boss` 形成不同威胁，并复用已有 `EnemyBullet` 资源补齐敌方攻击链路。

## What Changes

- `Enemy_1` 保持现状，不发射子弹。
- `Enemy_2` 在存活期间按固定间隔持续发射一个向下移动的敌方子弹。
- `Enemy_Boss` 在存活期间按固定间隔持续发射一个敌方子弹，子弹初始方向朝向玩家当前位置，生成后沿该方向直线移动。
- 敌方子弹命中玩家或飞出边界后应回收到现有对象池链路。
- 游戏结束或敌人回收后，敌人停止发射；敌人从对象池重新生成时重置发射计时。

## Capabilities

### New Capabilities

- `enemy-bullet-fire`: 定义敌人按类型发射子弹、敌方子弹方向、命中玩家和对象池回收行为。

### Modified Capabilities

- 无。

## Impact

- 受影响 GameLogic 脚本：`GameManager.cs`、`Enemy.cs`、`Bullet.cs`，可能涉及 `Player.cs` 的命中判定协作。
- 受影响资源：已有 `Assets/GameResRaw/Actor/Role/EnemyBullet.prefab` 需要被加载并纳入对象池复用。
- 受影响框架能力：`ResourceContainer` 资源加载生命周期、`GameEntry.ObjectPool` 对象池回收、`GameEntry.Timer` 延迟 GameOver 链路。
- 不新增外部依赖，不修改 Luban 生成代码，不改变 LFramework Runtime 架构。
