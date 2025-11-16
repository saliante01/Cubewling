using UnityEngine;

public class NormalCube : BloqueBase
{
    public override void Activar()
    {
        if (tablero != null)
        {
            tablero.NotificarBloqueDestruido(this);
        }
    }
}
