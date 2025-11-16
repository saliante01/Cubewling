using UnityEngine;

public class BloqueBase : MonoBehaviour
{
    public TableroManagerPosicional tablero;

    public BloqueBase vecinoArriba;
    public BloqueBase vecinoAbajo;
    public BloqueBase vecinoIzquierda;
    public BloqueBase vecinoDerecha;

    public int rowIndex; // fila a la que pertenece este bloque

    public virtual void Activar() { }
}
