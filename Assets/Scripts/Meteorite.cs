using UnityEngine;

public class Meteorite : MonoBehaviour
{
    [Header("Настройки прочности")]
    [SerializeField] private int maxHealth = 2;
    private int currentHealth;

    [Header("Настройки движения")]
    [SerializeField] private float baseSpeed = 4f;
    private float speed;

    [Header("Вращение (по оси Z)")]
    [SerializeField] private float rotationSpeed = 150f;

    void Start()
    {
        currentHealth = maxHealth;
        speed = baseSpeed;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);

        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        if (transform.position.x < -12)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage()
    {
        currentHealth--;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Die();
            }
            Destroy(gameObject);
        }
    }
}