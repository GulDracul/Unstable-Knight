using UnityEngine;

[RequireComponent(typeof(PlayerInput), typeof(PlayerPhysics))]
public class PlayerController : MonoBehaviour
{
    public PlayerInput Input { get; private set; }
    public PlayerPhysics Physics { get; private set; }

    private PlayerState currentState;

    public State_Grounded StateGrounded { get; private set; }
    public State_Airborne StateAirborne { get; private set; }
    public State_GroundPound StateGroundPound { get; private set; }
    public float LastFacingDirection { get; private set; } = 1f;

    // NUEVO: Variables para controlar la ballesta
    public float CrossbowCooldownTimer { get; set; } = 0f;
    public bool CanUseCrossbowInAir { get; set; } = true;

    private void Awake()
    {
        Input = GetComponent<PlayerInput>();
        Physics = GetComponent<PlayerPhysics>();

        StateGrounded = new State_Grounded(this, Physics, Input);
        StateAirborne = new State_Airborne(this, Physics, Input);
        // Inicializamos el nuevo estado
        StateGroundPound = new State_GroundPound(this, Physics, Input);
    }

    private void Start()
    {
        ChangeState(StateGrounded);
    }

    private void Update()
    {
        if (Input.MovementInput.x != 0)
        {
            LastFacingDirection = Mathf.Sign(Input.MovementInput.x);
        }

        // NUEVO: Reducir el enfriamiento de la ballesta
        if (CrossbowCooldownTimer > 0)
        {
            CrossbowCooldownTimer -= Time.deltaTime;
        }

        currentState?.HandleInput();
        currentState?.Update();
    }

    private void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }

    public void ChangeState(PlayerState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
}