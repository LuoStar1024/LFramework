# enemy-bullet-fire Specification

## Purpose
TBD - created by archiving change add-enemy-bullet-fire. Update Purpose after archive.
## Requirements
### Requirement: Enemy type controls bullet firing
The system SHALL determine enemy bullet firing behavior by enemy type during enemy initialization.

#### Scenario: Enemy_1 does not fire
- **WHEN** an `Enemy_1` instance is alive during gameplay
- **THEN** the enemy MUST NOT create enemy bullets

#### Scenario: Enemy_2 fires downward bullets
- **WHEN** an `Enemy_2` instance is alive for at least one configured firing interval
- **THEN** the enemy SHALL create one enemy bullet whose movement direction is downward

#### Scenario: Enemy_Boss fires toward the player
- **WHEN** an `Enemy_Boss` instance is alive for at least one configured firing interval and the player exists
- **THEN** the enemy SHALL create one enemy bullet whose initial movement direction points from the enemy position to the player's current position

### Requirement: Enemy firing repeats while alive
The system SHALL make firing enemies repeatedly fire at the configured interval while they remain alive and unrecycled.

#### Scenario: Firing enemy remains alive
- **WHEN** a firing enemy remains alive across multiple firing intervals
- **THEN** the enemy SHALL create one enemy bullet per elapsed firing interval

#### Scenario: Enemy is recycled
- **WHEN** an enemy is recycled because it moved out of bounds, collided with the player, or the game ended
- **THEN** the enemy MUST stop accumulating firing time and MUST NOT fire additional bullets until it is initialized again

#### Scenario: Enemy is reused from object pool
- **WHEN** an enemy instance is spawned again from the object pool
- **THEN** its firing timer SHALL be reset for the new lifetime

### Requirement: Enemy bullets use pooled EnemyBullet instances
The system SHALL create enemy bullets by loading the existing `EnemyBullet` prefab and reusing instances through the existing game object pool.

#### Scenario: Enemy bullet prefab is available
- **WHEN** an enemy fires and an inactive `EnemyBullet` instance exists in the object pool
- **THEN** the system SHALL reuse the pooled instance

#### Scenario: Enemy bullet prefab has no pooled instance
- **WHEN** an enemy fires and no inactive `EnemyBullet` instance exists in the object pool
- **THEN** the system SHALL instantiate the loaded `EnemyBullet` prefab, register it in the object pool, and spawn it

#### Scenario: Enemy bullet prefab is unavailable
- **WHEN** an enemy attempts to fire but the `EnemyBullet` prefab was not loaded
- **THEN** the system MUST skip bullet creation and report an error through the GameLogic logging path

### Requirement: Bullet movement supports explicit direction and target
The system SHALL allow a bullet to be initialized with a start position, movement direction, and target tag.

#### Scenario: Player bullet keeps existing behavior
- **WHEN** the player fires a bullet
- **THEN** the bullet SHALL move upward and target enemies

#### Scenario: Enemy bullet moves along initial direction
- **WHEN** an enemy bullet is created with a non-zero movement direction
- **THEN** the bullet SHALL move along the normalized initial direction until it hits its target or leaves the active bounds

#### Scenario: Boss bullet does not track after firing
- **WHEN** a boss bullet has been created toward the player's current position
- **THEN** subsequent player movement MUST NOT change that bullet's movement direction

### Requirement: Enemy bullets can end the player run
The system SHALL treat enemy bullets as hostile to the player and recycle them after a player hit.

#### Scenario: Enemy bullet hits player
- **WHEN** an enemy bullet collides or triggers with the player
- **THEN** the bullet SHALL be recycled and the player defeat flow SHALL begin consistently with the existing player collision GameOver behavior

#### Scenario: Enemy bullet hits non-target
- **WHEN** an enemy bullet collides or triggers with an object that is not the player
- **THEN** the bullet MUST NOT apply player defeat behavior because of that collision

#### Scenario: Enemy bullet leaves active bounds
- **WHEN** an enemy bullet moves outside the active bullet bounds
- **THEN** the bullet SHALL be recycled through the existing object pool path

### Requirement: Player bullets can cancel enemy bullets
The system SHALL allow player bullets and enemy bullets to cancel each other on contact.

#### Scenario: Player bullet hits enemy bullet
- **WHEN** a player bullet collides or triggers with an enemy bullet
- **THEN** both bullets SHALL be recycled through the existing object pool path

