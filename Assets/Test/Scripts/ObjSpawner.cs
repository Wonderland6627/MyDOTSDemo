using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class ObjSpawner : MonoBehaviour
{
    public GameObject ecsPrefab;

    public int countX;
    public int countY;
    public int countZ;

    private int amount = 0;

    private Entity entityPrefab;
    private EntityManager entityManager;

    private void Start()
    {
        entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(ecsPrefab,
            GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null));
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        CreateEntitiesCube();
    }

    private void CreateEntitiesCube()
    {
        float3 pos = new float3();
        for (int x = 0; x < countX; x++)
        {
            pos.y = 0;
            for (int y = 0; y < countY; y++)
            {
                pos.x = 0;
                for (int z = 0; z < countZ; z++)
                {
                    pos.x += 2f;
                    Entity temp = CreateEntityPrefab(entityPrefab);
                    entityManager.SetComponentData(temp, new Translation { Value = pos });
                }
                pos.y += 2f;
            }
            pos.z += 2f;
        }
    }

    private Entity CreateEntityPrefab(Entity entityPrefab)
    {
        var entity = entityManager.Instantiate(entityPrefab);

        return entity;
    }
}
