using UnityEngine;

public class LanzadorCajaController : MonoBehaviour
{
    [Header("Potencia")]
    public float maxPower = 9f;
    public float powerSpeed = 15f;
    private float currentPower = 0f;
    public float CurrentPower => currentPower;

    [Header("Parábola")]
    public float parabolaAngle = 0f;
    public float parabolaMin = -20f;
    public float parabolaMax = 20f;
    public bool puedeElegirParabola = true;

    private bool charging = false;
    private bool hasLaunched = false;

    private FisicasCaja boxPhysics;

    private void Awake()
    {
        boxPhysics = GetComponent<FisicasCaja>();
    }

    private void Update()
    {
        if (hasLaunched) return;

        // --- AJUSTAR PARÁBOLA ---
        if (puedeElegirParabola)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
                parabolaAngle -= 20f * Time.deltaTime;

            if (Input.GetKey(KeyCode.RightArrow))
                parabolaAngle += 20f * Time.deltaTime;

            parabolaAngle = Mathf.Clamp(parabolaAngle, parabolaMin, parabolaMax);
        }

        // --- CARGA DE POTENCIA ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            charging = true;
        }

        if (charging)
        {
            currentPower += powerSpeed * Time.deltaTime;
            currentPower = Mathf.Clamp(currentPower, 0f, maxPower);
        }

        // --- LANZAMIENTO ---
        if (Input.GetKeyUp(KeyCode.Space))
        {
            charging = false;
            Launch();
        }
    }

    private void Launch()
    {
        if (!TirosManager.Instance.CanShoot())
        {
            Debug.Log("NO HAY TIROS DISPONIBLES");
            return;
        }

        hasLaunched = true;
        puedeElegirParabola = false;

        TirosManager.Instance.ConsumirTiro();

        Vector3 fuerza = CalcularFuerzaDeLanzamiento();
        boxPhysics.ApplyLaunchForce(fuerza);

        currentPower = 0f;
    }

    private Vector3 CalcularFuerzaDeLanzamiento()
    {
        Vector3 fuerza = transform.forward * currentPower;

        // aplicar parábola
        fuerza.y += parabolaAngle;

        return fuerza;
    }

    public void ResetLaunch()
    {
        hasLaunched = false;
        currentPower = 0f;

        // volver a permitir ajustar parábola
        puedeElegirParabola = true;
    }
}
