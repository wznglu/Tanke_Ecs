using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// 1 ComponentSystem → SystemBase  
public partial class MovementSystem : SystemBase
{
    // 2
    protected override void OnUpdate()
    {
        var dt = Time.DeltaTime;
        // 3
        Entities.WithAll<MoveForward>().ForEach((ref Translation trans, in Rotation rot, in MoveForward moveForward) =>
        {
            // 4
            trans.Value += moveForward.speed * math.forward(rot.Value) * dt;
        }).ScheduleParallel();  // 5 
    }
}