using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

public class EnemyRotateSystem : JobComponentSystem
{
    [BurstCompile]
    struct RotateJob : IJobForEach<Translation, Rotation, RotateSpeed, EnemyState>
    {
        [ReadOnly]
        public float deltaTime;
        public float3 targetPos;

        [ReadOnly] public float3 randPos;
        [ReadOnly] public float3 playerPos;

        public void Execute(ref Translation pos, ref Rotation rot, ref RotateSpeed rotateSpeed, ref EnemyState state)
        {
            /*if (state.BehaviourState == EnemyBehaviourState.Idle)
            {
                return;
            }*/

            if (state.BehaviourState == EnemyBehaviourState.Attack)
            {
                targetPos = playerPos;
            }
            else
            {
                targetPos = randPos;
            }

            state.stateTime -= deltaTime;
            if (state.stateTime <= 0)
            {
                state.stateTime = state.Duration;

                float3 heading = targetPos - pos.Value;
                heading.y = 0;
                rot.Value = quaternion.LookRotation(heading, math.up() * deltaTime);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //System.Random rand = new System.Random(DateTime.Now.Second);
        float randX = UnityEngine.Random.Range(-1024, 2048f);
        float randZ = UnityEngine.Random.Range(-1024, 2048f);
        float3 randPos = new float3(randX, 0, randZ);

        RotateJob job = new RotateJob()
        {
            randPos = randPos,
            playerPos = GameWorld.GetInstance().Player.transform.position,
            deltaTime = Time.DeltaTime,
        };

        return job.Schedule(this, inputDeps);
    }
}
