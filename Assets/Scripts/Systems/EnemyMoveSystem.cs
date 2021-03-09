using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

public class EnemyMoveSystem : JobComponentSystem
{
    [BurstCompile]
    struct MoveJob : IJobForEach<Translation, Rotation, MoveSpeed, EnemyState>
    {
        public float deltaTime;

        public void Execute(ref Translation pos, ref Rotation rot, ref MoveSpeed moveSpeed, ref EnemyState state)
        {
            if (state.BehaviourState == EnemyBehaviourState.Idle || state.BehaviourState == EnemyBehaviourState.Attack)
            {
                return;
            }

            state.moveWaitTime += deltaTime;
            if (state.moveWaitTime > 0 && state.moveWaitTime < EnemyStateTime.IdleTimeValue)
            {
                return;
            }
            else if (state.moveWaitTime >= EnemyStateTime.IdleTimeValue && state.moveWaitTime < state.moveStartTime)
            {
                pos.Value += moveSpeed.Value * math.forward(rot.Value) * deltaTime;
                float3 clampPos = pos.Value;
                clampPos = math.clamp(clampPos, new float3(0, 0, 0), new float3(1024f, 0f, 1024f));

                pos.Value = clampPos;
            }
            else
            {
                state.moveWaitTime = 0;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        MoveJob job = new MoveJob()
        {
            deltaTime = Time.DeltaTime,
        };

        return job.Schedule(this, inputDeps);
    }
}
