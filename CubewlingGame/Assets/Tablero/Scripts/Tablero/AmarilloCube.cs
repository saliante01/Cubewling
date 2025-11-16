using UnityEngine;

public class AmarilloCube : BloqueBase
{
    public override void Activar()
    {
        if (tablero != null)
        {
            tablero.RomperFilaDesdeAmarillo(this);
        }
    }
}
