using UnityEngine;

public class State_Airborne : PlayerState
{
    public State_Airborne(PlayerController controller, PlayerPhysics physics, PlayerInput input)
        : base(controller, physics, input) { }

    public override void Enter()
    {
        // Escuchamos el disparo en el aire (Dash de la Ballesta)
        input.OnShoot += HandleShoot;
    }

    public override void FixedUpdate()
    {
        // Control aéreo: permitimos moverse pero podrías reducir la velocidad aquí si quieres
        physics.Move(input.MovementInput);

        // Si tocamos el suelo, volvemos al estado normal
        if (physics.IsGrounded())
        {
            controller.ChangeState(controller.StateGrounded);
        }
    }

    private void HandleShoot()
    {
        // Mismo cálculo de dirección: Dash aéreo opuesto hacia donde miramos
        Vector2 facingDirection = input.MovementInput.x != 0 ? new Vector2(input.MovementInput.x, 0).normalized : Vector2.right;
        Vector2 recoilDirection = -facingDirection;

        physics.ApplyCrossbowRecoil(recoilDirection);
    }

    public override void Exit()
    {
        input.OnShoot -= HandleShoot;
    }
}