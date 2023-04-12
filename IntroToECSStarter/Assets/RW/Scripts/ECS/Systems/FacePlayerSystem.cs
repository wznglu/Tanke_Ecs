// ✨ComponentSystem → SystemBase  
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class FacePlayerSystem : SystemBase
{
    // 1
    protected override void OnUpdate()
    {
        // 2
        if (GameManager.IsGameOver())
            return;
        // 3
        var playerPos = (float3)GameManager.GetPlayerPosition();
        // 4
        Entities.WithAll<Enemy>().ForEach((Entity entity, ref Translation trans, ref Rotation rot) =>
        {
            // 5
            var direction = playerPos - trans.Value;
            direction.y = 0f;
            // 6
            rot.Value = quaternion.LookRotation(direction, math.up());
        }).ScheduleParallel();  // 
    }
}