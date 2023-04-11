using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// System for turning an Entity toward the player position.
/// </summary>

// System inherits from ComponentSystem (single-threaded)
public class FacePlayerSystem : ComponentSystem
{
    // 1 event-style callback runs every frame
    protected override void OnUpdate()
    {
        // 2 return if player is already dead
        if (GameManager.IsGameOver())
        {
            return;
        }

        // 4 get the player's position
        float3 playerPos = (float3)GameManager.GetPlayerPosition();

        // 3 loop through all Entities with EnemyTag, passing in the Entity, Translation and Rotation as input
        Entities.WithAll<EnemyTag>().ForEach((Entity entity, ref Translation trans, ref Rotation rot) =>
        {
            // 5 calculate vector to player, ignoring the y value
            float3 direction = playerPos - trans.Value;
            direction.y = 0f;

            // 6 calculate the rotation on the xz-plane
            rot.Value = quaternion.LookRotation(direction, math.up());
        });
    }
}
