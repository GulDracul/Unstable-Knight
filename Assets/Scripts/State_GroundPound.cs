using UnityEngine;

public class State_GroundPound : PlayerState
{
    public State_GroundPound(PlayerController controller, PlayerPhysics physics, PlayerInput input)
        : base(controller, physics, input) { }

    public override void Enter()
    {
        physics.StartHeavyDrop();
    }

    public override void FixedUpdate()
    {
        // No llamamos a physics.Move(input) intencionalmente. 
        // ¡Pierdes el control horizontal mientras caes a plomo!

        // Si chocamos contra el suelo...
        if (physics.IsGrounded())
        {
            // 1. Explotamos
            physics.GenerateShockwave();

            // 2. Volvemos al estado normal de caminar
            controller.ChangeState(controller.StateGrounded);
        }
    }

    public override void Exit()
    {
        // Restauramos la gravedad al salir del estado
        physics.StopHeavyDrop();
    }
}