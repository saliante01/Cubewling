using System.Collections.Generic;
using UnityEngine;

public class TableroManagerPosicional : MonoBehaviour
{
    [Header("PADRE con los marcadores (GameObjects vacíos)")]
    public Transform contenedorMarcadores;

    [Header("Prefabs disponibles")]
    public GameObject prefabHierro;
    public GameObject prefabNormal;

    [Header("Probabilidades")]
    [Range(0, 1f)] public float probabilidadHierro = 0.5f;

    // Lista con todos los bloques colocados
    public List<BloqueBase> bloques = new List<BloqueBase>();

    private Queue<BloqueBase> cola = new Queue<BloqueBase>();
    private HashSet<BloqueBase> procesados = new HashSet<BloqueBase>();

    private void Start()
    {
        InstanciarBloques();
        DetectarVecinosPorDistancia();
    }

    // ============================================================
    // 1. INSTANCIAR BLOQUES ALEATORIOS EN MARCADORES
    // ============================================================

    void InstanciarBloques()
    {
        bloques.Clear();

        foreach (Transform marcador in contenedorMarcadores)
        {
            Vector3 pos = marcador.position;

            // Elegir tipo al azar
            GameObject prefabElegido = 
                (Random.value <= probabilidadHierro) ? prefabHierro : prefabNormal;

            // Instanciar cubo
            GameObject obj = Instantiate(prefabElegido, pos, Quaternion.identity);

            BloqueBase bloque = obj.GetComponent<BloqueBase>();

            if (bloque == null)
            {
                Debug.LogError("El prefab no tiene BloqueBase.");
                continue;
            }

            bloque.tablero = this;

            bloques.Add(bloque);
        }
    }

    // ============================================================
    // 2. DETECTAR VECINOS POR DISTANCIA EXACTA
    // ============================================================

    void DetectarVecinosPorDistancia()
    {
        foreach (var b in bloques)
        {
            b.vecinoArriba = null;
            b.vecinoAbajo = null;
            b.vecinoIzquierda = null;
            b.vecinoDerecha = null;

            foreach (var otro in bloques)
            {
                if (otro == b) continue;

                float dx = otro.transform.position.x - b.transform.position.x;
                float dz = otro.transform.position.z - b.transform.position.z;

                if (Mathf.Abs(dx) < 0.2f && Mathf.Abs(dz - 1f) < 0.2f) b.vecinoArriba = otro;
                if (Mathf.Abs(dx) < 0.2f && Mathf.Abs(dz + 1f) < 0.2f) b.vecinoAbajo = otro;
                if (Mathf.Abs(dz) < 0.2f && Mathf.Abs(dx - 1f) < 0.2f) b.vecinoDerecha = otro;
                if (Mathf.Abs(dz) < 0.2f && Mathf.Abs(dx + 1f) < 0.2f) b.vecinoIzquierda = otro;
            }
        }
    }

    // ============================================================
    // 3. EXPLOSIÓN EN CADENA (solo hierro la activa)
    // ============================================================

    public void PedirExplosion(BloqueBase bloque)
    {
        cola.Enqueue(bloque);
        ProcesarExplosiones();
    }

    void ProcesarExplosiones()
    {
        while (cola.Count > 0)
        {
            BloqueBase actual = cola.Dequeue();
            if (actual == null) continue;

            // Si ya fue procesado, saltar
            if (procesados.Contains(actual)) continue;

            procesados.Add(actual);

            // Guardar vecinos ANTES de destruir
            BloqueBase up = actual.vecinoArriba;
            BloqueBase down = actual.vecinoAbajo;
            BloqueBase left = actual.vecinoIzquierda;
            BloqueBase right = actual.vecinoDerecha;

            bloques.Remove(actual);
            Destroy(actual.gameObject);

            // SOLO hierro genera efectos en cadena
            if (actual is BloqueHierro)
            {
                if (up) cola.Enqueue(up);
                if (down) cola.Enqueue(down);
                if (left) cola.Enqueue(left);
                if (right) cola.Enqueue(right);
            }
        }

        procesados.Clear();
    }
}
