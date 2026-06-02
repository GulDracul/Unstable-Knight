using UnityEngine;

public class State_Grapple : PlayerState
{
    private bool isHookConnected; // Para saber si ya llegó al techo

    public State_Grapple(PlayerController controller, PlayerPhysics physics, PlayerInput input)
        : base(controller, physics, input) { }

    public override void Enter()
    {
        isHookConnected = false;

        input.OnJump += HandleJump;
        input.OnShoot += HandleShoot;

        // Disparamos el arpón pasando dos "órdenes" sobre qué hacer al terminar el viaje
        physics.TryShootGrapple(
            onHookConnected: () => isHookConnected = true, // Éxito: Activa el balanceo
            onHookMissed: () => controller.ChangeState(controller.StateAirborne) // Fallo: Te devuelve a la caída
        );
    }

    public override void FixedUpdate()
    {
        // Solo puedes balancearte si el gancho ya chocó contra la madera
        if (isHookConnected)
        {
            physics.ApplySwingForce(input.MovementInput.x);
        }
    }

    private void HandleJump()
    {
        controller.ChangeState(controller.StateAirborne);
    }

    private void HandleShoot()
    {
        // Si el gancho sigue viajando, no puedes usar la ballesta aún
        if (!isHookConnected || !controller.CanUseCrossbowInAir || controller.CrossbowCooldownTimer > 0) return;

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
        controller.CanUseCrossbowInAir = false;
    }

    public override void Exit()
    {
        physics.DetachGrapple();
        input.OnJump -= HandleJump;
        input.OnShoot -= HandleShoot;
    }
}