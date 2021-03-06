﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class EnemyOtherDetectSystem : JobComponentSystem
{
    private EntityQuery defenceGroup;
    private EntityQuery enemyGroup;

    protected override void OnCreate()
    {
        base.OnCreate();

        defenceGroup = GetEntityQuery(typeof(EntityHealth), typeof(EntityCollision), typeof(Translation), ComponentType.ReadOnly<DefenceTag>());
        enemyGroup = GetEntityQuery(typeof(EntityHealth), typeof(EntityCollision), typeof(Translation), typeof(Rotation), typeof(EnemyState), ComponentType.ReadOnly<EnemyTag>());
    }

    [BurstCompile]
    struct StateCollisionJob : IJobChunk
    {
        [ReadOnly] public float radius;

        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        [ReadOnly] public ArchetypeChunkComponentType<Rotation> rotataionType;
        public ArchetypeChunkComponentType<EnemyState> enemiesStateType;

        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Translation> otherTranslationsArray;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<EntityHealth> entityHealthsArray;

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
                    //EntityHealth defenceHealth = entityHealthsArray[j];
                    Translation pos2 = otherTranslationsArray[j];

                    if (MathExtension.CollisionStay(forwardPos, pos2.Value, radius))
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
    struct OtherCollisionJob : IJobChunk
    {
        public float radius;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        [ReadOnly] public ArchetypeChunkComponentType<Rotation> rotataionType;
        public ArchetypeChunkComponentType<EnemyState> enemiesStateType;

        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Translation> otherTranslationsArray;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray(translationType);
            var chunkRotations = chunk.GetNativeArray(rotataionType);
            var chunkEnemiesStates = chunk.GetNativeArray(enemiesStateType);

            for (int i = 0; i < chunk.Count; i++)
            {
                Translation pos1 = chunkTranslations[i];
                Rotation rot1 = chunkRotations[i];
                EnemyState state1 = chunkEnemiesStates[i];
                float3 forwardPos = pos1.Value + math.forward(rot1.Value) * radius / 2f;

                for (int j = 0; j < otherTranslationsArray.Length; j++)
                {
                    Translation pos2 = otherTranslationsArray[j];
                    if (pos1.Value.Equals(pos2.Value))
                    {
                        continue;
                    }

                    //EnemyState state2 = enemiesStatesArray[j];             
                    if (MathExtension.CollisionStay(pos1.Value, pos2.Value, radius))//如果生成的时候挨得很近 先主动走开
                    {
                        state1.BehaviourState = EnemyBehaviourState.Move;
                        chunkEnemiesStates[i] = state1;
                    }
                    else if (MathExtension.CollisionStay(forwardPos, pos2.Value, radius))//面前有同类 站着不动 等待转向
                    {
                        state1.BehaviourState = EnemyBehaviourState.Idle;
                        chunkEnemiesStates[i] = state1;
                    }
                }
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var translationType = GetArchetypeChunkComponentType<Translation>(true);//参数表示是否ReadOnly
        var rotationType = GetArchetypeChunkComponentType<Rotation>(true);
        var enemiesStateType = GetArchetypeChunkComponentType<EnemyState>();

        StateCollisionJob playerCollisionEnemyJob = new StateCollisionJob//主角和敌人的检测
        {
            radius = 5,
            translationType = translationType,
            rotataionType = rotationType,
            enemiesStateType = enemiesStateType,
            otherTranslationsArray = defenceGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
            entityHealthsArray = defenceGroup.ToComponentDataArray<EntityHealth>(Allocator.TempJob),
        };

        playerCollisionEnemyJob.Schedule(enemyGroup, inputDeps).Complete();

        OtherCollisionJob enemyCollisionEnemyJob = new OtherCollisionJob()//敌人之间距离检测
        {
            radius = 2.75f,
            translationType = translationType,
            rotataionType = rotationType,
            enemiesStateType = enemiesStateType,
            otherTranslationsArray = enemyGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
        };

        return enemyCollisionEnemyJob.Schedule(enemyGroup, inputDeps);
    }
}
