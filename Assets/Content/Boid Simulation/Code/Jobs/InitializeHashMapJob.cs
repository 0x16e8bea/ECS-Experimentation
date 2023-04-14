using Content.Assignments.Seperation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Content.Boid_Simulation.Code.Jobs
{
    [BurstCompile]
    public partial struct InitializeHashMapJob : IJobEntity
    {
        public NativeMultiHashMap<int, int>.ParallelWriter ParallelHashMap { get; set; }
        [NativeDisableParallelForRestriction] public NativeArray<float3> PositionsByQueryIndex;
        public float InverseCellRadius { get; set; }

        [ReadOnly] public NativeArray<int> IndicesOfFirstBoidInEachChunk;
        
        void Execute(
            [ChunkIndexInQuery] int chunkIndexInQuery,
            [EntityIndexInChunk] int entityIndexInChunk,
            in LocalToWorld localToWorld)
        {
            int entityIndexInQuery = IndicesOfFirstBoidInEachChunk[chunkIndexInQuery] + entityIndexInChunk;

            var hash = (int) math.hash(new int3(math.floor(localToWorld.Position * InverseCellRadius)));
            
            PositionsByQueryIndex[entityIndexInQuery] = localToWorld.Position;
            ParallelHashMap.Add(hash, entityIndexInQuery);
        }
    }
}