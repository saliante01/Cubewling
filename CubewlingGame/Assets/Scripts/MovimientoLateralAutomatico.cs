using UnityEngine;

public class MovimientoLateralAutomatico : MonoBehaviour
{
    [Header("Velocidad del movimiento")]
    public float speed = 3f;

    [Header("Límites de movimiento")]
    public float leftLimit = -3f;
    public float rightLimit = 3f;

    private int direction = 1; // 1 = derecha, -1 = izquierda

    void Update()
    {
        // Mover en la dirección actual
        transform.position += Vector3.right * direction * speed * Time.deltaTime;

        // Revisar límites y cambiar dirección
        if (transform.position.x >= rightLimit)
            direction = -1; // Ir hacia la izquierda
        
        if (transform.position.x <= leftLimit)
            direction = 1;  // Ir hacia la derecha
    }
}
