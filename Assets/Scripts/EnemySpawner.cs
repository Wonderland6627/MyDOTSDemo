using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    [SerializeField]
    private Entity enemtEntity;
    private EntityManager entityManager;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        enemtEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(enemyPrefab, settings);

        for (int i = 0; i < 34000; i++)
        {
            CreateEntities();
        }
    }

    private void CreateEntities()
    {
        float randX = UnityEngine.Random.Range(0, 2050f);
        float randZ = UnityEngine.Random.Range(0, 2050f);
        Vector3 randomPos = new Vector3(randX, 0, randZ);

        Entity enemy = entityManager.Instantiate(enemtEntity);
        entityManager.SetComponentData(enemy, new Translation() { Value = randomPos });
    }
}
