using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

[DisableAutoCreation]
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

            pos.Value += moveSpeed.Value * math.forward(rot.Value) * deltaTime;
            float3 clampPos = pos.Value;
            clampPos = math.clamp(clampPos, new float3(0, 0, 0), new float3(1024, 0, 1024f));

            pos.Value = clampPos;
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
