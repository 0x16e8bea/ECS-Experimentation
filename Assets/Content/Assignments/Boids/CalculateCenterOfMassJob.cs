using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct CalculateCenterOfMassJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<WorldTransform> BoidPositions;

    public NativeArray<float3> CenterOfMassResultArray;

    public void Execute(int index)
    {
        WorldTransform boidPosition = BoidPositions[index];
        int numberOfBoidsForCenterOfMass = 0;

        for (var j = 0; j < BoidPositions.Length; j++)
        {
            if (index == j)
            {
                continue;
            }

            WorldTransform otherBoidPosition = BoidPositions[j];

            float distance = math.distancesq(boidPosition.Position, otherBoidPosition.Position);

            const int viewDistance = 100;
            if (!(distance < viewDistance)) continue;

            CenterOfMassResultArray[index] += otherBoidPosition.Position - boidPosition.Position;
            numberOfBoidsForCenterOfMass++;
        }

        CenterOfMassResultArray[index] /= numberOfBoidsForCenterOfMass;
    }
}

/* 
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct CalculateCenterOfMassJob : IJobEntity
{
    [ReadOnly] public NativeArray<WorldTransform> BoidPositions;
    
    public NativeArray<float3> CenterOfMassResultArray;
    
    public void Execute(int batchIndexInQuery, [EntityIndexInQuery] int entityIndexInQuery, ref BoidAspect boidAspect)
    {
        for (var i = 0; i < BoidPositions.Length; i++)
        {
            WorldTransform boidPosition = BoidPositions[i];
            int numberOfBoidsForCenterOfMass = 0;
            
            for (var j = 0; j < BoidPositions.Length; j++)
            {
                if (i == j)
                {
                    continue;
                }
                
                WorldTransform otherBoidPosition = BoidPositions[j];
                
                float distance = math.distancesq(boidPosition.Position, otherBoidPosition.Position);

                const int viewDistance = 100;
                if (!(distance < viewDistance)) continue;
                
                CenterOfMassResultArray[i] += otherBoidPosition.Position - boidPosition.Position;
                numberOfBoidsForCenterOfMass++;
            }
            
            CenterOfMassResultArray[i] /= numberOfBoidsForCenterOfMass;
        }
    }
}

*/ 