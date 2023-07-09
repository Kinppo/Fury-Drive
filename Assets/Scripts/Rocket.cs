using System.Collections;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public float speed = 120;
    public float damage = 1;
    public Rigidbody rb;
    public ParticleSystem hitEffect;

    public void LaunchRocket(Vector3 velocity, float dmg)
    {
        damage = dmg;
        rb.velocity = velocity;
    }

    private void Start()
    {
        StartCoroutine("DestroyRocket");
    }

    private void PlayHitEffect(Transform parent)
    {
        var instance = Instantiate(hitEffect, transform.position, transform.rotation);
        instance.transform.parent = parent;
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.rocketHit);
    }

    private IEnumerator DestroyRocket()
    {
        yield return new WaitForSeconds(6);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 14)
        {
            var enemy = other.gameObject.GetComponent<Enemy>();
            enemy.health -= damage;
            PlayHitEffect(enemy.transform);
            Destroy(gameObject);
        }

        if (other.gameObject.layer == 6)
        {
            PlayHitEffect(null);
            Destroy(gameObject);
        }
    }
}

// 1- Levels
// 2- Swipe controll
// 3- Crosshair & Zoom
