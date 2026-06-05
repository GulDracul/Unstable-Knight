using UnityEngine;

public class State_Water : PlayerState
{
    public State_Water(PlayerController controller, PlayerPhysics physics, PlayerInput input)
        : base(controller, physics, input) { }

    public override void Enter()
    {
        // Modificamos la gravedad para simular el ambiente acuático
        physics.SetGravity(physics.waterGravityScale);

        // SOLO permitimos las reliquias compatibles con el agua
        input.OnAbilityHead += HandleYelmo;
        input.OnAbilityLegs += HandleBotas;

        // NUEVO: Ahora el estado del agua es consciente de que existe el botón de salto
        input.OnJump += HandleJump;
    }

    public override void FixedUpdate()
    {
        // Si salimos del área del agua, el motor nos devuelve al estado aéreo
        if (!physics.IsInWater())
        {
            controller.ChangeState(controller.StateAirborne);
            return;
        }

        // Aplicamos el movimiento denso y viscoso del agua
        physics.MoveInWater(input.MovementInput);
    }

    private void HandleYelmo()
    {
        physics.ApplyHelmPropulsion();
    }

    private void HandleBotas()
    {
        physics.ApplyWaterSink();
    }
    // NUEVO: Método del salto
    private void HandleJump()
    {
        // El salto bajo el agua SOLO funciona si tocamos el fondo
        if (physics.IsGrounded())
        {
            physics.ApplyWaterJump();
        }
    }

    public override void Exit()
    {
        // Restauramos la gravedad normal al salir a la superficie
        physics.SetGravity(physics.normalGravityScale);

        input.OnAbilityHead -= HandleYelmo;
        input.OnAbilityLegs -= HandleBotas;
        input.OnJump -= HandleJump;
    }
}