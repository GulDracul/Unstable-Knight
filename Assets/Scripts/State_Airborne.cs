using UnityEngine;

public class State_Airborne : PlayerState
{
    public State_Airborne(PlayerController controller, PlayerPhysics physics, PlayerInput input)
        : base(controller, physics, input) { }

    public override void Enter()
    {
        // Escuchamos el disparo en el aire (Dash de la Ballesta)
        input.OnShoot += HandleShoot;
        // NUEVO: Escuchamos el gatillo derecho
        input.OnAbilityLegs += HandleLegsAbility;
        input.OnAbilityChest += HandleChestAbility;
        input.OnAbilityArms += HandlePunch;
    }

    public override void FixedUpdate()
    {
        // Si tocamos el agua, cambiamos de estado y abortamos el resto del movimiento
        if (physics.IsInWater())
        {
            controller.ChangeState(controller.StateWater);
            return;
        }
        // Le enviamos "true" para indicarle que debe respetar la inercia del péndulo
        physics.Move(input.MovementInput, true);

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
    private void HandleLegsAbility()
    {
        // Transicionamos al estado de caída libre
        controller.ChangeState(controller.StateGroundPound);
    }
    private void HandleChestAbility()
    {
        // Cambiamos de estado sin preguntar. El State_Grapple manejará el tiro.
        controller.ChangeState(controller.StateGrapple);
    }
    public override void Exit()
    {
        input.OnShoot -= HandleShoot;
        input.OnAbilityLegs -= HandleLegsAbility;
        input.OnAbilityChest -= HandleChestAbility;
        input.OnAbilityArms -= HandlePunch;
    }
    private void HandlePunch()
    {
        // Le pasamos a las físicas la dirección hacia la que estamos mirando
        physics.Punch(controller.LastFacingDirection);
    }
}