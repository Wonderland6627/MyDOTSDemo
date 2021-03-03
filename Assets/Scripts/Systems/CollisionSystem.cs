using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;

public class CollisionSystem : JobComponentSystem
{
    private EntityQuery playerGroup;
    private EntityQuery weaponGroup;

    private EntityQuery enemyGroup;

    protected override void OnCreate()
    {
        base.OnCreate();

        playerGroup = GetEntityQuery(typeof(EntityHealth), typeof(EntityCollision), typeof(Translation), typeof(Rotation), ComponentType.ReadOnly<PlayerTag>());
        enemyGroup = GetEntityQuery(typeof(EntityHealth), typeof(EntityCollision), typeof(Translation), typeof(Rotation), ComponentType.ReadOnly<EnemyTag>());
        weaponGroup = GetEntityQuery(typeof(Translation), typeof(EntityCollision), typeof(Rotation), ComponentType.ReadOnly<WeaponTag>());
    }

    [BurstCompile]
    struct CollisionJob : IJobChunk
    {
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            throw new System.NotImplementedException();
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        
        return default;
    }
}
