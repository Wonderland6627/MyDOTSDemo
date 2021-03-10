using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

public class VFXFlyForwardSystem : JobComponentSystem
{
    //private EntityQuery skillVfxGroup;

    //protected override void OnCreate()
    //{
    //    base.OnCreate();

    //    skillVfxGroup = GetEntityQuery(typeof(Translation), typeof(MoveSpeed), ComponentType.ReadOnly<SkillVFXTag>());
    //}

    //[BurstCompile]
    //struct FlyJob : IJobChunk
    //{
    //    public float deltaTime;
    //    public float3 direction;

    //    public ArchetypeChunkComponentType<Translation> translationType;
    //    public ArchetypeChunkComponentType<MoveSpeed> moveSpeedType;

    //    public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
    //    {
    //        var chunkTranslations = chunk.GetNativeArray(translationType);
    //        var chunkMoveSpeeds = chunk.GetNativeArray(moveSpeedType);

    //        for (int i = 0; i < chunk.Count; i++)
    //        {
    //            float3 offset = chunkMoveSpeeds[i].Value * math.normalize(direction) * deltaTime;
    //            chunkTranslations[i] = new Translation
    //            {
    //                Value = chunkTranslations[i].Value + offset,
    //            };
    //        }
    //    }
    //}

    //protected override JobHandle OnUpdate(JobHandle inputDeps)
    //{
    //    FlyJob job = new FlyJob
    //    {
    //        deltaTime = Time.DeltaTime,
    //        direction = new float3(GameWorld.GetInstance().Player.transform.forward),
    //        translationType = GetArchetypeChunkComponentType<Translation>(),
    //        moveSpeedType = GetArchetypeChunkComponentType<MoveSpeed>(),
    //    };

    //    return job.Schedule(skillVfxGroup, inputDeps);
    //}


    [BurstCompile]
    struct MoveJob : IJobForEach<Translation, Rotation, MoveSpeed, SkillVFXTag>
    {
        [ReadOnly]
        public float deltaTime;

        public void Execute(ref Translation pos, ref Rotation rot, ref MoveSpeed moveSpeed, ref SkillVFXTag tag)
        {
            pos.Value += moveSpeed.Value * math.forward(rot.Value) * deltaTime;
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
