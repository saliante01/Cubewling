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

        if (rb.linearVelocity.magnitude < stopVelocityThreshold)
        {
            stopTimer += Time.deltaTime;

            if (stopTimer >= requiredStopTime)
            {
                hasNotified = true;

                OnBoxStopped?.Invoke();

                ExplodeTouchedBlocksAtStop();

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
        Collider col = GetComponent<Collider>();

        Vector3 center = col.bounds.center;
        Vector3 halfExtents = col.bounds.extents * 1.1f;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);

        foreach (Collider h in hits)
        {
            BloqueBase b = h.GetComponent<BloqueBase>();
            if (b != null)
                b.Activar();
        }
    }

    private void ResetBoxDelayed()
    {
        FisicasCaja fc = GetComponent<FisicasCaja>();
        if (fc != null)
            fc.ResetBox();

        EnableDetection();
    }

    public void EnableDetection()
    {
        detectionEnabled = true;
        stopTimer = 0f;
        hasNotified = false;
    }
}
