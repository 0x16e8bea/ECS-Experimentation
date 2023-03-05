using Unity.Entities;

namespace Content.Assignments.Seperation
{
    public struct BoidPropertyData : IComponentData
    {
        public float MaxSpeed;
        public float VisualRange;
        public float ProtectedRange;
    }
}