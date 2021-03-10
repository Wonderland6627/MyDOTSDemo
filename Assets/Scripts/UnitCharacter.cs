/*
 *	Date: 2020-08-15 16:54:39
 *	Description: 单位角色状态控制
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class UnitCharacter : MonoBehaviour
{
    [Header("武器位置")]
    public Transform weaponPos;

    [Header("防御位置")]
    public Transform defencePos;

    public Animator animator;
    public Rigidbody unitRigidbody;
    public CapsuleCollider unitCapsuleCollider;

    #region MovePart
    [Header("在地面上")]
    public bool isGrounded = true;//是否站在地上
    [Header("是否冲刺")]
    public bool isSprinting = false;

    [Header("转向速度")]
    public float moveTurnSpeed = 360;
    [Header("静止转向速度")]
    public float stationaryTurnSpeed = 180;
    [Header("跳跃力")]
    public float jumpPower = 12f;
    [Header("重力系数")]
    public float gravityMultiplier = 2f;
    [Header("m_RunCycleLegOffset")]
    public float runCycleLegOffset = 0.2f;
    [Header("移速系数")]
    public float moveSpeedMultiplier = 1f;
    [Header("动画速度系数")]
    public float animSpeedMultiplier = 1f;
    [Header("地面检测距离")]
    public float groundCheckDistance = 0.1f;
    [Header("下半身动画权重")]
    public float belowAnimWeight = 1;
    [SerializeField] private float defaultGroundCheckDistance;

    protected Vector3 input;
    protected Vector3 inputSmooth;

    protected Vector3 capsuleCenter;
    protected float capsuleHeight;

    protected Vector3 groundNormal;
    protected float turnAmount;
    protected float forwardAmount;

    [HideInInspector] public RaycastHit groundHit;
    public LayerMask groundLayer;
    public float groundMinDistance = 0.25f;
    public float groundMaxDistance = 0.50f;
    public float groundDistance;

    [SerializeField]
    private float stayAirTime = 0;
    #endregion

    #region AttackPart
    public int vfxCount = 0;

    public Transform attackVFXPos;//攻击特效释放位置
    public LayerMask aimLayerMask;

    public float inAttackTime = 0;//在连击中的时间
    public bool inAttackCombo;//是否连击
    public bool heavyAttack;
    public float skillsDuration;//整个连击所需时间，按键时间大于这个 重新开始或者结束连击

    private AnimatorOverrideController overrideController;
    #endregion

    public void Init()
    {
        animator = GetComponent<Animator>();
        unitRigidbody = GetComponent<Rigidbody>();
        unitCapsuleCollider = GetComponent<CapsuleCollider>();

        capsuleCenter = unitCapsuleCollider.center;
        capsuleHeight = unitCapsuleCollider.height;

        defaultGroundCheckDistance = groundCheckDistance;

        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;

        SetupSkillInfos();
    }

    public virtual void MoveCharacter(Vector3 direction, bool jump, bool sprint)
    {
        if (direction.magnitude > 1)
        {
            direction.Normalize();
        }
        direction = transform.InverseTransformDirection(direction);
        CheckGroundStatus();
        direction = Vector3.ProjectOnPlane(direction, groundNormal);
        turnAmount = Mathf.Atan2(direction.x, direction.z);
        forwardAmount = direction.z;
        FixFowardAmount(sprint);

        ApplyExtraTurnRotation();

        if (isGrounded)
        {
            GroundedMovement(jump);
        }
        else
        {
            AirborneMovement();
        }

        UpdateAnimator(direction);
    }

    private void FixFowardAmount(bool sprint)
    {
        isSprinting = sprint;
        if (!sprint)
        {
            forwardAmount = forwardAmount > 0 ? Mathf.Clamp(forwardAmount, 0, 0.5f) : Mathf.Clamp(forwardAmount, -0.5f, 0);
        }
    }

    private void UpdateAnimator(Vector3 direction)
    {
        animator.SetFloat(AnimatorParams.Foward, forwardAmount, 0.05f, Time.deltaTime);
        animator.SetFloat(AnimatorParams.Turn, turnAmount, 0.1f, Time.deltaTime);
        animator.SetBool(AnimatorParams.OnGround, isGrounded);
        if (!isGrounded)
        {
            animator.SetFloat(AnimatorParams.Jump, unitRigidbody.velocity.y);
        }

        /*float runCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + runCycleLegOffset, 1);
        float jumpLeg = (runCycle < 0.5f ? 1 : -1) * forwardAmount;
        if (isGrounded)
        {
            animator.SetFloat(AnimatorParams.JumpLeg, jumpLeg);
        }*/

        if (isGrounded && direction.magnitude > 0)
        {
            animator.speed = animSpeedMultiplier;
        }
        else
        {
            animator.speed = 1;
        }
    }

    public void UpdateAnimator(bool inAttackCombo, bool heavyAttack)
    {
        animator.SetBool(AnimatorParams.HeavyAttack, heavyAttack);

        animator.SetBool(AnimatorParams.InAttackCombo, inAttackCombo);
    }

    private void GroundedMovement(bool jump)
    {
        if (jump)
        {
            unitRigidbody.velocity = new Vector3(unitRigidbody.velocity.x, jumpPower, unitRigidbody.velocity.z);
            isGrounded = false;
            animator.applyRootMotion = false;
            groundCheckDistance = defaultGroundCheckDistance;
        }
    }

    private void AirborneMovement()
    {
        Vector3 extraGravityForce = (Physics.gravity * gravityMultiplier) - Physics.gravity;
        unitRigidbody.AddForce(extraGravityForce);

        groundCheckDistance = unitRigidbody.velocity.y < 0 ? defaultGroundCheckDistance : 0.01f;
    }

    private void ApplyExtraTurnRotation()
    {
        float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, moveTurnSpeed, forwardAmount);
        transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
    }

    private void CheckGroundStatus()
    {
        /*if (Physics.Raycast(transform.position + Vector3.up * defaultGroundCheckDistance, Vector3.down, out RaycastHit hit, groundCheckDistance))
        {
            groundNormal = hit.normal;
            isGrounded = true;
            animator.applyRootMotion = true;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
            animator.applyRootMotion = false;
        }*/

        CheckGroundDistance(groundHit);

        if (groundDistance < groundMinDistance)
        {
            stayAirTime = 0;
            groundNormal = groundHit.normal;
            isGrounded = true;
            animator.applyRootMotion = true;
        }
        else if (groundDistance >= groundMaxDistance)
        {
            stayAirTime += Time.deltaTime;
            if (stayAirTime > 0.5f)
            {
                isGrounded = false;
                animator.applyRootMotion = false;
            }
            groundNormal = Vector3.up;
        }
    }

    private void CheckGroundDistance(RaycastHit hit)
    {
        float radius = unitCapsuleCollider.radius * 0.9f;
        var dist = 10f;
        Ray ray2 = new Ray(transform.position + new Vector3(0, capsuleHeight / 2, 0), Vector3.down);
        RaycastHit groundHit;
        if (Physics.Raycast(ray2, out groundHit, (capsuleHeight / 2) + dist, groundLayer) && !groundHit.collider.isTrigger)
        {
            dist = transform.position.y - groundHit.point.y;
            hit = groundHit;
        }

        if (dist >= groundMinDistance)
        {
            Vector3 pos = transform.position + Vector3.up * unitCapsuleCollider.radius;
            Ray ray = new Ray(pos, -Vector3.up);
            if (Physics.SphereCast(ray, radius, out groundHit, unitCapsuleCollider.radius + groundMaxDistance, groundLayer) && !groundHit.collider.isTrigger)
            {
                Physics.Linecast(groundHit.point + (Vector3.up * 0.1f), groundHit.point + Vector3.down * 0.15f, out groundHit, groundLayer);
                float newDist = transform.position.y - groundHit.point.y;
                hit = groundHit;
                if (dist > newDist)
                {
                    dist = newDist;
                }
            }
        }

        groundDistance = (float)Math.Round(dist, 2);
    }

    public void OnAnimatorMove()
    {
        if (isGrounded)
        {
            Vector3 v = (animator.deltaPosition * moveSpeedMultiplier) / Time.deltaTime;
            v.y = unitRigidbody.velocity.y;
            unitRigidbody.velocity = v;
        }
    }

    public void ComboAttack(bool attack, bool heavyAttack)
    {
        if (attack && inAttackTime < skillsDuration)
        {
            inAttackCombo = true;
            inAttackTime += Time.deltaTime;
        }
        else
        {
            inAttackTime = 0;
            inAttackCombo = false;
        }

        this.heavyAttack = heavyAttack;
        UpdateAnimator(inAttackCombo, heavyAttack);
    }

    /// <summary>
    /// 计算连招总时间
    /// </summary>
    private void SetupSkillInfos()
    {
        inAttackCombo = false;
        skillsDuration = 0;
        skillsDuration += overrideController[AnimatorParams.AttackClip01].length;
        skillsDuration += overrideController[AnimatorParams.AttackClip02].length;
        skillsDuration += overrideController[AnimatorParams.AttackClip03].length;
    }

    #region SkillVFX

    private void ReleaseSkill()
    {
        ReleaseSkillVFX("Attack_0");
    }

    private void ReleaseSkillVFX(string vfxGoName)
    {
        GameWorld.GetInstance().CreateSkillVfxEntities(vfxCount, vfxGoName, attackVFXPos.position, transform.rotation);

        //GameWorld.GetInstance().CreateSkillVfxEntity(vfxGoName, attackVFXPos, transform.rotation);
    }

    private void SetWeaponState(string tof)
    {
        bool isAttacking = true ? tof == "T" : false;
        GameWorld.GetInstance().SetCurrentWeaponAttackState(isAttacking);
    }

    #endregion
}

public static partial class AnimatorParams
{
    public static int Foward = Animator.StringToHash("Forward");
    public static int Turn = Animator.StringToHash("Turn");
    public static int OnGround = Animator.StringToHash("OnGround");
    public static int Jump = Animator.StringToHash("Jump");
    public static int JumpLeg = Animator.StringToHash("JumpLeg");

    public static int HeavyAttack = Animator.StringToHash("HeavyAttack");
    public static int InAttackCombo = Animator.StringToHash("InAttackCombo");

    public const int BaseLayer = 0;
    public const int UpperBodyLayer = 1;
    public const int BelowBodyLayer = 2;

    public static string AttackClip01 = "Attack_01";
    public static string AttackClip02 = "Attack_02";
    public static string AttackClip03 = "Attack_03";
    public static string HeavyAttackClip = "HeavyAttack";
}