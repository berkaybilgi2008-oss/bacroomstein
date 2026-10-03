# FPS Controller Prototype Setup

This prototype targets the project's current Unity Input System package.

## 1. Create the player
1. Open your existing level scene in Unity.
2. In the Hierarchy, create an empty GameObject named `Player`.
3. Set its position so it stands over the floor (the Player object's origin should be at foot level).
4. Add a `Character Controller` component. Suggested starting values: Height 2, Radius 0.35, Center (0, 1, 0).
5. Add the `FPSController` script from `Assets/Scripts/FPSController.cs`.

## 2. Add the camera
1. Drag the scene's Main Camera onto Player in the Hierarchy so it becomes a child.
2. Set the camera's local Position to (0, 1.6, 0) and local Rotation to (0, 0, 0).
3. Drag the camera into the Player's `Player Camera` field in the FPSController component. The script also tries to find a child Camera automatically.

## 3. Prevent duplicate cameras
If the scene has another active camera, disable or remove it so only the player camera renders the game.

## 4. Test
Press Play. Use WASD to move, mouse to look, Space to jump, and Escape to release the cursor. Click the Game view to recapture the cursor.

## Notes
- This script uses the project's installed `com.unity.inputsystem` package and the new Input System API.
- This change adds the controller script only; the Player and Camera must be placed in the scene as described above.
- Make sure the player starts above the floor and inside the level, not inside a wall.
