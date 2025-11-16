using UnityEngine;

public class BloqueHierro : BloqueBase
{
    public override void Activar()
    {
        if (tablero != null)
        {
            tablero.PedirExplosion(this);
        }
    }
}
