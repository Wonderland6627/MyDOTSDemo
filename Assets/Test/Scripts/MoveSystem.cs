using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

public class MoveSystem : JobComponentSystem
{
    private EntityQuery entityQuery;//实体查询

    protected override void OnCreate()
    {
        entityQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<MoveSpeed>());
    }

    [BurstCompile]
    struct MoveJob : IJobChunk
    {
        public float dt;
        public float3 direction;
        public ArchetypeChunkComponentType<Translation> translationType;
        [ReadOnly] public ArchetypeChunkComponentType<MoveSpeed> moveSpeedType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var translationArray = chunk.GetNativeArray(translationType);
            var moveSpeedArray = chunk.GetNativeArray(moveSpeedType);

            for (int i = 0; i < chunk.Count; i++)
            {
                float3 offset = dt * moveSpeedArray[i].Value * math.normalize(direction);
                translationArray[i] = new Translation()
                {
                    Value = translationArray[i].Value + offset
                };
            }
        }
    }


    /*[BurstCompile]
    struct MoveUpJob : IJobChunk
    {
        public float dt;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            for (int i = 0; i < chunk.Count; i++)
            {

            }
        }
    }*/

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        MoveJob job = new MoveJob()
        {
            dt = UnityEngine.Time.deltaTime,
            direction = new float3(0, 0, 1),
            translationType = GetArchetypeChunkComponentType<Translation>(),
            moveSpeedType = GetArchetypeChunkComponentType<MoveSpeed>(true)
        };

        //Debug.Log(job);

        return job.Schedule(entityQuery, inputDeps);
    }
}
