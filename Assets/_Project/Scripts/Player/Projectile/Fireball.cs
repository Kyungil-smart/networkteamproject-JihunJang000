using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float damage = 10f;

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponent<IDamageable>();
        
        if (target != null)
        {
            target.TakeDamage(damage); 
            Destroy(gameObject); 
        }
    }
}