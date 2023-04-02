using Unity.Entities;
using Unity.Mathematics;

namespace Content.Assignments.Seperation
{
    public struct BoidMovementData : IComponentData
    {
        public float3 Velocity;
        public float3 Heading;
    }
}