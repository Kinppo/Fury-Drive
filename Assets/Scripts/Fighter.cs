using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EnemyFighterState
{
    Idle,
    Jumping,
    Attacking,
    Dead,
}

public class Fighter : MonoBehaviour
{
    private static readonly int Blend1 = Animator.StringToHash("Blend 1");
    private static readonly int Blend2 = Animator.StringToHash("Blend 2");
    private float para1, para2;
    public Animator animator;
    public Rigidbody rb;
    public Transform spear;
    public Enemy enemy;
    public EnemyFighterState fighterState;
    public float jumpRange = 5f;
    public LayerMask playerLayer;
    public List<Rigidbody> ragdoll = new List<Rigidbody>();
    private bool isJumping, isReadyForHit;

    private void Update()
    {
        if (isJumping) Jump();

        if (!isJumping && fighterState == EnemyFighterState.Idle && CheckPlayerInRange() &&
            enemy.car.carState == CarState.OnRoad && Player.Instance.attackingFighter == null && !enemy.isDead)
        {
            ChangeAnimation(EnemyFighterState.Jumping);
            transform.parent = Player.Instance.transform;
            Player.Instance.attackingFighter = this;
            Player.Instance.isUnderAttack = true;
            isJumping = true;
        }
    }

    private bool CheckPlayerInRange()
    {
        var colliders = new Collider[50];
        Physics.OverlapSphereNonAlloc(transform.position, jumpRange, colliders, playerLayer);
        return colliders.Any(t => t);
    }

    private void Jump()
    {
        transform.position =
            Vector3.Lerp(transform.position, Player.Instance.enemyLandPoint.position, Time.deltaTime * 8);
        transform.rotation =
            Quaternion.Lerp(transform.rotation, Player.Instance.enemyLandPoint.rotation, Time.deltaTime * 8);

        if (Vector3.Distance(transform.position, Player.Instance.enemyLandPoint.position) < 0.1f)
        {
            isJumping = false;
            ChangeAnimation(EnemyFighterState.Attacking);
            StartCoroutine("UpdateHitReady");
        }
    }

    public void KillFighter()
    {
        if (!isReadyForHit)
            return;

        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.carSquish);
        animator.enabled = false;
        Destroy(spear.gameObject);
        rb.isKinematic = false;
        for (int i = 0; i < ragdoll.Count; i++)
        {
            ragdoll[i].isKinematic = false;
            ragdoll[i].gameObject.GetComponent<Collider>().isTrigger = false;
        }

        rb.AddForce(transform.forward * 10);
        fighterState = EnemyFighterState.Dead;
        Destroy(gameObject, 1);
        Player.Instance.isUnderAttack = false;
        Player.Instance.attackingFighter = null;
    }

    private IEnumerator UpdateHitReady()
    {
        yield return new WaitForSeconds(0.75f);
        isReadyForHit = true;
    }

    public void ChangeAnimation(EnemyFighterState state = EnemyFighterState.Idle)
    {
        if (fighterState == state) return;

        switch (state)
        {
            case EnemyFighterState.Idle:
                para1 = 1;
                para2 = 1;
                break;
            case EnemyFighterState.Jumping:
                para1 = 1;
                para2 = -1;
                break;
            case EnemyFighterState.Attacking:
                para1 = -1;
                para2 = 1;
                StartCoroutine(Player.Instance.ReceiveDamage());
                break;
            case EnemyFighterState.Dead:
                para1 = -1;
                para2 = -1;
                break;
        }

        fighterState = state;
        animator.SetFloat(Blend1, para1);
        animator.SetFloat(Blend2, para2);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, jumpRange);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == 6)
        {
            Destroy(gameObject, 0.1f);
        }
    }
}