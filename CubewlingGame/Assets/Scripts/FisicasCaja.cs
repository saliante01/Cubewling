using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class FisicasCaja : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;

    [Header("Friccion del deslizamiento")]
    public float dynamicFriction = 0.1f;
    public float staticFriction = 0.1f;

    private bool launched = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // La caja inicia sin fisicas
        rb.isKinematic = true;

        // Aplicar friccion baja
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

    public void ApplyLaunchForce(float power)
    {
        if (launched) return;
        launched = true;

        rb.isKinematic = false;

        // Direccion hacia adelante
        Vector3 force = transform.forward * power;

        rb.AddForce(force, ForceMode.Impulse);

        DetectorCajaDetenida detector = GetComponent<DetectorCajaDetenida>();
        if (detector != null)
            detector.EnableDetection();

    }
}
