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

    private CajaSlideAudioController slideAudio;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        parentInicial = transform.parent;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = true;

        SetupSlidingPhysics();

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

    public void ApplyLaunchForce(Vector3 fuerza)
    {
        if (launched) return;
        launched = true;

        transform.SetParent(null);
        rb.isKinematic = false;

        rb.AddForce(fuerza, ForceMode.Impulse);

        if (slideAudio != null)
            slideAudio.OnLaunch(fuerza.magnitude);

        DetectorCajaDetenida detector = GetComponent<DetectorCajaDetenida>();
        if (detector != null)
            detector.EnableDetection();
    }

    public void ResetBox()
    {
        StartCoroutine(ResetBoxRoutine());
    }

    private IEnumerator ResetBoxRoutine()
    {
        launched = false;
        rb.isKinematic = true;

        yield return null;

        transform.SetParent(parentInicial);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        yield return null;

        if (slideAudio != null)
            slideAudio.OnReset();

        LanzadorCajaController launcher = GetComponent<LanzadorCajaController>();
        if (launcher != null)
        {
            launcher.ResetLaunch();
            launcher.puedeElegirParabola = true; // ← SIEMPRE permitir parábola al reset
        }
    }
}
