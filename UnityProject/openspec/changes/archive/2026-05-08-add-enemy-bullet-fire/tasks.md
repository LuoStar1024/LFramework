## 1. Resource and Pool Wiring

- [x] 1.1 Load `EnemyBullet` in `GameManager.InitManager()` through the existing `ResourceContainer`.
- [x] 1.2 Add a pooled retrieval path for `EnemyBullet`, either by introducing `GetEnemyBullet()` or by generalizing the current bullet retrieval helper.
- [x] 1.3 Ensure failed enemy bullet prefab lookup logs through the GameLogic logging path and skips bullet creation without throwing.

## 2. Bullet Behavior

- [x] 2.1 Extend `Bullet` initialization to accept start position, normalized movement direction, and target tag.
- [x] 2.2 Preserve existing player bullet behavior: upward movement, enemy target, pooled recycle on enemy hit or out-of-bounds.
- [x] 2.3 Add enemy bullet behavior: move along initial direction, target player, pooled recycle on player hit or out-of-bounds.
- [x] 2.4 Ensure pooled bullets reset all retained state when reinitialized.
- [x] 2.5 Configure enemy bullets so player bullets can cancel them on contact.

## 3. Enemy Firing

- [x] 3.1 Extend enemy initialization so each spawned enemy knows its enemy type.
- [x] 3.2 Configure `Enemy_1` to never fire.
- [x] 3.3 Configure `Enemy_2` to fire one downward enemy bullet per firing interval while alive.
- [x] 3.4 Configure `Enemy_Boss` to fire one enemy bullet per firing interval toward the player's current position at fire time.
- [x] 3.5 Stop firing while an enemy is recycled and reset firing timer when reused from the object pool.

## 4. Player Hit Flow

- [x] 4.1 Route enemy bullet hits on the player into the existing player defeat/GameOver flow.
- [x] 4.2 Avoid applying player defeat behavior when an enemy bullet hits non-player objects.
- [x] 4.3 Avoid Boss firing when the player object no longer exists.

## 5. Verification

- [x] 5.1 Search affected call sites to verify `GameManager`, `Enemy`, `Bullet`, and `Player` APIs remain consistent.
- [x] 5.2 Check `EnemyBullet.prefab` in Unity Editor for Tag, Layer, Collider2D, Rigidbody2D, and collision matrix compatibility with the player.
- [x] 5.3 Run the game scene in Unity Editor and verify `Enemy_1` does not fire, `Enemy_2` fires downward repeatedly, and `Enemy_Boss` repeatedly fires toward the player's current position.
- [x] 5.4 Verify player bullets still hit enemies, enemy bullets hit the player, and player/enemy bullets cancel each other without breaking object pool reuse.
