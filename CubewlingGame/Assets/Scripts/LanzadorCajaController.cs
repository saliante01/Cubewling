using UnityEngine;

public class LanzadorCajaController : MonoBehaviour
{
    [Header("Movimiento horizontal")]
    public float horizontalSpeed = 5f;
    public float minX = -5f;
    public float maxX = 5f;

    [Header("Carga de potencia")]
    public KeyCode powerKey = KeyCode.Space;

    [Tooltip("Potencia máxima que se puede cargar")]
    public float maxPower = 20f;

    [Tooltip("Velocidad a la que aumenta la potencia por segundo")]
    public float powerChargeSpeed = 10f;

    [Header("Tiro parabólico (opcional)")]
    public bool usarParabola = false;

    [Tooltip("Ángulo del disparo parabólico (0° recto – 60° alto)")]
    [Range(0f, 60f)]
    public float anguloLanzamiento = 25f;

    [Tooltip("Multiplicador de fuerza vertical")]
    public float potenciaVerticalExtra = 1.2f;

    [Header("DEBUG (solo lectura)")]
    public float currentPower = 0f;

    private bool hasLaunched = false;
    private bool isCharging = false;
    private bool maxReached = false;

    private FisicasCaja boxPhysics;

    private void Start()
    {
        boxPhysics = GetComponent<FisicasCaja>();

        if (boxPhysics == null)
            Debug.LogError("FisicasCaja no está adjunto.");
    }

    private void Update()
    {
        if (hasLaunched) return;

        HandleHorizontalMovement();
        HandlePowerCharge();
    }

    private void HandleHorizontalMovement()
    {
        float dir = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) dir = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) dir = 1f;

        if (dir != 0f)
        {
            Vector3 pos = transform.position;
            pos.x += dir * horizontalSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            transform.position = pos;
        }
    }

    private void HandlePowerCharge()
    {
        // Inicio de carga
        if (Input.GetKeyDown(powerKey))
        {
            isCharging = true;
            maxReached = false;
            currentPower = 0f;
        }

        if (Input.GetKey(powerKey) && isCharging)
        {
            if (maxReached)
            {
                currentPower = maxPower;
                return;
            }

            currentPower += powerChargeSpeed * Time.deltaTime;

            if (currentPower >= maxPower)
            {
                currentPower = maxPower;
                maxReached = true;
            }
        }

        // Al soltar
        if (Input.GetKeyUp(powerKey))
        {
            isCharging = false;
            Launch();
        }
    }

    private void Launch()
    {
        hasLaunched = true;

        // === GENERAR FUERZA DEL DISPARO ===
        Vector3 fuerzaFinal = CalcularFuerzaDeLanzamiento();

        boxPhysics.ApplyLaunchForce(fuerzaFinal);
    }

    private Vector3 CalcularFuerzaDeLanzamiento()
    {
        // Fuerza horizontal base
        Vector3 forward = transform.forward * currentPower;

        if (!usarParabola)
            return forward;

        // Convertimos el ángulo a radianes
        float rad = anguloLanzamiento * Mathf.Deg2Rad;

        // Componente vertical
        float verticalForce = Mathf.Sin(rad) * currentPower * potenciaVerticalExtra;

        // Disparo parabólico real
        Vector3 final = forward;
        final.y = verticalForce;

        return final;
    }

    public void ResetLaunch()
    {
        hasLaunched = false;
        isCharging = false;
        maxReached = false;
        currentPower = 0f;
    }
}
