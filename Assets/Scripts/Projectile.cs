using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public float speed = 25f;
    public float lifetime = 2f; // Se destruye solo después de 2 segundos para no consumir memoria

    private Rigidbody2D rb;

    public void Fire(Vector2 direction)
    {
        rb = GetComponent<Rigidbody2D>();

        // Le damos el impulso inicial
        rb.linearVelocity = direction.normalized * speed;

        // Magia matemática: Rotamos el virote para que apunte en la dirección de vuelo
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Iniciamos la cuenta regresiva de autodestrucción
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignoramos al propio jugador y al agua (para que puedas disparar a través de ella)
        if (collision.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Agua"))
            return;

        // Más adelante, aquí le diremos al enemigo que reciba daño si la colisión es con uno.
        // Por ahora, el virote se destruye al chocar contra cualquier otra cosa (Suelo, Madera, Cajas).
        Destroy(gameObject);
    }
}