using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPhysics : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Ajustes Base")]
    public float moveSpeed = 10f;
    public float jumpForce = 15f;
    public float crossbowRecoilForce = 20f;
    public float normalGravityScale = 3f; // BUG 1 SOLUCIONADO: Fija esto en el Inspector
    private float recoilStunTimer = 0f;
    private float defaultGravity;

    [Header("Grebas de Plomo")]
    public float heavyGravityScale = 10f; // Gravedad exagerada para la caída libre
    public float shockwaveRadius = 3f;    // Área de efecto de la explosión
    public float shockwaveForce = 15f;    // Fuerza con la que empuja objetos

    [Header("Peto con Arpón")]
    public float grappleRange = 8f;   // 3. RANGO MÁXIMO DEL GANCHO
    public float hookSpeed = 40f;     // 1. VELOCIDAD DE VIAJE DEL GANCHO
    public float swingForce = 15f;

    private DistanceJoint2D currentGrappleJoint;
    private LineRenderer lr; // Para dibujar la cuerda


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // BUG 3 SOLUCIONADO: Evita atravesar paredes a alta velocidad
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.startWidth = 0.08f;
        lr.endWidth = 0.08f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
    }

    private void Update()
    {
        // Reducimos el temporizador cada frame
        if (recoilStunTimer > 0)
        {
            recoilStunTimer -= Time.deltaTime;
        }
        // Hacemos que el inicio de la cuerda siempre siga al jugador visualmente
        if (lr.positionCount == 2)
        {
            lr.SetPosition(0, transform.position);
        }
    }

    // Añadimos un booleano para saber si estamos en el aire
    public void Move(Vector2 direction, bool isAirborne = false)
    {
        if (recoilStunTimer > 0) return;

        // LA SOLUCIÓN: Si estamos intentando caminar hacia una pared, cancelamos nuestra intención
        // de forzar la velocidad horizontal en esa dirección.
        if (IsHittingWall(direction.x))
        {
            direction.x = 0;
        }

        if (isAirborne)
        {
            float targetSpeedX = direction.x * moveSpeed;
            float airAcceleration = Mathf.Abs(rb.linearVelocity.x) > moveSpeed ? 15f : 100f;
            float newVelocityX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeedX, airAcceleration * Time.deltaTime);
            rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        }
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
        // Usamos la variable pública en lugar de la memoria caché
        rb.gravityScale = normalGravityScale;
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

    public void TryShootGrapple(System.Action onHookConnected, System.Action onHookMissed)
    {
        // Iniciamos el disparo SIEMPRE, sin importar si hay techo o no
        StartCoroutine(ThrowHookRoutine(onHookConnected, onHookMissed));
    }
    private IEnumerator ThrowHookRoutine(System.Action onHookConnected, System.Action onHookMissed)
    {
        lr.positionCount = 2;
        Vector2 currentHookPos = transform.position;
        Vector2 aimDirection = Vector2.up;

        // 1. Calculamos hasta dónde va a llegar el gancho
        RaycastHit2D hit = Physics2D.Raycast(transform.position, aimDirection, grappleRange, LayerMask.GetMask("Madera"));

        // Si pegamos en madera, el objetivo es el impacto. Si no, el objetivo es el rango máximo en el aire.
        Vector2 targetPoint = hit.collider != null ? hit.point : (Vector2)transform.position + (aimDirection * grappleRange);
        bool success = hit.collider != null;

        // 2. VIAJE DE IDA (El gancho sale disparado)
        while (Vector2.Distance(currentHookPos, targetPoint) > 0.5f)
        {
            currentHookPos = Vector2.MoveTowards(currentHookPos, targetPoint, hookSpeed * Time.deltaTime);
            lr.SetPosition(1, currentHookPos);
            yield return null;
        }
        lr.SetPosition(1, targetPoint);

        // 3. BUG 2 SOLUCIONADO: Decidimos si anclamos o recogemos cuerda
        if (success)
        {
            currentGrappleJoint = gameObject.AddComponent<DistanceJoint2D>();
            currentGrappleJoint.autoConfigureConnectedAnchor = false;
            currentGrappleJoint.connectedAnchor = targetPoint;
            currentGrappleJoint.autoConfigureDistance = false;
            currentGrappleJoint.distance = Vector2.Distance(transform.position, targetPoint);

            // EL CAMBIO QUE EVITA ATRAVESAR PAREDES:
            // true = Actúa como una cuerda flexible. Si chocas contra una pared, la cuerda se "destensa" y puedes rebotar sin atravesarla.
            currentGrappleJoint.maxDistanceOnly = true;

            onHookConnected?.Invoke();
        }
        else
        {
            // VIAJE DE VUELTA (El gancho no encontró nada y se devuelve)
            while (Vector2.Distance(currentHookPos, transform.position) > 0.5f)
            {
                currentHookPos = Vector2.MoveTowards(currentHookPos, transform.position, hookSpeed * 2f * Time.deltaTime);
                lr.SetPosition(1, currentHookPos);
                yield return null;
            }

            lr.positionCount = 0; // Ocultamos la cuerda
            onHookMissed?.Invoke(); // Avisamos que falló para volver a caer
        }
    }
    public void DetachGrapple()
    {
        if (currentGrappleJoint != null)
        {
            Destroy(currentGrappleJoint);
        }
        lr.positionCount = 0; // Ocultamos la cuerda
    }

    public void ApplySwingForce(float directionX)
    {
        if (currentGrappleJoint != null && directionX != 0)
        {
            rb.AddForce(Vector2.right * directionX * swingForce, ForceMode2D.Force);
        }
    }

    public bool IsGrounded()
    {
        float distance = 1.1f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distance, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }
    // Método para detectar paredes a los lados
    public bool IsHittingWall(float directionX)
    {
        // Si no pulsas nada, leemos hacia dónde te está llevando la inercia del gancho
        float checkDir = directionX != 0 ? directionX : rb.velocity.x;

        // Si estamos casi quietos, no necesitamos revisar paredes
        if (Mathf.Abs(checkDir) < 0.1f) return false;

        float distance = 0.6f;
        Vector2 checkDirection = checkDir > 0 ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, checkDirection, distance, LayerMask.GetMask("Ground"));

        // DIBUJO VISIBLE (Recuerda mirar la pestaña SCENE o activar "Gizmos" en Game)
        if (hit.collider != null)
        {
            Debug.DrawRay(transform.position, checkDirection * distance, Color.green);
        }
        else
        {
            Debug.DrawRay(transform.position, checkDirection * distance, Color.red);
        }

        return hit.collider != null;
    }
}