using System.Linq;
using Content.Assignments.Seperation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateAfter(typeof(BoidSpawnerSystem))]

public partial struct BoidMovementSystem : ISystem
{
    // EntityQuery for boids
    private EntityQuery _boidQuery;
    
    // Called once when the system is created.
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
        // Use the querybuilder
        _boidQuery = state.GetEntityQuery(
            new EntityQueryBuilder(Allocator.Persistent)
                .WithAll<BoidPropertyData>()
                .WithAll<WorldTransform>()
        );
        
        state.RequireForUpdate<BoidPropertyData>();
        
    }

    // Called once when the system is destroyed.
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    // Usually called every frame. When exactly a system is updated
    // is determined by the system group to which it belongs.
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        var boidWorldTransforms = _boidQuery.ToComponentDataArray<WorldTransform>(Allocator.TempJob);

        var calculateCenterOfMassJob = new CalculateCenterOfMassJob()
        {
            BoidPositions = boidWorldTransforms, 
            CenterOfMassResultArray = new NativeArray<float3>(_boidQuery.CalculateEntityCount(), Allocator.TempJob)
        };

        JobHandle handle = calculateCenterOfMassJob.Schedule(_boidQuery.CalculateEntityCount(), 64);
        handle.Complete();

        new MoveBoidJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime, CenterOfMassResultArray = calculateCenterOfMassJob.CenterOfMassResultArray
        }.ScheduleParallel();







    }
}