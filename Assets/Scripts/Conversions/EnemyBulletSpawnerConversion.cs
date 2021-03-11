using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class EnemyBulletSpawnerConversion : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject bulletPrefabGo;

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(bulletPrefabGo);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new EnemyBulletSpawnerData
        {
            bulletPrefab = conversionSystem.GetPrimaryEntity(bulletPrefabGo),
        };

        dstManager.AddComponentData(entity, data);
    }
}
