using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[WithAll(typeof(BoidAspect))]
partial struct SteerBoidsJob : IJobEntity
{
    [ReadOnly] public NativeArray<int> BoidChunkBaseEntityIndexArray;
    [ReadOnly] public NativeArray<float3> DirectionsByQueryIndices;
    [ReadOnly] public float DeltaTime;

    public void Execute(            
        [ChunkIndexInQuery] int chunkIndexInQuery,
        [EntityIndexInChunk] int entityIndexInChunk,
        ref BoidAspect boidAspect)
    {
        int entityIndexInQuery = BoidChunkBaseEntityIndexArray[chunkIndexInQuery] + entityIndexInChunk;
        
        boidAspect.Move(DirectionsByQueryIndices[entityIndexInQuery], DeltaTime);
    }
    
}
