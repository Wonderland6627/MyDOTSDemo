using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

[DisableAutoCreation]
public class SystemBaseTest : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .ForEach((ref Translation pos) =>
            {
                Debug.Log(string.Format("{0}", pos.Value));
            })
            .WithAny<WeaponTag>()
            .Schedule();
    }
}
