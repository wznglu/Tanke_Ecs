using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
/// <summary>
/// System for destroying Enemy Entities and the Player
/// </summary>
public class DestructionSystem : ComponentSystem
{
    // 1 minimum distance for destruction
    float thresholdDistance = 2f;

    protected override void OnUpdate()
    {
        // 2 check if the game is over; if so, return and skip any logic
        if (GameManager.IsGameOver())
        {
            return;
        }

        // 3 use the static method to store the player's position
        float3 playerPosition = (float3)GameManager.GetPlayerPosition();

        // 4 loop through all Entities with the EnemyTag, passing in the enemy and Translation component
        // note: the ref keyword is not used for the Entity input parameter
        Entities.WithAll<EnemyTag>().ForEach((Entity enemy, ref Translation enemyPos) =>
        {

            // 5 discard the player's y value; only check the xz-plane
            playerPosition.y = enemyPos.Value.y;

            // 6 check if the enemy is close enough to the player
            if (math.distance(enemyPos.Value, playerPosition) <= thresholdDistance)
            {
                // create an explosion at the enemy
                FXManager.Instance.CreateExplosion(enemyPos.Value);

                // create an explosion at the player (comment this out for stress test)
                FXManager.Instance.CreateExplosion(playerPosition);

                // end the game (comment this out for stress test)
                GameManager.EndGame();

                // 7 safely remove the enemy Enity using an EntityCommandBuffer
                PostUpdateCommands.DestroyEntity(enemy);
            }

            // check if the enemy is struck by bullet

            // 8 save the enemy position in a separate temp variable; pass this into a second Entities.ForEach loop
            float3 enemyPosition = enemyPos.Value;

            // 9 check all Entities with the BulletTag, passing in the bullet Entity and Translation component
            Entities.WithAll<BulletTag>().ForEach((Entity bullet, ref Translation bulletPos) =>
            {
                // 10 if the bullet and enemy are close enough...
                if (math.distance(enemyPosition, bulletPos.Value) <= thresholdDistance)
                {
                    // destroy the enemy when it's safe to do so
                    PostUpdateCommands.DestroyEntity(enemy);

                    // destroy the bullet when it's safe to do so
                    PostUpdateCommands.DestroyEntity(bullet);

                    //11 play back a particle effect
                    FXManager.Instance.CreateExplosion(enemyPosition);

                    // increment the score
                    GameManager.AddScore(1);
                }
            });
        });
    }
}
