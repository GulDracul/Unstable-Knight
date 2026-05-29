using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController controller;
    protected PlayerPhysics physics;
    protected PlayerInput input;

    // Constructor que inyecta las dependencias al estado
    public PlayerState(PlayerController controller, PlayerPhysics physics, PlayerInput input)
    {
        this.controller = controller;
        this.physics = physics;
        this.input = input;
    }

    // Métodos que cada estado concreto deberá definir
    public virtual void Enter() { }
    public virtual void HandleInput() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
}