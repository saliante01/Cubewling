using UnityEngine;

public class NormalCube : BloqueBase
{
    public override void Activar()
    {
        // Este cubo SOLO explota si se le hace click
        // NO activa vecinos, NO genera reacción en cadena.
        Destroy(gameObject);
    }
}
