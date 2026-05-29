using UnityEngine;
using System;
using UnityEngine.InputSystem; // Añadido para claridad

public class PlayerInput : MonoBehaviour
{
    // Vector público que el PlayerController leerá para saber hacia dónde te mueves
    public Vector2 MovementInput { get; private set; }

    // Eventos que la Máquina de Estados va a escuchar
    public event Action OnJump;
    public event Action OnShoot;
    public event Action OnAbilityLegs;
    public event Action OnAbilityChest;

    // Referencia a la clase que Unity generó automáticamente en el Paso 4
    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();

        // 1. Leer movimiento continuo
        controls.Gameplay.Movement.performed += ctx => MovementInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Movement.canceled += ctx => MovementInput = Vector2.zero;

        // 2. Escuchar pulsaciones de botones (Disparar eventos)
        controls.Gameplay.Jump.performed += ctx => OnJump?.Invoke();
        controls.Gameplay.Shoot.performed += ctx => OnShoot?.Invoke();
        controls.Gameplay.AbilityLegs.performed += ctx => OnAbilityLegs?.Invoke();
        controls.Gameplay.AbilityChest.performed += ctx => OnAbilityChest?.Invoke();
    }

    // Es vital activar y desactivar los controles
    private void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }
}