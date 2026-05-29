using UnityEngine;

public class State_Grounded : PlayerState
{
    public State_Grounded(PlayerController controller, PlayerPhysics physics, PlayerInput input)
        : base(controller, physics, input) { }

    public override void Enter()
    {
        // Nos suscribimos a los eventos del Input al entrar al estado
        input.OnJump += HandleJump;
        input.OnShoot += HandleShoot;
    }

    public override void FixedUpdate()
    {
        // Moverse usando la física nativa
        physics.Move(input.MovementInput);

        // Si caemos por un borde, transicionamos al estado aéreo
        if (!physics.IsGrounded())
        {
            controller.ChangeState(controller.StateAirborne);
        }
    }

    private void HandleJump()
    {
        physics.Jump();
        controller.ChangeState(controller.StateAirborne);
    }

    private void HandleShoot()
    {
        // Calculamos la dirección opuesta a la que estamos mirando (asumiendo input X)
        // Si no nos movemos, disparamos hacia adelante y nos empuja hacia atrás.
        Vector2 facingDirection = input.MovementInput.x != 0 ? new Vector2(input.MovementInput.x, 0).normalized : Vector2.right;
        Vector2 recoilDirection = -facingDirection;

        physics.ApplyCrossbowRecoil(recoilDirection);
        // El impulso masivo nos levantará o empujará del borde, la transición a Airborne ocurrirá sola en el FixedUpdate
    }

    public override void Exit()
    {
        // IMPORTANTE: Desuscribirse para no disparar eventos múltiples
        input.OnJump -= HandleJump;
        input.OnShoot -= HandleShoot;
    }
}