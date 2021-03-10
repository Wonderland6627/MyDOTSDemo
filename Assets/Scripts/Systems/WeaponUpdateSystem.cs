using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;

/// <summary>
/// 用于同步手中剑的Position和Rotation
/// 与主角身上的武器位置同步
/// 通过WithAll来筛选WeaponEntity
/// </summary>
public class WeaponUpdateSystem : JobComponentSystem
{
    //1. ComponentSystem实现同步
    /*protected override void OnUpdate()
    {
        Entities
            .WithAll<WeaponTag>()
            .ForEach((ref Translation pos, ref Rotation rot) =>
        {
            pos = new Translation { Value = GameWorld.GetInstance().Player.weaponPos.position };
            rot = new Rotation { Value = GameWorld.GetInstance().Player.weaponPos.rotation };
        });
    }*/

    private EntityQuery weaponGroup;

    protected override void OnCreate()
    {
        base.OnCreate();

        weaponGroup = GetEntityQuery(typeof(WeaponTag), typeof(Translation), typeof(Rotation));//筛选出带有武器标签的实体
    }

    [BurstCompile]
    struct WeaponUpdateJob : IJobForEach<Translation, Rotation>
    {
        [ReadOnly]
        public float3 Postion;

        [ReadOnly]
        public quaternion Rotation;

        public void Execute(ref Translation translation, ref Rotation rotation)
        {
            translation.Value = Postion;
            rotation.Value = Rotation;
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
            Rotation = GameWorld.GetInstance().Player.weaponPos.rotation
        };

        return job.Schedule(weaponGroup, inputDeps);
    }
}
