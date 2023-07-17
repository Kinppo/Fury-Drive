using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PathCreation;
using UnityEngine;

public enum PlayerState
{
    Idle,
    IdleToHide,
    Hide,
    HideToIdle,
}

public class Player : MonoBehaviour
{
    public static Player Instance { get; protected set; }
    private static readonly int Blend1 = Animator.StringToHash("Blend 1");
    private static readonly int Blend2 = Animator.StringToHash("Blend 2");
    private float para1, para2, progress;
    private bool isAnimating;
    private Transform target;
    public PlayerState fighterState;
    public Animator fighterAnimator;
    public Transform fighter;
    public LayerMask enemiesLayer;
    public Rocket rocket;
    public Transform shootPoint;
    public ParticleSystem bazookaLaunchEffect;
    public DOTweenAnimation bazookaAnimation;
    public Transform enemyLandPoint;
    public Car car;
    public bool isUnderAttack;

    [Header("Fire Settings")] public int health = 5;
    public float shootRange = 50;
    public float reloadTime = 3;
    public float damage = 1;

    [Header("Car Crash Animation")] public float crashUpMovement = 12f;
    public float crashRotationAngle = 170f;
    public float crashMovementSpeed = 24f;
    public float crashRotationSpeed = 100f;
    public float yOffset = 3f;
    public AnimationCurve movementCurve;
    public AnimationCurve rotationCurve;

    private bool _isMovingUpward = true;
    private bool _isRotating = true;
    private bool _isMovingToGround;
    private Vector3 _initialCrashPosition;
    private Vector3 _crashTargetPosition;
    private float _movementTime;
    private float _rotationTime;
    private float _backTime;

    [HideInInspector] public Fighter attackingFighter;
    [HideInInspector] public bool isDead;
    private bool isFiring;
    private bool isReloading = true;
    private bool isRotating;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (attackingFighter != null && car.carState == CarState.Finished)
            attackingFighter.KillFighter();

        if (car.carState != CarState.OnRoad) return;
        if (isDead)
        {
            CrashCar();
            return;
        }

