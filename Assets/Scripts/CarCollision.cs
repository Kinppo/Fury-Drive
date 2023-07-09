using UnityEngine;

public class CarCollision : MonoBehaviour
{
    public float bounceForce = 20f;
    public float bounceForceLimit = 250f;
    public float updateInterval = 0.1f;
    public CarController carController;
    private Vector3 lastPosition;
    private float lastUpdateTime;
    private float currentSpeed;

    private void Start()
    {
        lastPosition = transform.position;
        lastUpdateTime = Time.time;
    }

    private void FixedUpdate()
    {
        var deltaTime = Time.time - lastUpdateTime;
        if (!(deltaTime >= updateInterval)) return;

        var distance = Vector3.Distance(lastPosition, transform.position);
        currentSpeed = distance / deltaTime;
        lastPosition = transform.position;
        lastUpdateTime = Time.time;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 9)
        {
            // var carSpeed = carController.sphereRb.velocity.magnitude;
            var bounceDirection = (transform.position - collision.contacts[0].point).normalized;
            bounceDirection.y = 0;

            var force = currentSpeed * bounceForce;
            force = force > bounceForceLimit ? bounceForceLimit : force;
            carController.sphereRb.AddForce(bounceDirection * force, ForceMode.Impulse);
            
            if(!AudioManager.Instance.soundEffectSource.isPlaying)
                AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.carBump2);
            
            if (Player.Instance.isUnderAttack)
                Player.Instance.attackingFighter.KillFighter();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 14)
        {
            // var carSpeed = carController.sphereRb.velocity.magnitude;
            var bounceDirection = (transform.position - other.ClosestPoint(transform.position)).normalized;
            bounceDirection.y = 0;

            var force = currentSpeed * bounceForce * 50;
            force = force > bounceForceLimit ? bounceForceLimit : force;
            carController.sphereRb.AddForce(bounceDirection * force, ForceMode.Impulse);
            
            if(!AudioManager.Instance.soundEffectSource.isPlaying)
                AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.carBump1);

            if (Player.Instance.isUnderAttack)
                Player.Instance.attackingFighter.KillFighter();
        }
    }
    
}