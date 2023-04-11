/*
 * Copyright (c) 2020 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * Notwithstanding the foregoing, you may not use, copy, modify, merge, publish, 
 * distribute, sublicense, create a derivative work, and/or sell copies of the 
 * Software in any work that is designed, intended, or marketed for pedagogical or 
 * instructional purposes related to programming, coding, application development, 
 * or information technology.  Permission for such use, copying, modification,
 * merger, publication, distribution, sublicensing, creation of derivative works, 
 * or sale is expressly withheld.
 *    
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;


using Random = UnityEngine.Random;


/// <summary>
/// spawns a swarm of enemy entities offscreen, encircling the player
/// </summary>
public class EnemySpawner : MonoBehaviour
{

    [Header("Spawner")]
    // number of enemies generated per interval
    [SerializeField] private int spawnCount = 30;

    // time between spawns
    [SerializeField] private float spawnInterval = 3f;

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

    private EntityManager entityManager;

    [SerializeField] private Mesh enemyMesh;
    [SerializeField] private Material enemyMaterial;


    // flag from GameManager to enable spawning
    private bool canSpawn;

    // GameObject prefab to convert
    [SerializeField] private GameObject enemyPrefab;

    // Entity prefab
    private Entity enemyEntityPrefab;


    private void Start()
    {
        // each World has one EntityManager; store a reference to it
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // settings used to convert GameObject prefab
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);

        // convert the GameObject prefab into an Entity prefab and store it
        enemyEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(enemyPrefab, settings);

        // spawn multiple enemies
        SpawnWave();
    }

    // spawns enemies in a ring around the player
    private void SpawnWave()
    {
        // 1 create a memory-efficient NativeArray for processing multiple Entities
        NativeArray<Entity> enemyArray = new NativeArray<Entity>(spawnCount, Allocator.Temp);

        // 2 loop through NativeArray
        for (int i = 0; i < enemyArray.Length; i++)
        {
            // instantiate an Entity prefab
            enemyArray[i] = entityManager.Instantiate(enemyEntityPrefab);

            // 3 move the instance to a random point on a circle around the player
            entityManager.SetComponentData(enemyArray[i], new Translation { Value = RandomPointOnCircle(spawnRadius) });

            // 4 set a random forward speed
            entityManager.SetComponentData(enemyArray[i], new MoveForward { speed = Random.Range(minSpeed, maxSpeed) });
        }

        // 5 remove the temporary NativeArray
        enemyArray.Dispose();

        // 6 increment the spawnCount to make each wave progressively harder
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
