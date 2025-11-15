using UnityEngine;

public abstract class CajaTablero : MonoBehaviour
{
    public int x;
    public int y;

    public TableroManager manager;

    private void OnMouseDown()
    {
        EventBus.Explode(this);
    }

    public abstract void EjecutarExplosion();
}
