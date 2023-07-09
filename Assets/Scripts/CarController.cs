using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car Controller")] public float moveSpeed;
    public float rotateSpeed;
    public float airDrag;
    public float gravity;
    public float groundDistance;
    public float alignToGroundTime;
    public LayerMask groundLayer;
    public Rigidbody sphereRb;
    public Rigidbody carRb;
    public Car car;
    public WheelController wheelController;

    private FloatingJoystick _joystick;
    private float _moveInput;
    private float _rotateInput;
    private float _groundDrag;
    private bool _isCarGrounded;
    public bool isAuto;

    private void Start()
    {
        _joystick = UIManager.Instance.joystick;
        sphereRb.transform.parent = null;
        carRb.transform.parent = null;
        _groundDrag = sphereRb.drag;
    }

    private void Update()
    {
        transform.position = sphereRb.transform.position;

        if (car.carState != CarState.OnRoad) return;
        if (!isAuto)
        {
            //JoystickControl();
            KeyboardControl();
            //SwipeControl();
        }

        var newRot = _rotateInput * rotateSpeed * Time.deltaTime * _moveInput;

        if (_isCarGrounded)
            transform.Rotate(0, newRot, 0, Space.World);

        transform.position = sphereRb.transform.position;

        RaycastHit hit;
        _isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, groundDistance, groundLayer);

        var toRotateTo = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotateTo, alignToGroundTime * Time.deltaTime);

        sphereRb.drag = _isCarGrounded ? _groundDrag : airDrag;

        wheelController.RotateWheels(_moveInput, _rotateInput);
    }

    private void FixedUpdate()
    {
        if (car.carState != CarState.OnRoad) return;

        if (_isCarGrounded)
            sphereRb.AddForce(transform.forward * _moveInput * moveSpeed, ForceMode.Acceleration);
        else
            sphereRb.AddForce(transform.up * -gravity);

        carRb.MoveRotation(transform.rotation);
    }

    public void SetInputs(float move, float rotate)
    {
        _moveInput = move;
        _rotateInput = rotate;

        if (isAuto && GameManager.gameState != GameState.Play) return;

        AudioManager.Instance.ChangeEngineVolume(move == 0);
    }

    private void JoystickControl()
    {
        if (_joystick.Horizontal != 0 && _joystick.Vertical == 0)
            SetInputs(1, _joystick.Horizontal);
        else
            SetInputs(_joystick.Vertical, _joystick.Horizontal);
    }

    private void KeyboardControl()
    {
        SetInputs(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
    }

    #region Swipe Controll

    private Vector2 _startPosition;
    private float _verticalAxis;
    private float _horizontalAxis;
    private float _currentRotation;
    private float _swipeThreshold = 100f;
    private float _rotationSpeed = 75f;
    private float _rotationLerpSpeed = 35f;

    private void SwipeControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _verticalAxis = 1;
            _startPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _verticalAxis = 0;
            _horizontalAxis = 0;
        }

        if (Input.GetMouseButton(0))
        {
            var swipeDelta = (Vector2) Input.mousePosition - _startPosition;
            if (swipeDelta.magnitude >= _swipeThreshold)
            {
                var swipeDirection = Mathf.Sign(swipeDelta.x);
                var targetRotation = swipeDirection * _rotationSpeed * Time.deltaTime;
                _currentRotation = Mathf.Lerp(_currentRotation, targetRotation, _rotationLerpSpeed * Time.deltaTime);
                _horizontalAxis = Mathf.Clamp(_currentRotation, -1, 1);
            }
            else
                _horizontalAxis = 0;
        }

        SetInputs(_verticalAxis, _horizontalAxis);
    }

    #endregion


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 16)
            sphereRb.AddForce(transform.forward * _moveInput * moveSpeed * Random.Range(6f, 12f)
                , ForceMode.Acceleration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, -transform.up * 1.8f);
    }
}