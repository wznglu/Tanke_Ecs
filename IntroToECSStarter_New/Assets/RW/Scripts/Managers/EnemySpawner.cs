

using UnityEngine;
using Unity.Mathematics;

using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Random = UnityEngine.Random;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;

/// <summary>
/// spawns a swarm of enemy entities offscreen, encircling the player
/// </summary>
public class EnemySpawner : MonoBehaviour
{

    [Header("Spawner")]
    // number of enemies generated per interval
    [SerializeField] private int spawnCount = 10;

    // time between spawns
    [SerializeField] private float spawnInterval = 1f;

    // enemies spawned on a circle of this radius
    [SerializeField] private float spawnRadius = 30f;

    // extra enemy increase each wave
    [SerializeField] private int difficultyBonus = 5;

    [Header("Enemy")]
    // random speed range
    [SerializeField] float minSpeed = 4f;
    [SerializeField] float maxSpeed = 12f;

    // counter
    private float spawnTimer;

    // flag from GameManager to enable spawning
    private bool canSpawn;


    public Entity mEntity;

    public GameObject mEntityObj;

    private EntityManager entityManager;

    private void Start()
    {
        BlobAssetStore aa = new BlobAssetStore();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        mEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(mEntityObj, GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, aa));
    }

    // spawns enemies in a ring around the player
    private void SpawnWave()
    {
        // 1
        var enemyArray = new NativeArray<Entity>(spawnCount, Allocator.Temp);
        // 2
        for (int i = 0; i < enemyArray.Length; i++)
        {
            enemyArray[i] = entityManager.Instantiate(mEntity);
            // 3
            entityManager.SetComponentData(enemyArray[i], new Translation { Value = RandomPointOnCircle(spawnRadius) });
            // 4
            entityManager.SetComponentData(enemyArray[i], new MoveForward { speed = Random.Range(minSpeed, maxSpeed) });
        }
        // 5  
        enemyArray.Dispose();
        // 6
        spawnCount += difficultyBonus;
    }

    // get a random point on a circle with given radius
    private float3 RandomPointOnCircle(float radius)
    {
        Vector2 randomPoint = Random.insideUnitCircle.normalized * radius;

        // return random point on circle, centered around the player position
        return new float3(randomPoint.x, 0.5f, randomPoint.y) + (float3)GameManager.GetPlayerPosition();
    }

    // signal from GameManager to begin spawning
    public void StartSpawn()
    {
        canSpawn = true;
    }

    private void Update()
    {
        // disable if the game has just started or if player is dead
        if (!canSpawn || GameManager.IsGameOver())
        {
            return;
        }

        // count up until next spawn
        spawnTimer += Time.deltaTime;

        // spawn and reset timer
        if (spawnTimer > spawnInterval)
        {
            SpawnWave();
            spawnTimer = 0;
        }
    }
}
