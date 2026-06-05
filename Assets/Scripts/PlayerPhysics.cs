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
    public float minDropDistanceToBreak = 4f; // Distancia mínima de caída para romper el suelo
    public float shockwaveRadius = 1.5f;      // El tamaño del radio de destrucción
    public float heavyGravityScale = 10f; // Gravedad exagerada para la caída libre
    public float shockwaveForce = 15f;    // Fuerza con la que empuja objetos

    [Header("Peto con Arpón")]
    public float grappleRange = 8f;   // 3. RANGO MÁXIMO DEL GANCHO
    public float hookSpeed = 40f;     // 1. VELOCIDAD DE VIAJE DEL GANCHO
    public float swingForce = 15f;

    [Header("Guanteletes Cinéticos")]
    public float punchForce = 1500f; // Fuerza masiva porque la caja ahora pesa 50
    public float punchRadius = 1.2f; // Tamaño del puño

    [Header("Agua y Yelmo")]
    public float waterGravityScale = 0.5f; // Flotabilidad (caes muy lento)
    public float waterMoveSpeed = 4f;      // Caminar/nadar cuesta más esfuerzo
    public float helmPropulsionForce = 25f;// Impulso tecnológico hacia arriba
    public float bootsSinkForce = 20f;     // Hundimiento rápido
    public float waterJumpForce = 12f; // Un salto ligeramente más pesado que el salto normal

    [Header("Ballesta")]
    public GameObject projectilePrefab;
    public Transform firePoint; // Desde dónde sale la flecha (para que no salga del centro de la barriga)

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

    public void Move(Vector2 direction, bool isAirborne = false)
    {
        if (recoilStunTimer > 0) return;

        if (isAirborne)
        {
            float targetSpeedX = direction.x * moveSpeed;
            // Inercia de gancho vs Salto normal
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
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(recoilDirection * crossbowRecoilForce, ForceMode2D.Impulse);
        recoilStunTimer = 0.2f;

        if (projectilePrefab != null && firePoint != null)
        {
            Vector2 shootDirection = -recoilDirection;

            // LA CORRECCIÓN: Movemos la posición local del FirePoint para que orbite al jugador
            // Multiplicamos la dirección (1) por la distancia que quieres (0.6f)
            firePoint.localPosition = shootDirection * 0.6f;

            // Ahora sí, disparamos desde la nueva posición
            GameObject arrow = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            arrow.GetComponent<Projectile>().Fire(shootDirection);
        }
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
    // Este es el método que llama el State_GroundPound al aterrizar
    public void TriggerShockwave(float dropDistance)
    {
        // Solo rompemos el suelo si caímos desde suficientemente alto
        if (dropDistance >= minDropDistanceToBreak)
        {
            // Buscamos todo lo que esté en el radio del impacto
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, shockwaveRadius);

            foreach (Collider2D hit in hits)
            {
                // Si lo que golpeamos tiene el componente FragileFloor, lo destruimos
                FragileFloor floor = hit.GetComponent<FragileFloor>();
                if (floor != null)
                {
                    floor.Break();
                }
            }
        }
    }

    // Actualiza tu OnDrawGizmosSelected para poder ver la caja de colisión del golpe en Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius); // Onda de choque de Grebas

        // Dibuja el círculo del puñetazo hacia la derecha (solo como referencia visual)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(1f * 0.8f, 0), punchRadius);
        // DIBUJO DEL SENSOR DE AGUA (Un círculo celeste en el centro de tu cubo)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
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

        // 1. CORRECCIÓN: El rayo ahora choca con CUALQUIER obstáculo sólido (Suelo o Madera)
        int capasSolidas = LayerMask.GetMask("Ground", "Madera");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, aimDirection, grappleRange, capasSolidas);

        // Calculamos dónde se detiene visualmente el gancho
        Vector2 targetPoint = hit.collider != null ? hit.point : (Vector2)transform.position + (aimDirection * grappleRange);

        // LA LÓGICA CLAVE: Solo es un "éxito" si chocamos con algo, Y ese algo específicamente es Madera
        bool success = false;
        if (hit.collider != null)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Madera"))
            {
                success = true;
            }
        }

        // 2. VIAJE DE IDA
        while (Vector2.Distance(currentHookPos, targetPoint) > 0.5f)
        {
            currentHookPos = Vector2.MoveTowards(currentHookPos, targetPoint, hookSpeed * Time.deltaTime);
            lr.SetPosition(1, currentHookPos);
            yield return null;
        }
        lr.SetPosition(1, targetPoint);

        // 3. DECISIÓN: ¿Nos anclamos o nos rebotó la piedra?
        if (success)
        {
            currentGrappleJoint = gameObject.AddComponent<DistanceJoint2D>();
            currentGrappleJoint.autoConfigureConnectedAnchor = false;
            currentGrappleJoint.connectedAnchor = targetPoint;
            currentGrappleJoint.autoConfigureDistance = false;
            currentGrappleJoint.distance = Vector2.Distance(transform.position, targetPoint);
            currentGrappleJoint.maxDistanceOnly = true;

            onHookConnected?.Invoke();
        }
        else
        {
            // VIAJE DE VUELTA (Chocó con piedra o con el aire, se devuelve al jugador)
            while (Vector2.Distance(currentHookPos, transform.position) > 0.5f)
            {
                currentHookPos = Vector2.MoveTowards(currentHookPos, transform.position, hookSpeed * 2f * Time.deltaTime);
                lr.SetPosition(1, currentHookPos);
                yield return null;
            }

            lr.positionCount = 0;
            onHookMissed?.Invoke();
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


    // --- MÉTODOS DE LOS GUANTELETES ---

    public void Punch(float facingDir)
    {
        // 1. Calculamos el centro del golpe: un poco hacia adelante del jugador
        Vector2 punchCenter = (Vector2)transform.position + new Vector2(facingDir * 0.8f, 0);

        // 2. Detectamos TODO lo que esté en ese círculo
        Collider2D[] hits = Physics2D.OverlapCircleAll(punchCenter, punchRadius);
        bool hitSomething = false;

        foreach (Collider2D hit in hits)
        {
            Rigidbody2D hitRb = hit.GetComponent<Rigidbody2D>();

            // Si el objeto tiene Rigidbody, no somos nosotros, y no es el techo/suelo estático
            if (hitRb != null && hitRb != rb && hitRb.bodyType != RigidbodyType2D.Static)
            {
                // Matamos su inercia actual para que el golpe sea limpio
                hitRb.linearVelocity = Vector2.zero;

                // Calculamos una dirección diagonal (hacia adelante y un poco hacia arriba)
                Vector2 forceDir = new Vector2(facingDir, 0.5f).normalized;

                // ¡BATEO!
                hitRb.AddForce(forceDir * punchForce, ForceMode2D.Impulse);
                hitSomething = true;
            }
        }

        // Opcional: Si golpeamos algo, aturdimos al jugador un microsegundo por el impacto
        if (hitSomething)
        {
            recoilStunTimer = 0.1f;
        }
    }

    public void MoveInWater(Vector2 direction)
    {
        // Movimiento viscoso: aceleración y velocidad reducidas
        float targetSpeedX = direction.x * waterMoveSpeed;
        float newVelocityX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeedX, 10f * Time.deltaTime);
        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
    }

    public void SetGravity(float newGravity)
    {
        rb.gravityScale = newGravity;
    }

    public void ApplyHelmPropulsion()
    {
        // Frenamos cualquier inercia vertical y disparamos el yelmo hacia arriba
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * helmPropulsionForce, ForceMode2D.Impulse);
    }

    public void ApplyWaterSink()
    {
        // Frenamos y usamos el plomo para hundirnos de golpe
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.down * bootsSinkForce, ForceMode2D.Impulse);
    }
    public void ApplyWaterJump()
    {
        // Matamos la inercia vertical (por si estabas hundiéndote) y aplicamos el impulso
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * waterJumpForce, ForceMode2D.Impulse);
    }
    public bool IsGrounded()
    {
        float distance = 1.1f;

        // CORRECCIÓN: Ahora el rayo busca chocar contra "Ground" O "Madera"
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distance, LayerMask.GetMask("Ground", "Madera"));

        return hit.collider != null;
    }
    public bool IsInWater()
    {
        bool touchingWater = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Agua")) != null;

        if (touchingWater)
        {
            Debug.Log("💦 ¡El sensor físico está tocando la capa Agua!");
        }

        return touchingWater;
    }
}