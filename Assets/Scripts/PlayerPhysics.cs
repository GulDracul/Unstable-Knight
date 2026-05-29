using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPhysics : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Ajustes Base")]
    public float moveSpeed = 10f;
    public float jumpForce = 15f;
    public float crossbowRecoilForce = 20f;

    // Temporizador para solucionar el Bug 2
    private float recoilStunTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

    public bool IsGrounded()
    {
        float distance = 1.1f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distance, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }
}