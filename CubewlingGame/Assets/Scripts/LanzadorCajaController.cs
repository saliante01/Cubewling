using UnityEngine;

public class LanzadorCajaController : MonoBehaviour
{
    [Header("Movimiento horizontal")]
    public float horizontalSpeed = 5f;
    public float minX = -5f;
    public float maxX = 5f;

    [Header("Carga de potencia")]
    public KeyCode powerKey = KeyCode.Space;
    public float maxPower = 20f;
    public float powerChargeSpeed = 10f;

    private float currentPower = 0f;
    private bool hasLaunched = false;

    private FisicasCaja boxPhysics;

    private void Start()
    {
        boxPhysics = GetComponent<FisicasCaja>();

        if (boxPhysics == null)
            Debug.LogError("BoxPhysics no está adjunto al mismo objeto.");
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
        if (Input.GetKey(powerKey))
        {
            currentPower += powerChargeSpeed * Time.deltaTime;
            currentPower = Mathf.Clamp(currentPower, 0, maxPower);
        }

        if (Input.GetKeyUp(powerKey))
        {
            Launch();
        }
    }

    private void Launch()
    {
        hasLaunched = true;

        // Llama a la física para aplicar la fuerza real
        boxPhysics.ApplyLaunchForce(currentPower);
    }
}
