using UnityEngine;

public class MarcoHierro : CajaTablero
{
    public override void EjecutarExplosion()
    {
        manager.ExplotarHierro(x, y);
    }
}
