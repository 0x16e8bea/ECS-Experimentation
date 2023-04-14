using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public partial struct HandleHashCollisionsJob : IJobNativeParallelMultiHashMapMergedSharedKeyIndices
{
    public NativeArray<float3> HashIndexedPositions;
    public NativeArray<int> CellCount;
    
    public void ExecuteFirst(int index)
    {
        var position = HashIndexedPositions[index] / CellCount[index];
    }

    public void ExecuteNext(int firstIndex, int index)
    {
        CellCount[firstIndex]++;
        HashIndexedPositions[firstIndex] += HashIndexedPositions[index];
    }
}