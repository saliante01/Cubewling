using UnityEngine;

public class CajaFueraDeTableroDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FueraDeTablero"))
        {
            Debug.Log("La caja salió del tablero. Reseteando tiro y posición.");

            FisicasCaja fc = GetComponent<FisicasCaja>();
            if (fc != null)
            {
                fc.ResetBox();
            }

            DetectorCajaDetenida detector = GetComponent<DetectorCajaDetenida>();
            if (detector != null)
            {
                detector.EnableDetection(); // por si la caja se relanza
            }
        }
    }
}
