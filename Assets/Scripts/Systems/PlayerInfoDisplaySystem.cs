using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class PlayerInfoDisplaySystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .ForEach((in Translation pos) =>
            {
                Debug.Log(pos);
            })
            .WithAll<PlayerTag>()
            .Schedule();
    }
}
