using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class FisicasCaja : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;

    [Header("Fricción del deslizamiento")]
    public float dynamicFriction = 0.1f;
    public float staticFriction = 0.1f;

    [Header("Punto de respawn")]
    public Transform respawnPoint;

    [Header("Plataforma que mueve la caja (Padre)")]
    public Transform plataformaMovimiento;

    private bool launched = false;
    private Quaternion initialRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (plataformaMovimiento == null)
        {
            Debug.LogError("Debes asignar la plataformaMovimiento en FisicasCaja.");
        }

        if (respawnPoint == null)
        {
            Debug.LogWarning("No se asignó RespawnPoint. Usando posición actual.");
            GameObject temp = new GameObject("RespawnPoint_Auto");
            temp.transform.position = transform.position;
            respawnPoint = temp.transform;
        }

        initialRotation = transform.rotation;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = true;

        SetupSlidingPhysics();
    }

    private void SetupSlidingPhysics()
    {
        PhysicsMaterial mat = new PhysicsMaterial();
        mat.dynamicFriction = dynamicFriction;
        mat.staticFriction = staticFriction;
        mat.frictionCombine = PhysicsMaterialCombine.Multiply;
        col.material = mat;
    }

    // ============================================================
    //     LANZAMIENTO
    // ============================================================
    public void ApplyLaunchForce(float power)
    {
        if (launched) return;
        launched = true;

        // DEPARENTAR → ya no es movida por la plataforma
        transform.parent = null;

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Dirección recta hacia adelante
        Vector3 dir = plataformaMovimiento.forward; // ← USAR LA DIRECCIÓN DE LA PLATAFORMA
        dir.x = 0;
        dir.y = 0;

        rb.AddForce(dir.normalized * power, ForceMode.Impulse);

        DetectorCajaDetenida detector = GetComponent<DetectorCajaDetenida>();
        if (detector != null)
            detector.EnableDetection();
    }

    // ============================================================
    //      RESET PERFECTO
    // ============================================================
    public void ResetBox()
    {
        StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        launched = false;

        rb.isKinematic = true;

        yield return null;

        // 🔥 VUELVE A SER HIJO DE LA PLATAFORMA
        transform.parent = plataformaMovimiento;

        // 🔥 PONER EXACTAMENTE EN EL PUNTO DE RESPAWN
        transform.position = respawnPoint.position;

        // 🔥 ALINEAMOS MISMO FORWARD QUE LA PLATAFORMA
        transform.rotation = plataformaMovimiento.rotation;

        yield return null;

        // RESET DEL LANZADOR
        LanzadorCajaController launcher = GetComponent<LanzadorCajaController>();
        if (launcher != null)
            launcher.ResetLaunch();

        // REACTIVAR DETECTOR
        DetectorCajaDetenida detector = GetComponent<DetectorCajaDetenida>();
        if (detector != null)
            detector.EnableDetection();
    }
}
