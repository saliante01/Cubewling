using UnityEngine;

public class InputRaycaster : MonoBehaviour
{
    public TableroManagerPosicional tablero;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                BloqueBase b = hit.collider.GetComponent<BloqueBase>();
                if (b != null)
                {
                    tablero.PedirExplosion(b);
                }
            }
        }
    }
}
