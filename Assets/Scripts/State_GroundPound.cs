using UnityEngine;

public class State_GroundPound : PlayerState
{
    private float startY; // Aquí guardaremos la altura inicial
    public State_GroundPound(PlayerController controller, PlayerPhysics physics, PlayerInput input)
        : base(controller, physics, input) { }

    public override void Enter()
    {
        // 1. Guardamos la posición Y exacta desde la que iniciamos el ataque
        startY = physics.transform.position.y;
        physics.StartHeavyDrop();
    }

    public override void FixedUpdate()
    {
        // No llamamos a physics.Move(input) intencionalmente. 
        // ¡Pierdes el control horizontal mientras caes a plomo!

        // Si chocamos contra el suelo...
        if (physics.IsGrounded())
        {
            // 2. Calculamos la distancia: Posición de inicio menos Posición actual
            float dropDistance = startY - physics.transform.position.y;

            // 3. Disparamos la onda de choque pasándole esa distancia
            physics.TriggerShockwave(dropDistance);

            // Y volvemos al estado normal
            controller.ChangeState(controller.StateGrounded);
        }
    }

    public override void Exit()
    {
        // Restauramos la gravedad al salir del estado
        physics.StopHeavyDrop();
    }
}