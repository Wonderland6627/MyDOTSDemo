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
    private EntityQuery playerGroup;
    private EntityQuery weaponGroup;
    private EntityQuery skillVfxGroup;
    private EntityQuery enemyGroup;

    protected override void OnCreate()
    {
        base.OnCreate();

        playerGroup = GetEntityQuery(typeof(EntityHealth), typeof(EntityCollision), typeof(Translation), typeof(Rotation), ComponentType.ReadOnly<PlayerTag>());
        enemyGroup = GetEntityQuery(typeof(EntityHealth), typeof(EntityCollision), typeof(Translation), typeof(Rotation), typeof(EnemyState), ComponentType.ReadOnly<EnemyTag>());
        skillVfxGroup = GetEntityQuery(typeof(Translation), typeof(EntityCollision), ComponentType.ReadOnly<SkillVFXTag>());
        weaponGroup = GetEntityQuery(typeof(Translation), typeof(EntityCollision), typeof(Rotation), ComponentType.ReadOnly<WeaponState>(), ComponentType.ReadOnly<WeaponTag>());
    }

    [BurstCompile]
    struct WeaponCollsionJob : IJobChunk
    {
        public float radius;
        public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkComponentType<EntityHealth> entitiesHealthType;

        [DeallocateOnJobCompletion]
        public NativeArray<Translation> otherTranslationsArray;//武器的Array
        [DeallocateOnJobCompletion]
        public NativeArray<WeaponState> weaponStatesArray;

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

                    if (weaponState.isAttacking)
                    {
                        if (CheckCollision(pos1.Value, pos2.Value, radius))
                        {
                            health.Value -= 100;
                            chunkHealths[i] = health;//struct是值类型 所以必须再赋值回去
                            Debug.Log(string.Format("武器击杀 {0}", pos1.Value));
                        }
                    }
                }
            }
        }
    }

    [BurstCompile]
    struct SkillVfxCollsionJob : IJobChunk
    {
        public float radius;
        public ArchetypeChunkComponentType<Translation> translationType;

        public ArchetypeChunkComponentType<EntityHealth> entitiesHealthType;

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
                    if (health.Value <= 0)
                    {
                        continue;
                    }

                    Translation pos2 = otherTranslationsArray[j];
                    if (CheckCollision(pos1.Value, pos2.Value, radius))
                    {
                        health.Value -= 100;
                        chunkHealths[i] = health;
                        Debug.Log(string.Format("技能击杀 {0}", pos1.Value));
                    }
                }
            }
        }
    }

    [BurstCompile]
    struct StateCollsionJob : IJobChunk
    {
        public float radius;

        public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkComponentType<Rotation> rotataionType;
        public ArchetypeChunkComponentType<EnemyState> enemiesStateType;

        [DeallocateOnJobCompletion]
        public NativeArray<Translation> otherTranslationsArray;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray(translationType);
            var chunkRotations = chunk.GetNativeArray(rotataionType);
            var chunkEnemiesStates = chunk.GetNativeArray(enemiesStateType);

            for (int i = 0; i < chunk.Count; i++)
            {
                Translation pos1 = chunkTranslations[i];
                Rotation rot1 = chunkRotations[i];
                EnemyState state = chunkEnemiesStates[i];
                float3 forwardPos = pos1.Value + math.forward(rot1.Value) * 5;
                //Debug.Log(string.Format("物体位置{0} 探测位置{1}", pos1.Value, forwardPos));

                for (int j = 0; j < otherTranslationsArray.Length; j++)
                {
                    Translation pos2 = otherTranslationsArray[j];
                    if (CheckCollision(forwardPos, pos2.Value, radius))
                    {
                        state.BehaviourState = EnemyBehaviourState.Attack;
                        chunkEnemiesStates[i] = state;
                    }
                    else
                    {
                        state.BehaviourState = EnemyBehaviourState.Move;
                        chunkEnemiesStates[i] = state;
                    }
                }
            }
        }
    }

    [BurstCompile]
    struct OtherCollsionJob : IJobChunk
    {
        public float radius;
        public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkComponentType<EnemyState> enemiesStateType;

        [DeallocateOnJobCompletion]
        public NativeArray<Translation> otherTranslationsArray;
        [DeallocateOnJobCompletion]
        public NativeArray<EnemyState> enemiesStatesArray;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray(translationType);
            var chunkEnemiesStates = chunk.GetNativeArray(enemiesStateType);

            for (int i = 0; i < chunk.Count; i++)
            {
                Translation pos1 = chunkTranslations[i];
                EnemyState state1 = chunkEnemiesStates[i];

                for (int j = 0; j < otherTranslationsArray.Length; j++)
                {
                    Translation pos2 = otherTranslationsArray[j];
                    if(pos1.Value.Equals(pos2.Value))
                    {
                        continue;
                    }

                    EnemyState state2 = enemiesStatesArray[j];
                    if (CheckCollision(pos1.Value, pos2.Value, radius))
                    {
                        state1.BehaviourState = EnemyBehaviourState.Idle;
                        chunkEnemiesStates[i] = state1;

                        state2.BehaviourState = EnemyBehaviourState.Idle;
                        enemiesStatesArray[j] = state2;
                    }
                }
            }
        }
    }

    static bool CheckCollision(float3 posA, float3 posB, float radius)
    {
        float3 delta = posA - posB;
        float distance = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;

        return distance <= radius * radius;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var translationType = GetArchetypeChunkComponentType<Translation>();
        var rotationType = GetArchetypeChunkComponentType<Rotation>();
        var healthType = GetArchetypeChunkComponentType<EntityHealth>();
        var enemiesStateType = GetArchetypeChunkComponentType<EnemyState>();

        WeaponCollsionJob weaponCollisionEnemyJob = new WeaponCollsionJob//武器和敌人的检测
        {
            radius = 2,
            translationType = translationType,
            entitiesHealthType = healthType,
            otherTranslationsArray = weaponGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
            weaponStatesArray = weaponGroup.ToComponentDataArray<WeaponState>(Allocator.TempJob),
        };
        weaponCollisionEnemyJob.Schedule(enemyGroup, inputDeps).Complete();

        SkillVfxCollsionJob skillVfxCollisionEnemyJob = new SkillVfxCollsionJob//武器技能和敌人的检测
        {
            radius = 2,
            translationType = translationType,
            entitiesHealthType = healthType,
            otherTranslationsArray = skillVfxGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
        };
        skillVfxCollisionEnemyJob.Schedule(enemyGroup, inputDeps).Complete();

        StateCollsionJob playerCollisionEnemyJob = new StateCollsionJob//主角和敌人的检测
        {
            radius = 5,
            translationType = translationType,
            rotataionType = rotationType,
            enemiesStateType = enemiesStateType,
            otherTranslationsArray = weaponGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
        };

        playerCollisionEnemyJob.Schedule(enemyGroup, inputDeps).Complete();

        OtherCollsionJob enemyCollisionEnemyJob = new OtherCollsionJob()//敌人之间距离检测
        {
            radius = 3,
            translationType = translationType,
            enemiesStateType = enemiesStateType,
            otherTranslationsArray = enemyGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
            enemiesStatesArray = enemyGroup.ToComponentDataArray<EnemyState>(Allocator.TempJob),
        };

        return enemyCollisionEnemyJob.Schedule(enemyGroup, inputDeps);

        /*StateCollsionJob stateCollsionJob = new StateCollsionJob
        {
            radius = 3,
            translationType = translationType,
            enemiesStateType = enemiesStateType,
            otherTranslationsArray = enemyGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
        };
        return stateCollsionJob.Schedule(enemyGroup, inputDeps);*/

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
