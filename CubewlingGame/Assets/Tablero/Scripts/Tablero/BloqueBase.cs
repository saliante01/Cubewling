using UnityEngine;

public abstract class BloqueBase : MonoBehaviour
{
    public int x;
    public int y;

    public TableroManagerPosicional tablero;

    public BloqueBase vecinoArriba;
    public BloqueBase vecinoAbajo;
    public BloqueBase vecinoIzquierda;
    public BloqueBase vecinoDerecha;

    public abstract void Activar();
}
