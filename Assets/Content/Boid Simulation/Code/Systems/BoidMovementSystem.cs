using Content.Assignments.Seperation;
using Content.Boid_Simulation.Code.Jobs;
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
    const float CellRadius = 1;

    // EntityQuery for boids
    private EntityQuery _boidQuery;

    // Called once when the system is created.
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BoidPropertyData>();
        
        _boidQuery = SystemAPI.QueryBuilder()
            .WithAllRW<LocalToWorld>()
            .WithAll<BoidMovementData>()
            .Build();
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
        var world = state.WorldUnmanaged;

        var positionsToBoidQueryIndices =
            new NativeMultiHashMap<int, int>(_boidQuery.CalculateEntityCount(), world.UpdateAllocator.ToAllocator);
        var positionsByQueryIndex = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(_boidQuery.CalculateEntityCount(), ref world.UpdateAllocator);
        var cellCohesionByQueryIndex = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(_boidQuery.CalculateEntityCount(), ref world.UpdateAllocator);
        var cellCountByHash = CollectionHelper.CreateNativeArray<int, RewindableAllocator>(_boidQuery.CalculateEntityCount(), ref world.UpdateAllocator);
        
        var indicesOfFirstBoidInEachChunk = _boidQuery.CalculateBaseEntityIndexArrayAsync(world.UpdateAllocator.ToAllocator, state.Dependency, out var boidChunkBaseIndexJobHandle);

        var initializeHashMapJob = new InitializeHashMapJob()
        {
            IndicesOfFirstBoidInEachChunk = indicesOfFirstBoidInEachChunk,
            InverseCellRadius = 1 / CellRadius,
            PositionsByQueryIndex = positionsByQueryIndex,
            ParallelHashMap = positionsToBoidQueryIndices.AsParallelWriter()
        };

        var initializeHashMapJobHandle = initializeHashMapJob.ScheduleParallel(_boidQuery, boidChunkBaseIndexJobHandle);
        initializeHashMapJobHandle.Complete();


        var initialCellCountJob = new MemsetNativeArray<int>
        {
            Source = cellCountByHash,
            Value = 1
        };

        var initialCellCountJobHandle = initialCellCountJob.Schedule(_boidQuery.CalculateEntityCount(), 64, state.Dependency);

        var handleHashCollisionJob = new HandleHashCollisionsJob
        {
            HashIndexedPositions = positionsByQueryIndex,
            CellCount = cellCountByHash,
        };

        JobHandle calculateCenterOfMassJobHandle = handleHashCollisionJob.Schedule(positionsToBoidQueryIndices, 64, initialCellCountJobHandle);
        calculateCenterOfMassJobHandle.Complete();

        var centerOfMassJob = new CalculateCenterOfMass()
        {
            HashIndexedPositions = positionsByQueryIndex,
            CellCount = cellCountByHash,
            BoidChunkBaseEntityIndexArray = indicesOfFirstBoidInEachChunk
        };

        var centerOfMassJobHandle = centerOfMassJob.ScheduleParallel(_boidQuery, calculateCenterOfMassJobHandle);
        centerOfMassJobHandle.Complete();


        var calculateCohesionJob = new CalculateCohesion()
        {
            PositionsByQueryIndex = positionsByQueryIndex,
            BoidChunkBaseEntityIndexArray = indicesOfFirstBoidInEachChunk,
            InverseCellRadius = 1 / CellRadius,
            PositionsToBoidQueryIndices = positionsToBoidQueryIndices,
            CohesionArray = cellCohesionByQueryIndex
        }.ScheduleParallel(_boidQuery, centerOfMassJobHandle);

        calculateCohesionJob.Complete();
        
        var steerJob = new SteerBoidsJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            BoidChunkBaseEntityIndexArray = indicesOfFirstBoidInEachChunk,
            DirectionsByQueryIndices = cellCohesionByQueryIndex
        }.ScheduleParallel(_boidQuery, calculateCohesionJob);
        
        
        steerJob.Complete();
        
        state.Dependency = steerJob;
        
        _boidQuery.AddDependency(state.Dependency);
        _boidQuery.ResetFilter();
    }
}

[BurstCompile]
partial struct CalculateCenterOfMass : IJobEntity
{
    [NativeDisableParallelForRestriction] public NativeArray<float3> HashIndexedPositions;
    [ReadOnly] public NativeArray<int> CellCount;
    [ReadOnly] public NativeArray<int> BoidChunkBaseEntityIndexArray;

    void Execute(
        [ChunkIndexInQuery] int chunkIndexInQuery,
        [EntityIndexInChunk] int entityIndexInChunk)
    {
        int entityIndexInQuery = BoidChunkBaseEntityIndexArray[chunkIndexInQuery] + entityIndexInChunk;

        HashIndexedPositions[entityIndexInQuery] /= CellCount[entityIndexInQuery];
    }
}

[BurstCompile]
partial struct CalculateCohesion : IJobEntity
{
    [ReadOnly] public NativeArray<float3> PositionsByQueryIndex;
    [ReadOnly] public NativeMultiHashMap<int, int> PositionsToBoidQueryIndices;
    [ReadOnly] public NativeArray<int> BoidChunkBaseEntityIndexArray;
    [NativeDisableParallelForRestriction] public NativeArray<float3> CohesionArray;
    [ReadOnly] public float InverseCellRadius;

    void Execute(
        [ChunkIndexInQuery] int chunkIndexInQuery,
        [EntityIndexInChunk] int entityIndexInChunk)
    {
        int entityIndexInQuery = BoidChunkBaseEntityIndexArray[chunkIndexInQuery] + entityIndexInChunk;

        float3 direction = float3.zero;
        int count = 0;

        // Check the neighboring cells for boids
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    var hash = (int)math.hash(new int3(
                        math.floor((PositionsByQueryIndex[entityIndexInQuery]  + new int3(x, y, z)) * InverseCellRadius)));
                    var enumerator = PositionsToBoidQueryIndices.GetValuesForKey(hash);

                    foreach (var index in enumerator)
                    {
                        if (index == entityIndexInQuery)
                            continue;

                        // Check if the distance is less than the radius
                        if (math.distance(PositionsByQueryIndex[entityIndexInQuery], PositionsByQueryIndex[index]) > 5)
                            continue;
                        
                        direction += PositionsByQueryIndex[index];
                        count++;
                    }
                }
            }
        }

        if (count > 0)
        {
            direction /= count;
            direction -= PositionsByQueryIndex[entityIndexInQuery];
        }

        CohesionArray[entityIndexInQuery] = direction;
    }
}