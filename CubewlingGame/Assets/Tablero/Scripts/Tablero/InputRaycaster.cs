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
                BloqueBase bloque = hit.collider.GetComponent<BloqueBase>();
                if (bloque != null)
                {
                    bloque.Activar();
                }
            }
        }
    }
}
