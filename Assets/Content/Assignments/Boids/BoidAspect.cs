using Content.Assignments.Seperation;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct BoidAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly TransformAspect _transform;
        private readonly RefRO<BoidPropertyData> _boidPropertyData;
        private readonly RefRW<BoidMovementData> _boidMovementData;

        private float MaxSpeed => _boidPropertyData.ValueRO.MaxSpeed;
        private float VisualRange => _boidPropertyData.ValueRO.VisualRange;
        
        private float3 Heading => _boidMovementData.ValueRW.Heading;
        private float3 Velocity => _boidMovementData.ValueRW.Velocity;

        public void Move(float3 dir, float deltaTime)
        {
            _transform.WorldTransform = new WorldTransform
            {
                Position = _transform.WorldTransform.Position + dir * deltaTime,
                Rotation = Quaternion.LookRotation(dir),
                Scale = _transform.WorldTransform.Scale
            };
        }
    }