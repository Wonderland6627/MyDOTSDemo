using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// 用于同步手中剑的Position和Rotation
/// 与主角身上的武器位置同步
/// 通过WithAll来筛选WeaponEntity
/// </summary>
public class WeaponUpdateSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities
            .WithAll<WeaponTag>()
            .ForEach((ref Translation pos, ref Rotation rot) =>
        {
            pos = new Translation { Value = GameWorld.GetInstance().Player.weaponPos.position };
            rot = new Rotation { Value = GameWorld.GetInstance().Player.weaponPos.rotation };
        });
    }
}