        if (isAnimating) SmoothAnimation();
        if (!isReloading) LookAt();
        if (!isRotating && !isReloading && isFiring) Shoot();
    }

    private void LookAt()
    {
        if (CheckEnemiesInRange().Count != 0)
        {
            var targetPoint = GetClosestEnemiesInRange();
            var relativePos = targetPoint - fighter.position;
            relativePos.y = 0;
            var targetRotation = Quaternion.LookRotation(relativePos, Vector3.up);
            SmoothRotation(targetRotation, true);
        }
        else if (Mathf.Abs(fighter.rotation.eulerAngles.y) > 7)
        {
            SmoothRotation(transform.rotation, false);
        }
    }

    private void SmoothRotation(Quaternion targetRotation, bool reset)
    {
        if (Mathf.Abs(Quaternion.Dot(fighter.rotation, targetRotation)) < 0.9999f)
        {
            isRotating = true;
            fighter.rotation =
                Quaternion.Slerp(fighter.rotation, targetRotation, Time.deltaTime * 5);
        }
        else
        {
            isRotating = false;
            if (reset) isFiring = true;
        }
    }

    private List<Transform> CheckEnemiesInRange()
    {
        var allEnemies = new Collider[50];
        var enemies = new List<Transform>();
        Physics.OverlapSphereNonAlloc(transform.position, shootRange, allEnemies, enemiesLayer);
        for (int i = 0; i < allEnemies.Length; i++)
            if (allEnemies[i] && IsTargetInFront(allEnemies[i].transform))
                enemies.Add(allEnemies[i].transform);

        return enemies;
    }

    private bool IsTargetInFront(Transform target)
    {
        var direction = transform.position - target.position;
        var localDirection = target.InverseTransformDirection(direction);
        return localDirection.z < 1f;
    }

    private Vector3 GetClosestEnemiesInRange()
    {
        var enemies = CheckEnemiesInRange();
        if (enemies.Count == 0) return Vector3.zero;
        var minDist = Mathf.Infinity;
        var closestEnemy = enemies[0];
        for (int i = 0; i < enemies.Count; i++)
        {
            var dist = Vector3.Distance(transform.position, enemies[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                closestEnemy = enemies[i];
            }
        }

        target = closestEnemy;
        return new Vector3(closestEnemy.position.x, 1f, closestEnemy.position.z);
    }

    private void Shoot()
    {
        print("shoot");
        var targetCar = target.GetComponent<Enemy>();
        var aiController = targetCar.aICarController;

        if (targetCar.car.carState != CarState.OnRoad)
            return;

        float time;
        var hitPoint = GetHitPoint(targetCar.transform.position + new Vector3(0, 1, 0), aiController.currentSpeed,
            transform.position,
            rocket.speed, out time);
        var aim = hitPoint - transform.position;
        aim.y = 0;

        var antiGravity = -Physics.gravity.y * time / 2;
        var deltaY = (hitPoint.y - shootPoint.position.y) / time;

        var rocketSpeed = aim.normalized * rocket.speed;
        rocketSpeed.y = antiGravity + deltaY;

        var rocketRotation = Quaternion.LookRotation(hitPoint - shootPoint.position);
        var instance = Instantiate(rocket, shootPoint.position, rocketRotation);
        instance.LaunchRocket(rocketSpeed, damage);

        bazookaLaunchEffect.gameObject.SetActive(true);
        bazookaLaunchEffect.Play();
        bazookaAnimation.DOPlay();
        bazookaAnimation.DORestart();
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.rpgLaunch);

        isFiring = false;
        isReloading = true;
        StartCoroutine("DelayAnimation", PlayerState.Hide);
        StartCoroutine("EndReloadTime");
    }

    private Vector3 GetHitPoint(Vector3 targetPosition, Vector3 targetSpeed, Vector3 attackerPosition,
        float bulletSpeed,
        out float time)
    {
        Vector3 q = targetPosition - attackerPosition;
        //Ignoring Y for now. Add gravity compensation later, for more simple formula and clean game design around it
        q.y = 0;
        targetSpeed.y = 0;

        //solving quadratic ecuation from t*t(Vx*Vx + Vy*Vy - S*S) + 2*t*(Vx*Qx)(Vy*Qy) + Qx*Qx + Qy*Qy = 0

        float a = Vector3.Dot(targetSpeed, targetSpeed) -
                  (bulletSpeed *
                   bulletSpeed); //Dot is basicly (targetSpeed.x * targetSpeed.x) + (targetSpeed.y * targetSpeed.y)
        float b = 2 * Vector3.Dot(targetSpeed, q); //Dot is basicly (targetSpeed.x * q.x) + (targetSpeed.y * q.y)
        float c = Vector3.Dot(q, q); //Dot is basicly (q.x * q.x) + (q.y * q.y)

        //Discriminant
        float D = Mathf.Sqrt((b * b) - 4 * a * c);

        float t1 = (-b + D) / (2 * a);
        float t2 = (-b - D) / (2 * a);

        time = Mathf.Max(t1, t2);

        Vector3 ret = targetPosition + targetSpeed * time;
        return ret;
    }

    private IEnumerator EndReloadTime()
    {
        yield return new WaitForSeconds(reloadTime);
        ChangeAnimation(PlayerState.HideToIdle);
        StartCoroutine("ResetReloadTime");
    }

    private IEnumerator DelayAnimation(PlayerState state)
    {
        yield return new WaitForSeconds(0.3f);
        ChangeAnimation(state);
    }

    private IEnumerator ResetReloadTime()
    {
        yield return new WaitForSeconds(1);
        isReloading = false;
    }

    public IEnumerator ReceiveDamage()
    {
        yield return new WaitForSeconds(1.5f);

        if (!isUnderAttack) yield break;
        UIManager.Instance.UpdateHealthSlider();
        if (!AudioManager.Instance.soundEffectSource.isPlaying)
            AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.swordHit);

        yield return new WaitForSeconds(1.5f);
        StartCoroutine("ReceiveDamage");
    }

    public void ChangeAnimation(PlayerState state = PlayerState.Idle)
    {
        if (fighterState == state) return;

        switch (state)
        {
            case PlayerState.Idle:
                para1 = 1;
                para2 = 1;
                break;
            case PlayerState.IdleToHide:
                para1 = 1;
                para2 = -1;
                break;
            case PlayerState.Hide:
                para1 = -1;
                para2 = 1;
                break;
            case PlayerState.HideToIdle:
                para1 = -1;
                para2 = -1;
                break;
        }

        isAnimating = true;
        fighterState = state;
    }

    private void SmoothAnimation()
    {
        var blend1 = Mathf.Lerp(fighterAnimator.GetFloat(Blend1), para1, 10 * Time.deltaTime);
        var blend2 = Mathf.Lerp(fighterAnimator.GetFloat(Blend2), para2, 10 * Time.deltaTime);

        fighterAnimator.SetFloat(Blend1, blend1);
        fighterAnimator.SetFloat(Blend2, blend2);

        if ((blend1 - para1 == 0) && (blend2 - para2 == 0))
            isAnimating = false;
    }

    public void InitializeCrash()
    {
        GetComponent<CarController>().enabled = false;
        var wheelController = GetComponent<WheelController>();
        wheelController.trails.SetActive(false);
        _initialCrashPosition = transform.position + new Vector3(0, yOffset, 0);
        _crashTargetPosition = transform.position + Vector3.up * crashUpMovement;
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.explosion);
    }

    private void CrashCar()
    {
        if (_isMovingUpward)
        {
            var speedMultiplier = movementCurve.Evaluate(_movementTime);
            var currentMovementSpeed = speedMultiplier * crashMovementSpeed;
            transform.position = Vector3.MoveTowards(transform.position, _crashTargetPosition,
                currentMovementSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _crashTargetPosition) <= 0.2f)
            {
                _isMovingUpward = false;
                _movementTime = 0f;
            }

            _movementTime += Time.deltaTime;
        }

        if (_isRotating)
        {
            var speedMultiplier = rotationCurve.Evaluate(_rotationTime);
            var currentMovementSpeed = speedMultiplier * crashRotationSpeed;
            var targetRotation = Quaternion.Euler(crashRotationAngle, 0f, 0f);
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, targetRotation, currentMovementSpeed * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRotation) <= 0.01f)
            {
                _isRotating = false;
                _rotationTime = 0f;
                _isMovingToGround = true;
            }

            _rotationTime += Time.deltaTime;
        }

        if (_isMovingToGround)
        {
            var speedMultiplier = rotationCurve.Evaluate(_backTime);
            var currentMovementSpeed = speedMultiplier * crashMovementSpeed;
            transform.position = Vector3.MoveTowards(transform.position, _initialCrashPosition,
                currentMovementSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _initialCrashPosition) <= 0.2f)
            {
                _backTime = 0f;
                _isMovingToGround = false;
                Destroy(gameObject, 0.7f);
            }

            _backTime += Time.deltaTime;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}