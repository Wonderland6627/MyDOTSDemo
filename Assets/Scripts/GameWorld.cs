using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class GameWorld : MonoBehaviour
{
    #region Instance

    private static GameWorld _instance = null;
    public static GameWorld GetInstance()
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType(typeof(GameWorld)) as GameWorld;
            if (_instance == null)
            {
                var go = new GameObject("GameWorld");
                _instance = go.AddComponent<GameWorld>();
            }

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(_instance.gameObject);
            }
        }

        return _instance;
    }

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
    #endregion

    private UnitCharacter player;
    public UnitCharacter Player
    {
        get
        {
            if (player == null)
            {
                var character = FindObjectOfType<UnitCharacter>();
                if (character)
                {
                    player = character;
                }
            }

            return player;
        }
    }
    public Entity CurrentWeapon;

    private GameObject enemyPrefab;
    private GameObject weaponPrefab;

    private Entity enemyEntity;
    private Entity weaponEntity;

    private EntityManager entityManager;
    private GameObjectConversionSettings settings;

    public void Init()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);

        enemyPrefab = Resources.Load<GameObject>("Prefabs/Enemy 1");
        weaponPrefab = Resources.Load<GameObject>("Prefabs/Sword");

        enemyEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(enemyPrefab, settings);
        weaponEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(weaponPrefab, settings);

        Player.Init();

        for (int i = 0; i < 34000; i++)
        {
            CreateEnemyEntity();
        }
        CurrentWeapon = CreateWeaponEntity();
    }

    /// <summary>
    /// 创建一个武器实体 并为其绑定WeaponTag
    /// </summary>
    private Entity CreateWeaponEntity()
    {
        Entity weapon = entityManager.Instantiate(weaponEntity);
        entityManager.SetName(weapon, "PlayerWeapon");
        entityManager.SetComponentData(weapon, new Translation { Value = Player.weaponPos.position });

        return weapon;
    }

    private Entity CreateEnemyEntity()
    {
        float randX = UnityEngine.Random.Range(0, 1024f);
        float randZ = UnityEngine.Random.Range(0, 1024f);
        Vector3 randomPos = new Vector3(randX, 0, randZ);

        Entity enemy = entityManager.Instantiate(enemyEntity);
        entityManager.SetComponentData(enemy, new Translation() { Value = randomPos });

        return enemy;
    }

    public Entity CreateSkillVfxEntity(string goPrefabName, Transform startPos, Quaternion startQuaterion)
    {
        GameObject vfxPrefab = Resources.Load<GameObject>("Prefabs/" + goPrefabName);
        if (vfxPrefab == null)
        {
            return Entity.Null;
        }

        var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(vfxPrefab, settings);
        var skillVfx = entityManager.Instantiate(entity);
        entityManager.SetName(skillVfx, goPrefabName);
        entityManager.AddComponent(skillVfx, typeof(SkillVFXTag));
        entityManager.AddComponentData(skillVfx, new EntityCollision() { Radius = 1 });
        entityManager.AddComponentData(entity, new DistanceRecord() { Distance = 0 });
        entityManager.SetComponentData(skillVfx, new Translation() { Value = startPos.position });
        entityManager.SetComponentData(skillVfx, new Rotation() { Value = startQuaterion });

        return skillVfx;
    }

    public string GetEntityName(Entity entity)
    {
        return entity == null ? "EntityNull" : entityManager.GetName(entity);
    }

    public Vector3 GetCurrentWeaponPos()
    {
        if (CurrentWeapon == null)
        {
            return Player.transform.position;
        }

        Translation translation = entityManager.GetComponentData<Translation>(CurrentWeapon);
        return translation.Value;
    }

    public void Clear()
    {

    }
}
