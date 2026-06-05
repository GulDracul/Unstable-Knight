using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    [Header("Condiciones de Ruptura")]
    public float requiredMass = 20f;     // Pedimos 40 por si en el futuro bajas la caja a 50kg
    public float minImpactSpeed = 15f;   // La velocidad mínima de la colisión para que cuente como "cañonazo"

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D hitRb = collision.rigidbody;

        // 1. Verificamos si lo que nos golpeó tiene un componente físico y es suficientemente pesado
        if (hitRb != null && hitRb.mass >= requiredMass)
        {
            // 2. Revisamos con qué fuerza fue el impacto (magnitude nos da la velocidad total del vector)
            if (collision.relativeVelocity.magnitude >= minImpactSpeed)
            {
                // ¡Impacto cinético masivo detectado! 
                // Aquí en el futuro instanciarás escombros metálicos, tuercas o tuberías rotas.
                Destroy(gameObject);
            }
        }
    }
}