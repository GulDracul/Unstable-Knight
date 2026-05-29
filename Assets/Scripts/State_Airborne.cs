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
        // Si ya usaste la ballesta en el aire, cancelamos el disparo
        if (!controller.CanUseCrossbowInAir) return;

        // FORZAR 8 DIRECCIONES: Mismo cálculo que en el suelo
        Vector2 rawAim = input.MovementInput;
        float snapX = rawAim.x > 0.1f ? 1 : (rawAim.x < -0.1f ? -1 : 0);
        float snapY = rawAim.y > 0.1f ? 1 : (rawAim.y < -0.1f ? -1 : 0);

        Vector2 snappedAim = new Vector2(snapX, snapY);

        if (snappedAim == Vector2.zero)
        {
            snappedAim = new Vector2(controller.LastFacingDirection, 0);
        }

        Vector2 recoilDirection = -snappedAim.normalized;
        physics.ApplyCrossbowRecoil(recoilDirection);

        // GASTAR BALA: Desactivamos el uso aéreo hasta tocar tierra
        controller.CanUseCrossbowInAir = false;
    }

    public override void Exit()
    {
        input.OnShoot -= HandleShoot;
    }
}