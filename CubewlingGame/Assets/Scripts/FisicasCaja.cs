using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class FisicasCaja : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;

    private Transform parentInicial;

    [Header("Fricción del deslizamiento")]
    public float dynamicFriction = 0.1f;
    public float staticFriction = 0.1f;

    private bool launched = false;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // ===== SISTEMA NUEVO DE AUDIO =====
    private CajaSlideAudioController slideAudio;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Guardar estado inicial
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // Guardar parent inicial (la plataforma de lanzamiento)
        parentInicial = transform.parent;

        // Bloquear toda rotación
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Al inicio, sin físicas
        rb.isKinematic = true;

        SetupSlidingPhysics();

        // Obtener referencia al sistema de audio
        slideAudio = GetComponent<CajaSlideAudioController>();
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
    // APLICACIÓN DE FUERZA DE LANZAMIENTO
    // ============================================================
    public void ApplyLaunchForce(Vector3 fuerza)
    {
        if (launched) return;
        launched = true;

        // Quitamos parent
        transform.SetParent(null);

        rb.isKinematic = false;

        // Aplicamos fuerza compuesta
        rb.AddForce(fuerza, ForceMode.Impulse);

        // === AUDIO DE ENTRADA ===
        if (slideAudio != null)
            slideAudio.OnLaunch(fuerza.magnitude);

        // Activamos detección de detención
        DetectorCajaDetenida detector = GetComponent<DetectorCajaDetenida>();
        if (detector != null)
            detector.EnableDetection();
    }

    // ============================================================
    // RESET — RUTINA MULTIFRAME CORRECTA
    // ============================================================
    public void ResetBox()
    {
        StartCoroutine(ResetBoxRoutine());
    }

    private IEnumerator ResetBoxRoutine()
    {
        launched = false;

        // Fase 1 — congelar física
        rb.isKinematic = true;

        yield return null;

        // Fase 2 — volver a parent original
        transform.SetParent(parentInicial);

        // Fase 3 — restaurar posición local exacta
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        yield return null;

        // === AUDIO: detener loop y resetear flags ===
        if (slideAudio != null)
            slideAudio.OnReset();

        // Fase 4 — resetear lanzador
        LanzadorCajaController launcher = GetComponent<LanzadorCajaController>();
        if (launcher != null)
            launcher.ResetLaunch();
    }
}
