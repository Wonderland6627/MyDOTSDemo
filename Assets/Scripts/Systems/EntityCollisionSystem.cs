using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

public class EntityCollisionSystem : JobComponentSystem
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
    struct CollsionJob : IJobChunk
    {
        public float radius;
        public ArchetypeChunkComponentType<EntityHealth> entitiesHealthType;
        public ArchetypeChunkComponentType<Translation> translationType;

        [DeallocateOnJobCompletion]
        public NativeArray<Translation> otherTranslationsArray;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray(translationType);
            var chunkHealths = chunk.GetNativeArray(entitiesHealthType);

            for (int i = 0; i < chunk.Count; i++)
            {
                Translation pos1 = chunkTranslations[i];
                EntityHealth health = chunkHealths[i];

                for (int j = 0; j < otherTranslationsArray.Length; j++)
                {
                    Translation pos2 = otherTranslationsArray[j];
                    if (CheckCollision(pos1.Value, pos2.Value, radius))
                    {
                        health.Value -= 10;
                        chunkHealths[i] = health;//struct是值类型 所以必须再赋值回去
                        Debug.Log(string.Format("碰撞 pos1 {0} pos2 {1}", pos1.Value, pos2.Value));
                    }
                }
            }
        }
    }

    static bool CheckCollision(float3 posA, float3 posB, float radius)
    {
        float3 delta = posA - posB;
        float distance = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;

        return distance <= radius;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var translationType = GetArchetypeChunkComponentType<Translation>();
        var healthType = GetArchetypeChunkComponentType<EntityHealth>();

        CollsionJob job = new CollsionJob
        {
            radius = 2,
            translationType = translationType,
            entitiesHealthType = healthType,
            otherTranslationsArray = weaponGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
        };

        return job.Schedule(enemyGroup, inputDeps);
        return default;
    }

    /*[BurstCompile]
    struct EntityCollisionJob : IJobChunk
    {
        //public float radius;
        //public ArchetypeChunkComponentType<EntityHealth> entityHealthType;
        public ArchetypeChunkComponentType<EntityCollision> collisionType;

        public ArchetypeChunkComponentType<Translation> translationType;

        [DeallocateOnJobCompletion]//不加这个属性会溢出
        public NativeArray<Translation> otherTranslationsArray;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray(translationType);
            var chunkCollisions = chunk.GetNativeArray(collisionType);
            for (int i = 0; i < chunkCollisions.Length; i++)
            {
                Translation pos = chunkTranslations[i];
                float4 posA = new float4(pos.Value, chunkCollisions[i].Radius);
                for (int j = 0; j < otherTranslationsArray.Length; j++)
                {
                    Translation pos2 = otherTranslationsArray[j];
                    float4 posB = new float4(pos2.Value, chunkCollisions[j].Radius);

                    if (CheckCollision(posA, posB))
                    {
                        Debug.Log("Collision");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 用一个float4 xyz代表坐标 w代表radius之和
    /// </summary>
    static bool CheckCollision(float4 posA, float4 posB)
    {
        float4 delta = posA - posB;
        float distanceSquare = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
        float radius = posA.w + posB.w;
        Debug.Log(string.Format("posA{0}  posB{1}", posA, posB));
        Debug.Log(string.Format("实际距离{0}  预计碰撞半径和{1}", distanceSquare, radius));

        return distanceSquare <= radius;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //var healthType = GetArchetypeChunkComponentType<EntityHealth>();
        var translationType = GetArchetypeChunkComponentType<Translation>();
        var collisionType = GetArchetypeChunkComponentType<EntityCollision>();

        EntityCollisionJob job = new EntityCollisionJob
        {
            collisionType = collisionType,
            translationType = translationType,
            otherTranslationsArray = enemyGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
        };

        return job.Schedule(weaponGroup, inputDeps);

        return default;
    }*/
}
