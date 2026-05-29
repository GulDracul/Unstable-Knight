using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPhysics : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Ajustes Base")]
    public float moveSpeed = 10f;
    public float jumpForce = 15f;
    public float crossbowRecoilForce = 20f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 1. Movimiento Horizontal (Fuerza continua)
    public void Move(Vector2 direction)
    {
        // Conservamos la velocidad en Y nativa (gravedad), solo alteramos X
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    // 2. Salto (Impulso)
    public void Jump()
    {
        // Reseteamos la velocidad en Y antes de saltar para que los saltos sean consistentes
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // 3. Habilidad: Ballesta (Retroceso masivo)
    public void ApplyCrossbowRecoil(Vector2 recoilDirection)
    {
        // Detenemos la inercia actual para que el disparo se sienta contundente
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(recoilDirection * crossbowRecoilForce, ForceMode2D.Impulse);
    }

    // Método simple para detectar si tocamos el suelo
    public bool IsGrounded()
    {
        // Para el Greybox usamos un Raycast simple. Luego puedes cambiarlo por un OverlapBox
        float distance = 1.1f; // Un poco más de la mitad de tu cubo (asumiendo escala 1,1)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distance, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }
}