using Unity.Entities;
using UnityEngine;

public class BoidSpawnerBaker : Baker<BoidSpawnerMono>
{
    public override void Bake(BoidSpawnerMono authoring)
    {
        if (authoring.boidPrefab == null)
            throw new System.Exception("Boid prefab is null");

        AddComponent(new BoidSpawnerComponentData() {
            boidCount = authoring.boidCount,
            boidPrefab = GetEntity(authoring.boidPrefab)
        });
    }
}

public class BoidSpawnerMono : MonoBehaviour
{
    public int boidCount = 100;
    public GameObject boidPrefab;
}