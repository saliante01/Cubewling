using UnityEngine;

public class BloqueBase : MonoBehaviour
{
    public TableroManagerPosicional tablero;

    public BloqueBase vecinoArriba;
    public BloqueBase vecinoAbajo;
    public BloqueBase vecinoIzquierda;
    public BloqueBase vecinoDerecha;

    // ID lógico de fila: 0 = más baja, 5 = más alta
    public int rowIndex; 

    public virtual void Activar() { }
}
