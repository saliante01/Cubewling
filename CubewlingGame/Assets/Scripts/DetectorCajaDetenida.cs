using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class DetectorCajaDetenida : MonoBehaviour
{
    [Header("Deteccion de detencion")]
    public float stopVelocityThreshold = 0.05f;     // Velocidad minima para considerar detenido
    public float requiredStopTime = 0.5f;           // Tiempo que debe mantenerse detenido

    private float stopTimer = 0f;
    private Rigidbody rb;
    private bool hasNotified = false;

    private bool detectionEnabled = false;

    // Evento para notificar a otros scripts
    public event Action OnBoxStopped;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!detectionEnabled || hasNotified) return;

        // comprobar si la velocidad es "casi cero"
        if (rb.linearVelocity.magnitude < stopVelocityThreshold)
        {
            stopTimer += Time.deltaTime;

            if (stopTimer >= requiredStopTime)
            {
                hasNotified = true;
                print("La caja se ha detenido.");
                // Lanzar el evento
                OnBoxStopped?.Invoke();
            }
        }
        else
        {
            stopTimer = 0f; // la caja volvio a moverse
        }
    }

    public void EnableDetection()
    {
        detectionEnabled = true;
        stopTimer = 0f;
        hasNotified = false;
    }
}
