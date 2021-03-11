using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[DisableAutoCreation]
public class DefenceListeningSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .WithChangeFilter<EntityHealth>()
            .ForEach((ref DefencePreData data ,in EntityHealth health) =>
            {
                if (data.preValue != health.Value)
                {
                    GameWorld.OnPlayerHPUpdate(health.Value);
                }
                data.preValue = health.Value;
            })
            .Run();
    }
}
