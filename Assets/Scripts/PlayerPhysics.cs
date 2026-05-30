using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPhysics : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Ajustes Base")]
    public float moveSpeed = 10f;
    public float jumpForce = 15f;
    public float crossbowRecoilForce = 20f;

    [Header("Grebas de Plomo")]
    public float heavyGravityScale = 10f; // Gravedad exagerada para la caída libre
    public float shockwaveRadius = 3f;    // Área de efecto de la explosión
    public float shockwaveForce = 15f;    // Fuerza con la que empuja objetos

    [Header("Peto con Arpón")]
    public float grappleRange = 10f; // Longitud máxima de la cadena
    public float swingForce = 15f;   // Fuerza para balancearte con el mando


    private float defaultGravity;
    // Temporizador para solucionar el Bug 2
    private float recoilStunTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Guardamos la gravedad que le hayas puesto en el Inspector
        defaultGravity = rb.gravityScale;
    }

    private void Update()
    {
        // Reducimos el temporizador cada frame
        if (recoilStunTimer > 0)
        {
            recoilStunTimer -= Time.deltaTime;
        }
    }

    // 1. Movimiento Horizontal (Fuerza continua)
    public void Move(Vector2 direction)
    {
        // SOLUCIÓN BUG 2: Si estamos aturdidos por el retroceso, cancelamos el movimiento
        // para no sobrescribir el impulso de la ballesta.
        if (recoilStunTimer > 0) return;

        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    // 2. Salto (Impulso)
    public void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // 3. Habilidad: Ballesta (Retroceso masivo)
    public void ApplyCrossbowRecoil(Vector2 recoilDirection)
    {
        // Detenemos TODA la inercia actual (X e Y). 
        // Al estar a cero, la fuerza aplicada siempre moverá al cubo exactamente la misma distancia.
        rb.linearVelocity = Vector2.zero;

        rb.AddForce(recoilDirection * crossbowRecoilForce, ForceMode2D.Impulse);

        recoilStunTimer = 0.2f;
    }
    // --- MÉTODOS DE LAS GREBAS ---

    public void StartHeavyDrop()
    {
        // Frenamos en seco horizontal y verticalmente antes de empezar a caer a plomo
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = heavyGravityScale;
    }

    public void StopHeavyDrop()
    {
        // Restauramos la gravedad normal al tocar el suelo
        rb.gravityScale = defaultGravity;
    }

    public void GenerateShockwave()
    {
        // 1. Dibujamos un círculo imaginario y detectamos todos los colliders dentro
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, shockwaveRadius);

        foreach (Collider2D hit in colliders)
        {
            Rigidbody2D hitRb = hit.GetComponent<Rigidbody2D>();

            // 2. Si tiene Rigidbody y NO somos nosotros mismos, lo hacemos saltar
            if (hitRb != null && hitRb != rb)
            {
                // Lo empujamos hacia arriba (puedes añadir fuerzas diagonales si prefieres)
                hitRb.linearVelocity = new Vector2(hitRb.linearVelocity.x, 0f); // Matamos su inercia actual en Y
                hitRb.AddForce(Vector2.up * shockwaveForce, ForceMode2D.Impulse);
            }
        }

        Debug.Log("¡Onda de choque generada!");
    }

    // Opcional: Dibuja un círculo rojo en la escena de Unity para ver el tamaño de tu onda de choque
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);
    }

    public bool IsGrounded()
    {
        float distance = 1.1f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distance, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }
}