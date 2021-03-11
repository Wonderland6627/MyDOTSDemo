using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

public class CollisionDetectSystem : JobComponentSystem
{
    private EntityQuery defenceGroup;
    private EntityQuery weaponGroup;
    private EntityQuery skillVfxGroup;
    private EntityQuery enemyGroup;
    private EntityQuery enemyBulletGroup;

    protected override void OnCreate()
    {
        base.OnCreate();

        defenceGroup = GetEntityQuery(typeof(EntityHealth), typeof(EntityCollision), typeof(Translation), ComponentType.ReadOnly<DefenceTag>());
        enemyGroup = GetEntityQuery(typeof(EntityHealth), typeof(EntityCollision), typeof(Translation), typeof(Rotation), typeof(EnemyState), ComponentType.ReadOnly<EnemyTag>());
        skillVfxGroup = GetEntityQuery(typeof(Translation), typeof(EntityCollision), ComponentType.ReadOnly<DangerousGoods>(), ComponentType.ReadOnly<SkillVFXTag>());
        weaponGroup = GetEntityQuery(typeof(Translation), typeof(EntityCollision), typeof(Rotation), ComponentType.ReadOnly<WeaponState>(), ComponentType.ReadOnly<DangerousGoods>(), ComponentType.ReadOnly<WeaponTag>());
        enemyBulletGroup = GetEntityQuery(typeof(Translation), typeof(EntityCollision), typeof(Rotation),ComponentType.ReadOnly<DangerousGoods>(), ComponentType.ReadOnly<EnemyBulletTag>());
    }

    [BurstCompile]
    struct WeaponCollisionJob : IJobChunk
    {
        [ReadOnly] public float radius;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkComponentType<EntityHealth> entitiesHealthType;

        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Translation> otherTranslationsArray;//武器的Array
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<WeaponState> weaponStatesArray;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<DangerousGoods> dangerousArray;

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
                    if (health.Value <= 0)
                    {
                        continue;
                    }

                    Translation pos2 = otherTranslationsArray[j];
                    WeaponState weaponState = weaponStatesArray[j];
                    DangerousGoods dangerous = dangerousArray[j];

                    if (weaponState.isAttacking)
                    {
                        if (MathExtension.CollisionStay(pos1.Value, pos2.Value, radius))
                        {
                            health.Value -= dangerous.power;
                            chunkHealths[i] = health;//struct是值类型 所以必须再赋值回去
                            Debug.Log(string.Format("武器击杀 {0}", pos1.Value));
                        }
                    }
                }
            }
        }
    }

    [BurstCompile]
    struct SkillVfxCollisionJob : IJobChunk
    {
        public float radius;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;

        public ArchetypeChunkComponentType<EntityHealth> entitiesHealthType;

        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Translation> otherTranslationsArray;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<DangerousGoods> dangerousArray;

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
                    if (health.Value <= 0)
                    {
                        continue;
                    }

                    Translation pos2 = otherTranslationsArray[j];
                    DangerousGoods dangerous = dangerousArray[j];
                    if (MathExtension.CollisionStay(pos1.Value, pos2.Value, radius))
                    {
                        health.Value -= dangerous.power;
                        chunkHealths[i] = health;
                        Debug.Log(string.Format("技能击杀 {0}", pos1.Value));
                    }
                }
            }
        }
    }

    struct DefenceCollisionJob : IJobChunk
    {
        public float radius;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;//角色的位置
        public ArchetypeChunkComponentType<EntityHealth> entitiesHealthType;//角色的生命值

        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Translation> otherTranslationsArray;//子弹位置
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<DangerousGoods> dangerousArray;//子弹威力

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
                    if (health.Value <= 0)
                    {
                        continue;
                    }

                    Translation pos2 = otherTranslationsArray[j];
                    DangerousGoods dangerous = dangerousArray[j];
                    if (MathExtension.CollisionStay(pos1.Value, pos2.Value, radius))
                    {
                        health.Value -= dangerous.power;
                        chunkHealths[i] = health;
                        //Debug.Log(string.Format("玩家受伤 剩余HP: {0}", health.Value));
                        //GameWorld.OnPlayerHPUpdate(health.Value);
                    }
                }
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var translationType = GetArchetypeChunkComponentType<Translation>(true);
        var healthType = GetArchetypeChunkComponentType<EntityHealth>();
        var dangerGoodsType = GetArchetypeChunkComponentType<DangerousGoods>(true);

        WeaponCollisionJob weaponCollisionEnemyJob = new WeaponCollisionJob//武器和敌人的检测
        {
            radius = 2,
            translationType = translationType,
            entitiesHealthType = healthType,
            otherTranslationsArray = weaponGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
            weaponStatesArray = weaponGroup.ToComponentDataArray<WeaponState>(Allocator.TempJob),
            dangerousArray = weaponGroup.ToComponentDataArray<DangerousGoods>(Allocator.TempJob),
        };
        weaponCollisionEnemyJob.Schedule(enemyGroup, inputDeps).Complete();

        DefenceCollisionJob denfenceJob = new DefenceCollisionJob()
        {
            radius = 1,
            translationType = translationType,
            entitiesHealthType = healthType,
            otherTranslationsArray = enemyBulletGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
            dangerousArray = enemyBulletGroup.ToComponentDataArray<DangerousGoods>(Allocator.TempJob),
        };
        denfenceJob.Schedule(defenceGroup, inputDeps).Complete();

        SkillVfxCollisionJob skillVfxCollisionEnemyJob = new SkillVfxCollisionJob//武器技能和敌人的检测
        {
            radius = 2,
            translationType = translationType,
            entitiesHealthType = healthType,
            otherTranslationsArray = skillVfxGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
            dangerousArray = skillVfxGroup.ToComponentDataArray<DangerousGoods>(Allocator.TempJob),
        };
        return skillVfxCollisionEnemyJob.Schedule(enemyGroup, inputDeps);

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
