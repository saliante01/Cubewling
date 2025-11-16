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

    [Header("DEBUG (solo lectura)")]
    [SerializeField] private float currentPower = 0f;

    private bool hasLaunched = false;
    private bool isCharging = false;     // si realmente está cargando
    private bool maxReached = false;     // llegó al máximo y se congela

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
        // ==== INICIO CARGA ====
        if (Input.GetKeyDown(powerKey))
        {
            isCharging = true;
            maxReached = false;
            currentPower = 0f;
        }

        // ==== MIENTRAS SE MANTIENE PRESIONADO ====
        if (Input.GetKey(powerKey) && isCharging)
        {
            // Si ya llegó al máximo, NO recalculamos más
            if (maxReached)
            {
                currentPower = maxPower;
                return;
            }

            // Aumentar potencia
            currentPower += powerChargeSpeed * Time.deltaTime;

            if (currentPower >= maxPower)
            {
                currentPower = maxPower;
                maxReached = true;
            }
        }

        // ==== AL SOLTAR ====
        if (Input.GetKeyUp(powerKey))
        {
            isCharging = false;

            Launch();
        }
    }

    private void Launch()
    {
        hasLaunched = true;
        boxPhysics.ApplyLaunchForce(currentPower);
    }

    public void ResetLaunch()
    {
        hasLaunched = false;

        // reset del ciclo de carga
        isCharging = false;
        maxReached = false;
        currentPower = 0f;
    }
}
