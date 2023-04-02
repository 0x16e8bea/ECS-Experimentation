using Unity.Entities;
using UnityEngine;

public struct BoidSpawnerComponentData : IComponentData
{
    public int boidCount;
    public Entity boidPrefab { get; set; }
}