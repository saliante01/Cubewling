using UnityEngine;

public class CajaFueraDeTableroDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FueraDeTablero"))
        {
            FisicasCaja fc = GetComponent<FisicasCaja>();
            if (fc != null)
                fc.ResetBox();
        }
    }
}
