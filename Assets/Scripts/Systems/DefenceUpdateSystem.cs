using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;

public class DefenceUpdateSystem : JobComponentSystem
{
    private EntityQuery weaponGroup;

    protected override void OnCreate()
    {
        base.OnCreate();

        weaponGroup = GetEntityQuery(typeof(DefenceTag), typeof(Translation));
    }

    [BurstCompile]
    struct WeaponUpdateJob : IJobForEach<Translation>
    {
        [Unity.Collections.ReadOnly] public float3 Postion;

        public void Execute(ref Translation translation)
        {
            translation.Value = Postion;
        }
    }

    /// <summary>
    /// 2. JobComponentSystem实现同步
    /// </summary>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        WeaponUpdateJob job = new WeaponUpdateJob
        {
            Postion = GameWorld.GetInstance().Player.weaponPos.position,
        };

        return job.Schedule(weaponGroup, inputDeps);
    }
}
