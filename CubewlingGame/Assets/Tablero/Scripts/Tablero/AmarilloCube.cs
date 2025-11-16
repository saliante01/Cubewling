using UnityEngine;

public class AmarilloCube : BloqueBase
{
    public override void Activar()
    {
        tablero.RomperFila(this);
    }
}
