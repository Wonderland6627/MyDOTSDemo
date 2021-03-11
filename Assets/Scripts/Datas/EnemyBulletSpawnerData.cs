using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct EnemyBulletSpawnerData : IComponentData
{
    public Entity bulletPrefab;
}
