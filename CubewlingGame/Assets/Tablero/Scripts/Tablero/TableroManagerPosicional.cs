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

    // Fila -> lista de cubos
    private Dictionary<int, List<BloqueBase>> filas = new Dictionary<int, List<BloqueBase>>();

    // Explosión en cadena
    public Queue<BloqueBase> cola = new Queue<BloqueBase>();
    public HashSet<BloqueBase> procesados = new HashSet<BloqueBase>();


    void Start()
    {
        InstanciarBloquesTablero();
        DetectarVecinosPorDistancia();
        OrganizarFilas();
    }

    // ============================================================
    //   1. GENERAR TABLERO
    // ============================================================
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

            bloque.rowIndex = Mathf.RoundToInt(pos.z);

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

    // ============================================================
    //   2. ORGANIZAR FILAS
    // ============================================================
    void OrganizarFilas()
    {
        filas.Clear();

        foreach (var b in bloques)
        {
            if (!filas.ContainsKey(b.rowIndex))
                filas[b.rowIndex] = new List<BloqueBase>();

            filas[b.rowIndex].Add(b);
        }
    }

    // ============================================================
    //   3. DETECTAR VECINOS
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

    // ============================================================
    //   4. EXPLOSIÓN EN CADENA
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
            if (actual == null || procesados.Contains(actual))
                continue;

            procesados.Add(actual);

            BloqueBase up = actual.vecinoArriba;
            BloqueBase down = actual.vecinoAbajo;
            BloqueBase left = actual.vecinoIzquierda;
            BloqueBase right = actual.vecinoDerecha;

            // Amarillo activado por hierro
            if (actual is AmarilloCube)
                RomperFila(actual);

            // Eliminar del tablero
            bloques.Remove(actual);
            if (filas.ContainsKey(actual.rowIndex))
                filas[actual.rowIndex].Remove(actual);

            Destroy(actual.gameObject);

            // Hierro explota en cruz
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

    // ============================================================
    //   5. ROMPER FILA (BAJAN TODAS LAS FILAS SUPERIORES)
    // ============================================================
    public void RomperFila(BloqueBase amarillo)
    {
        int filaObjetivo = amarillo.rowIndex;

        if (!filas.ContainsKey(filaObjetivo))
            return;

        // COPIA de los bloques a eliminar
        List<BloqueBase> filaAEliminar = new List<BloqueBase>(filas[filaObjetivo]);

        // 1. Quitar del tablero
        foreach (var b in filaAEliminar)
        {
            bloques.Remove(b);

            if (filas.ContainsKey(b.rowIndex))
                filas[b.rowIndex].Remove(b);

            // Si hay hierro en la fila, explota
            if (b is BloqueHierro)
                cola.Enqueue(b);

            Destroy(b.gameObject);
        }

        // 2. Remover la fila del diccionario
        filas.Remove(filaObjetivo);

        // 3. BAJAR TODAS LAS FILAS SUPERIORES
        List<int> claves = new List<int>(filas.Keys);
        claves.Sort(); // orden ascendente

        foreach (int fila in claves)
        {
            if (fila > filaObjetivo)
            {
                List<BloqueBase> filaMover = filas[fila];

                foreach (var b in filaMover)
                {
                    b.transform.position += new Vector3(0, 0, -1f);
                    b.rowIndex -= 1;
                }

                // mover en diccionario
                filas.Remove(fila);
                filas[fila - 1] = filaMover;
            }
        }

        // recalcular vecinos después del movimiento
        DetectarVecinosPorDistancia();

        // aplicar explosiones pendientes
        ProcesarExplosiones();
    }
}
