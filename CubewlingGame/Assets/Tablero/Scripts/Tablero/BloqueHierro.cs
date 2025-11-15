using UnityEngine;

public class BloqueHierro : BloqueBase
{
    public override void Activar()
    {
        tablero.PedirExplosion(this);
    }
}
