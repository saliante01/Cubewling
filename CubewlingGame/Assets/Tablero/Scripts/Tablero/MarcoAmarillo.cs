using UnityEngine;

public class MarcoAmarillo : CajaTablero
{
    public override void EjecutarExplosion()
    {
        manager.ExplotarFila(y);
    }
}
