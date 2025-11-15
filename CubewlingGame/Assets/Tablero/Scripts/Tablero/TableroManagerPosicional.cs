using System.Collections.Generic;
using UnityEngine;

public class TableroManagerPosicional : MonoBehaviour
{
    [Header("PADRE de marcadores del tablero principal")]
    public Transform contenedorMarcadores;

    [Header("Prefabs")]
    public GameObject prefabHierro;
    public GameObject prefabNormal;
    public GameObject prefabAmarillo;

    [Header("Probabilidades")]
    [Range(0, 1f)] public float probabilidadHierro = 0.3f;
    [Range(0, 1f)] public float probabilidadAmarillo = 0.1f;

    public List<BloqueBase> bloques = new List<BloqueBase>();

    // Cola para explosión en cadena
    public Queue<BloqueBase> cola = new Queue<BloqueBase>();
    public HashSet<BloqueBase> procesados = new HashSet<BloqueBase>();


    // ======================================================================
    void Start()
    {
        InstanciarBloquesTablero();
        DetectarVecinosPorDistancia();
    }

    // ======================================================================
    //   1. GENERAR TABLERO PRINCIPAL ALEATORIO
    // ======================================================================

    void InstanciarBloquesTablero()
    {
        bloques.Clear();

        foreach (Transform marcador in contenedorMarcadores)
        {
            Vector3 pos = marcador.position;
            GameObject prefabElegido = ElegirPrefabAleatorio();

            GameObject obj = Instantiate(prefabElegido, pos, Quaternion.identity);

            BloqueBase bloque = obj.GetComponent<BloqueBase>();
            bloque.tablero = this;

            bloques.Add(bloque);
        }
    }

    GameObject ElegirPrefabAleatorio()
    {
        float r = Random.value;

        if (r < probabilidadAmarillo)
            return prefabAmarillo;

        if (r < probabilidadAmarillo + probabilidadHierro)
            return prefabHierro;

        return prefabNormal;
    }

    // ======================================================================
    //   2. DETECTAR VECINOS POR DISTANCIA EXACTA
    // ======================================================================

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

                if (Mathf.Abs(dx) < 0.2f && Mathf.Abs(dz - 1f) < 0.2f)
                    b.vecinoArriba = otro;

                if (Mathf.Abs(dx) < 0.2f && Mathf.Abs(dz + 1f) < 0.2f)
                    b.vecinoAbajo = otro;

                if (Mathf.Abs(dz) < 0.2f && Mathf.Abs(dx - 1f) < 0.2f)
                    b.vecinoDerecha = otro;

                if (Mathf.Abs(dz) < 0.2f && Mathf.Abs(dx + 1f) < 0.2f)
                    b.vecinoIzquierda = otro;
            }
        }
    }

    // ======================================================================
    //   3. EXPLOSIÓN EN CADENA (HIERRO + FIX DEL AMARILLO)
    // ======================================================================

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
            if (actual == null || procesados.Contains(actual))
                continue;

            procesados.Add(actual);

            BloqueBase up = actual.vecinoArriba;
            BloqueBase down = actual.vecinoAbajo;
            BloqueBase left = actual.vecinoIzquierda;
            BloqueBase right = actual.vecinoDerecha;

            // ==============================================================
            //   FIX IMPORTANTE:
            //   Si el hierro explota un AMARILLO → el amarillo rompe la fila
            // ==============================================================
            if (actual is AmarilloCube)
            {
                RomperFila(actual); // <-- DETONA SU FUNCIONALIDAD ANTES DE DESTRUIR
            }

            // ==============================================================
            //   Eliminar el bloque actual
            // ==============================================================
            bloques.Remove(actual);
            Destroy(actual.gameObject);

            // ==============================================================
            //   Explosión en cruz del Hierro
            // ==============================================================
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

    // ======================================================================
    //   4. FUNCIONALIDAD DEL AMARILLO – ROMPER FILA COMPLETA
    // ======================================================================
    public void RomperFila(BloqueBase amarillo)
    {
        float filaZ = amarillo.transform.position.z;

        List<BloqueBase> filaAEliminar = new List<BloqueBase>();

        foreach (var b in bloques)
        {
            if (Mathf.Abs(b.transform.position.z - filaZ) < 0.1f)
                filaAEliminar.Add(b);
        }

        foreach (var b in filaAEliminar)
        {
            bloques.Remove(b);

            if (b is BloqueHierro)
                cola.Enqueue(b);

            Destroy(b.gameObject);
        }

        // Procesar las explosiones en cadena
        ProcesarExplosiones();
    }
}
