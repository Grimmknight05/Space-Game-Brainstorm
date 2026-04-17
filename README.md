# Space Game Brainstorm


---
## PlayerController
**By Joshua Henrikson**

## PlayerController.cs

Handles player movement, jumping, camera look, scoring, UI, and interactions using Unity's Input System and Rigidbody physics.

### Features
- **Movement**
  - Three modes:
    - Force-based
    - Velocity-based
    - Acceleration-based (smooth)
  - Movement is relative to camera direction

- **Mouse Look**
  - Horizontal rotates player
  - Vertical rotates camera (clamped)

- **Jump System**
  - Ground jump + air jumps
  - Raycast ground detection
  - Separate forces and SFX

- **Score System**
  - Tracks points
  - Updates TextMeshPro UI
  - Fires `OnScoreChanged` event

- **Collision Handling**
  - `PickUp` → increases score
  - `Enemy` → triggers death

- **UI**
  - Score display
  - Win screen
  - Death screen

- **Audio**
  - Random jump / air jump sounds

### Required Components
- `Rigidbody`
- `PlayerInput`
- `TextMeshProUGUI`
- `AudioSource`

### Input Actions
- **Move** → `Vector2`
- **Look** → `Vector2`
- **Jump** → Button

### Events
```csharp
OnScoreChanged(int newScore)
OnPlayerDeath()