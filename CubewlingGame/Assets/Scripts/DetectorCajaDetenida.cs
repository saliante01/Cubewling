using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class DetectorCajaDetenida : MonoBehaviour
{
    [Header("Detección de detención")]
    public float stopVelocityThreshold = 0.05f;
    public float requiredStopTime = 0.5f;

    private float stopTimer = 0f;
    private Rigidbody rb;
    private bool hasNotified = false;
    private bool detectionEnabled = false;

    public event Action OnBoxStopped;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!detectionEnabled || hasNotified)
            return;

        // ¿Está casi detenida?
        if (rb.linearVelocity.magnitude < stopVelocityThreshold)
        {
            stopTimer += Time.deltaTime;

            if (stopTimer >= requiredStopTime)
            {
                hasNotified = true;
                Debug.Log("La caja se ha detenido.");

                OnBoxStopped?.Invoke();

                // Explosión de cubos tocados en ese instante
                ExplodeTouchedBlocksAtStop();

                // Respawn de la caja
                Invoke(nameof(ResetBoxDelayed), 0.15f);
            }
        }
        else
        {
            stopTimer = 0f;
        }
    }

    private void ExplodeTouchedBlocksAtStop()
    {
        Collider boxCol = GetComponent<Collider>();

        Vector3 center = boxCol.bounds.center;
        Vector3 halfExtents = boxCol.bounds.extents * 1.1f;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);

        Debug.Log("CUBOS DETECTADOS: " + hits.Length);

        foreach (Collider col in hits)
        {
            BloqueBase b = col.GetComponent<BloqueBase>();
            if (b != null)
            {
                Debug.Log("Activando cubo: " + b.name);
                b.Activar();
            }
        }
    }

    private void ResetBoxDelayed()
    {
        FisicasCaja fc = GetComponent<FisicasCaja>();
        if (fc != null)
            fc.ResetBox();
    }

    public void EnableDetection()
    {
        detectionEnabled = true;
        stopTimer = 0f;
        hasNotified = false;
    }
}
