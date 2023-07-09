using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 3f;

    [Header("Car Crash Animation")] public float crashUpMovement = 12f;
    public float crashRotationAngle = 170f;
    public float crashMovementSpeed = 24f;
    public float crashRotationSpeed = 100f;
    public float yOffset = 3f;
    public AnimationCurve movementCurve;
    public AnimationCurve rotationCurve;
    public Car car;
    public AICarController aICarController;
    public WheelController wheelController;

    private bool _isMovingUpward = true;
    private bool _isRotating = true;
    private bool _isMovingToGround;
    private Vector3 _initialCrashPosition;
    private Vector3 _crashTargetPosition;
    private float _movementTime;
    private float _rotationTime;
    private float _backTime;
    [HideInInspector] public bool isDead;

    private void Update()
    {
        if (car.carState != CarState.OnRoad) return;
        if (isDead)
        {
            CrashCar();
            return;
        }
        
        aICarController.MoveCar();
        wheelController.RotateWheels(1, 0);
        CheckCarHealth();
    }

    private void CheckCarHealth()
    {
        if (health == 0 && !isDead)
        {
            GetComponent<AICarController>().enabled = false;
            var wheelController = GetComponent<WheelController>();
            wheelController.trails.SetActive(false);
            _initialCrashPosition = transform.position + new Vector3(0, yOffset, 0);
            _crashTargetPosition = transform.position + Vector3.up * crashUpMovement;
            isDead = true;
            if (Player.Instance.attackingFighter != null)
                Player.Instance.attackingFighter.KillFighter();
            AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.explosion);
            GameManager.Instance.killedEnemies++;
        }
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
}