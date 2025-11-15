using UnityEngine;

public class CajaTableroNormal : CajaTablero
{
    public override void EjecutarExplosion()
    {
        manager.DestruirCaja(x, y);
    }
}
