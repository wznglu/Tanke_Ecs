using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// ComponentSystem ¡ú SystemBase  
public partial class DestructionSystem : SystemBase
{
    private float thresholdDistance = 2f;
    // 1
    private EndSimulationEntityCommandBufferSystem ecbSystem;
    // 2
    private EntityQuery bulletQuery;

    // 3
    protected override void OnCreate()
    {
        ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        bulletQuery = GetEntityQuery(ComponentType.ReadOnly<Bullet>(), ComponentType.ReadOnly<Translation>());
    }

    protected override void OnUpdate()
    {
        if (GameManager.IsGameOver())
            return;
        // 4
        var ecb = ecbSystem.CreateCommandBuffer();

        var playerPosition = (float3)GameManager.GetPlayerPosition();

        // 5
        var bulletEntities = bulletQuery.ToEntityArray(Allocator.Temp);
        var bulletPositions = bulletQuery.ToComponentDataArray<Translation>(Allocator.Temp);
        Entities
           // .WithDisposeOnCompletion(bulletEntities)  // 6
            .WithDisposeOnCompletion(bulletPositions)
            .WithAll<Enemy>()
            .ForEach((Entity enemy, ref Translation enemyPos) =>
            {
                playerPosition.y = enemyPos.Value.y;
                // 7
                if (math.distance(enemyPos.Value, playerPosition) <= thresholdDistance)
                {
                    //FXManager.Instance.CreateExplosion(enemyPos.Value);
                  //  FXManager.Instance.CreateExplosion(playerPosition);
                   // GameManager.EndGame();
                    // 8
                    ecb.DestroyEntity(enemy);
                }

                var enemyPosition = enemyPos.Value;
                // 9
                for (int i = 0; i < bulletPositions.Length; i++)
                {
                    if (math.distance(enemyPosition, bulletPositions[i].Value) <= thresholdDistance)
                    {
                        ecb.DestroyEntity(enemy);
                        ecb.DestroyEntity(bulletEntities[i]);

                     //   FXManager.Instance.CreateExplosion(enemyPosition);
                        GameManager.AddScore(1);
                    }
                }
            })
            .WithoutBurst()
            .Run();
        // 10
        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}