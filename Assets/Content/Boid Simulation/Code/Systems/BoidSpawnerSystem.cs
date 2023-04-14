using Content.Assignments.Seperation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct BoidSpawnerSystem : ISystem {
    // Called once when the system is created.
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BoidSpawnerComponentData>();
        
        state.GetComponentTypeHandle<BoidSpawnerComponentData>();
        state.GetEntityTypeHandle();
        
        Debug.Log("Initializing BoidSpawnerSystem");
    }

    // Called once when the system is destroyed.
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    // Usually called every frame. When exactly a system is updated
    // is determined by the system group to which it belongs.
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityManager em = state.EntityManager;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        
        foreach (var spawnerData in
                 SystemAPI.Query<RefRW<BoidSpawnerComponentData>>())
        {
            var spawnCount = spawnerData.ValueRO.boidCount;
            
            for (int i = 0; i < spawnCount; i++)
            {
                var boid = em.Instantiate(spawnerData.ValueRO.boidPrefab);
                ecb.AddComponent(boid, new BoidPropertyData());
                ecb.AddComponent(boid, new BoidMovementData());
            }
        }

        ecb.Playback(em);
        
        // Set the position of each of the boids.
        foreach (var (boid, entity) in
                 SystemAPI.Query<TransformAspect>().WithAll<BoidPropertyData>().WithEntityAccess())
        {
            var random = Random.CreateFromIndex((uint) entity.Index);

            boid.WorldPosition = new float3
            {
                x = random.NextFloat(-10, 10),
                y = random.NextFloat(-10, 10),
                z = random.NextFloat(-10, 10)
            };
            
        }

        Debug.Log("COMPLETED");
        // Disable the system.
        state.Enabled = false;

    }

}
