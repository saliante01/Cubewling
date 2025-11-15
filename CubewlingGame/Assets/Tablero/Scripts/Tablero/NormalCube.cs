using UnityEngine;

public class NormalCube : BloqueBase
{
    public override void Activar()
    {
        Destroy(gameObject);
    }
}
