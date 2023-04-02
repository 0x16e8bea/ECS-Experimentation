using Content.Assignments.Seperation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct BoidSpawnerSystem : ISystem { 
    
    private ComponentTypeHandle<BoidSpawnerComponentData> _boidSpawnerComponentDataHandle;
    private EntityTypeHandle _entityHandle;

    // Called once when the system is created.
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BoidSpawnerComponentData>();
        
        _boidSpawnerComponentDataHandle = state.GetComponentTypeHandle<BoidSpawnerComponentData>();
        _entityHandle = state.GetEntityTypeHandle();
        
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
        
        foreach (var (spawnerData, e) in
                 SystemAPI.Query<RefRW<BoidSpawnerComponentData>>()
                     .WithEntityAccess())
        {
            var spawnCount = spawnerData.ValueRO.boidCount;
            
            for (int i = 0; i < spawnCount; i++)
            {
                // Get the transform aspect for the component that holds the spawner data.
                var spawnerTransformAspect = SystemAPI.GetAspectRW<TransformAspect>(e);
                var spawnPosition = spawnerTransformAspect.WorldPosition;
                
                var boid = em.Instantiate(spawnerData.ValueRO.boidPrefab);
                
                // Get the transform aspect of the boid.
                var transformAspect = SystemAPI.GetAspectRW<TransformAspect>(boid);

                transformAspect.LocalPosition = new Vector3(
                    spawnPosition.x + Random.insideUnitSphere.x * 15,
                    spawnPosition.y + Random.insideUnitSphere.y * 15,
                    spawnPosition.z + Random.insideUnitSphere.z * 15);
                ecb.AddComponent(boid, new BoidPropertyData());
                ecb.AddComponent(boid, new BoidMovementData());

            }
        }
        
        ecb.Playback(em);
        
        // Disable the system.
        state.Enabled = false;

    }

}
