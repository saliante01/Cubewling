using System.Collections.Generic;
using UnityEngine;

public class DetectorBloquesColisionados : MonoBehaviour
{
    public List<BloqueBase> bloquesTocados = new List<BloqueBase>();

    private void OnCollisionEnter(Collision col)
    {
        BloqueBase b = col.collider.GetComponent<BloqueBase>();
        if (b != null && !bloquesTocados.Contains(b))
            bloquesTocados.Add(b);
    }

    private void OnCollisionStay(Collision col)
    {
        BloqueBase b = col.collider.GetComponent<BloqueBase>();
        if (b != null && !bloquesTocados.Contains(b))
            bloquesTocados.Add(b);
    }

    private void OnCollisionExit(Collision col)
    {
        BloqueBase b = col.collider.GetComponent<BloqueBase>();
        if (b != null)
            bloquesTocados.Remove(b);
    }
}
