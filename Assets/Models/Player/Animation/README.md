# PlayerWithMotion

This folder contains the player model, rig, and animation files for Unity.

## Contents

- Player model
- Rig / skeleton
- Animation clips:
  - walking
  - jumping

## How to use

1. Import the FBX file into Unity.
2. Select the FBX in the Project window.
3. Open the **Animation** tab in the Inspector.
4. Confirm the clips are available:
   - walking
   - jumping
5. Enable **Loop Time** for `walking` if needed.
6. Create an **Animator Controller**.
7. Drag the animation clips into the Animator.
8. Set `walking` as the default state.
9. Add transitions if you want to switch to `jumping`.

## Notes

- `walking` is intended to loop.
- `jumping` is intended to play once.
- If the model appears scaled incorrectly in Unity, check the FBX import settings.
- If parts of the mesh are missing, check normals and materials in Blender and Unity.
