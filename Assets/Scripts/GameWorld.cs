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

    public void Init()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);

        enemyPrefab = Resources.Load<GameObject>("Prefabs/Enemy 1");
        weaponPrefab = Resources.Load<GameObject>("Prefabs/Sword");

        enemyEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(enemyPrefab, settings);
        weaponEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(weaponPrefab, settings);

        Player.Init();

        //for (int i = 0; i < 34000; i++)
        //{
        //    CreateEnemyEntity();
        //}
        CurrentWeapon = CreateWeaponEntity();

        /*var sword = CreateWeaponEntity();
        entityManager.SetComponentData(sword, new Translation { Value = new Unity.Mathematics.float3(3, 3, 3) });*/
    }

    /// <summary>
    /// 创建一个武器实体 并为其绑定WeaponTag
    /// </summary>
    private Entity CreateWeaponEntity()
    {
        Entity weapon = entityManager.Instantiate(weaponEntity);
        entityManager.SetComponentData(weapon, new Translation { Value = Player.weaponPos.position });

        return weapon;
    }

    private Entity CreateEnemyEntity()
    {
        float randX = UnityEngine.Random.Range(0, 2048f);
        float randZ = UnityEngine.Random.Range(0, 2048f);
        Vector3 randomPos = new Vector3(randX, 0, randZ);

        Entity enemy = entityManager.Instantiate(enemyEntity);
        entityManager.SetComponentData(enemy, new Translation() { Value = randomPos });

        return enemy;
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
