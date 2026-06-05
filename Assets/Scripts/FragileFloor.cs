using UnityEngine;

public class FragileFloor : MonoBehaviour
{
    public void Break()
    {
        // Más adelante aquí reproduciremos un sonido metálico y partículas
        Destroy(gameObject);
    }
}