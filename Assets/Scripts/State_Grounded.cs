using UnityEngine;

public class State_Grounded : PlayerState
{
    public State_Grounded(PlayerController controller, PlayerPhysics physics, PlayerInput input)
        : base(controller, physics, input) { }

    public override void Enter()
    {
        input.OnJump += HandleJump;
        input.OnShoot += HandleShoot;

        // RECARGA: Al pisar el suelo, recuperas tu dash aéreo
        controller.CanUseCrossbowInAir = true;
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
        // Si el enfriamiento no ha terminado, cancelamos el disparo
        if (controller.CrossbowCooldownTimer > 0) return;

        // FORZAR 8 DIRECCIONES: Convertimos cualquier ángulo analógico en -1, 0 o 1
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

        // ENFRIAMIENTO: Tiempo de espera antes de volver a disparar en el suelo (ej: 0.5 segundos)
        controller.CrossbowCooldownTimer = 0.5f;
    }

    public override void Exit()
    {
        // IMPORTANTE: Desuscribirse para no disparar eventos múltiples
        input.OnJump -= HandleJump;
        input.OnShoot -= HandleShoot;
    }
}