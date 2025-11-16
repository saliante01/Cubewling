using UnityEngine;

public class BloqueBase : MonoBehaviour
{
    public TableroManagerPosicional tablero;

    public BloqueBase vecinoArriba;
    public BloqueBase vecinoAbajo;
    public BloqueBase vecinoIzquierda;
    public BloqueBase vecinoDerecha;

    public int rowIndex;

    // 🔊 Sonido específico del bloque
    public AudioClip sonidoExplosion;
    public float volumenSonido = 1f;

    public virtual void Activar() { }
}
