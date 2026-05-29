using UnityEngine;

[RequireComponent(typeof(PlayerInput), typeof(PlayerPhysics))]
public class PlayerController : MonoBehaviour
{
    public PlayerInput Input { get; private set; }
    public PlayerPhysics Physics { get; private set; }

    private PlayerState currentState;

    // Instancias de nuestros estados concretos
    public State_Grounded StateGrounded { get; private set; }
    public State_Airborne StateAirborne { get; private set; }

    private void Awake()
    {
        Input = GetComponent<PlayerInput>();
        Physics = GetComponent<PlayerPhysics>();

        // Inicializamos los estados inyectando este controlador
        StateGrounded = new State_Grounded(this, Physics, Input);
        StateAirborne = new State_Airborne(this, Physics, Input);
    }

    private void Start()
    {
        // Estado inicial
        ChangeState(StateGrounded);
    }

    private void Update()
    {
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