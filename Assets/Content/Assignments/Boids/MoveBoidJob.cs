using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[WithAll(typeof(BoidAspect))]
partial struct MoveBoidJob : IJobEntity
{
    public NativeArray<float3> CenterOfMassResultArray;
    public float DeltaTime;

    public void Execute([EntityIndexInQuery] int entityIndexInQuery, ref BoidAspect boidAspect)
    {
        boidAspect.Move(CenterOfMassResultArray[entityIndexInQuery], DeltaTime);
    }
    
}
