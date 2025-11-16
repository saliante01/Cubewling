using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableroManagerPosicional : MonoBehaviour
{
    [Header("PADRE con N hijos (uno por fila, de 0 abajo a N-1 arriba)")]
    public Transform contenedorPrincipalFilas;

    [Header("Prefabs")]
    public GameObject prefabHierro;
    public GameObject prefabNormal;
    public GameObject prefabAmarillo;

    [Header("Probabilidades")]
    [Range(0, 1f)] public float probabilidadHierro = 0.3f;
    [Range(0, 1f)] public float probabilidadAmarillo = 0.1f;

    [Header("Tiempos")]
    public float tiempoAntesDeBajar = 0.3f;
    public float tiempoBajada = 0.4f;

    // Lista global de bloques
    public List<BloqueBase> bloques = new List<BloqueBase>();

    // Filas lógicas fijas: índice = ID de fila (0 = abajo, N-1 = arriba)
    private List<List<BloqueBase>> filas = new List<List<BloqueBase>>();

    // Explosiones
    private Queue<BloqueBase> cola = new Queue<BloqueBase>();
    private HashSet<BloqueBase> procesados = new HashSet<BloqueBase>();

    // Para evitar colapsar la misma fila más de una vez
    private HashSet<int> filasEnColapso = new HashSet<int>();

    void Start()
    {
        InicializarEstructuraFilas();
        InstanciarBloquesTablero();
        DetectarVecinosPorDistancia();
        LogEstadoFilas("Estado inicial del tablero");
    }

    // ============================================================
    // 1. Inicializar estructura fija de filas (IDs)
    // ============================================================
    void InicializarEstructuraFilas()
    {
        filas.Clear();

        int totalFilas = contenedorPrincipalFilas.childCount;
        for (int i = 0; i < totalFilas; i++)
        {
            filas.Add(new List<BloqueBase>());
        }
    }

    int TotalFilas => filas.Count;

    // ============================================================
    // 2. Instanciar bloques al inicio
    // ============================================================
    void InstanciarBloquesTablero()
    {
        bloques.Clear();

        // Suponemos que el hijo 0 del contenedor es la fila 0 (inferior),
        // el hijo 1 la fila 1, etc.
        for (int row = 0; row < contenedorPrincipalFilas.childCount; row++)
        {
            Transform filaContenedor = contenedorPrincipalFilas.GetChild(row);

            foreach (Transform marcador in filaContenedor)
            {
                Vector3 pos = marcador.position;
                GameObject prefabElegido = ElegirPrefabAleatorio();

                GameObject obj = Instantiate(prefabElegido, pos, Quaternion.identity);
                BloqueBase bloque = obj.GetComponent<BloqueBase>();

                bloque.tablero = this;
                bloque.rowIndex = row;

                bloques.Add(bloque);
                filas[row].Add(bloque);
            }
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
    // 3. Detectar vecinos por distancia (para Hierro)
    // ============================================================
    void DetectarVecinosPorDistancia()
    {
        foreach (var b in bloques)
        {
            if (!b) continue;

            b.vecinoArriba = null;
            b.vecinoAbajo = null;
            b.vecinoIzquierda = null;
            b.vecinoDerecha = null;

            foreach (var otro in bloques)
            {
                if (!otro || otro == b) continue;

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
    // 4. Notificación de destrucción de un bloque (Normales)
    // ============================================================
    public void NotificarBloqueDestruido(BloqueBase bloque)
    {
        if (bloque == null) return;

        int fila = bloque.rowIndex;

        bloques.Remove(bloque);

        if (fila >= 0 && fila < TotalFilas)
        {
            filas[fila].Remove(bloque);

            Debug.Log($"[TABLERO] Bloque {bloque.GetType().Name} destruido en fila {fila}. " +
                      $"Quedan {filas[fila].Count} bloques en esa fila.");

            Destroy(bloque.gameObject);

            // Si la fila quedó vacía, se colapsa
            if (filas[fila].Count == 0 && !filasEnColapso.Contains(fila))
            {
                StartCoroutine(ColapsarFilaCoroutine(fila, "FILA VACÍA (último bloque destruido)"));
            }
        }
        else
        {
            Destroy(bloque.gameObject);
        }
    }

    // ============================================================
    // 5. Amarillo llamado directamente por click
    // ============================================================
    public void RomperFilaDesdeAmarillo(BloqueBase amarillo)
    {
        if (amarillo == null) return;
        int fila = amarillo.rowIndex;

        if (fila < 0 || fila >= TotalFilas) return;

        if (!filasEnColapso.Contains(fila))
        {
            StartCoroutine(ColapsarFilaCoroutine(fila, "AMARILLO (click)"));
        }
    }

    // ============================================================
    // 6. Explosiones en cadena (Hierro)
    // ============================================================
    public void PedirExplosion(BloqueBase bloque)
    {
        if (bloque == null) return;

        cola.Enqueue(bloque);
        ProcesarExplosiones();
    }

    void ProcesarExplosiones()
    {
        while (cola.Count > 0)
        {
            BloqueBase actual = cola.Dequeue();
            if (!actual || procesados.Contains(actual)) continue;

            procesados.Add(actual);

            // Si es amarillo afectado por explosión, rompe su fila
            if (actual is AmarilloCube)
            {
                int filaAmarillo = actual.rowIndex;
                if (filaAmarillo >= 0 && filaAmarillo < TotalFilas && !filasEnColapso.Contains(filaAmarillo))
                {
                    StartCoroutine(ColapsarFilaCoroutine(filaAmarillo, "AMARILLO (por explosión)"));
                }
                continue;
            }

            // El resto de bloques se destruyen normalmente
            int fila = actual.rowIndex;

            // Guardar vecinos ANTES de destruir (solo si hierro)
            BloqueBase arriba = actual.vecinoArriba;
            BloqueBase abajo = actual.vecinoAbajo;
            BloqueBase izq = actual.vecinoIzquierda;
            BloqueBase der = actual.vecinoDerecha;

            NotificarBloqueDestruido(actual);

            // Si era hierro, encola vecinos
            if (actual is BloqueHierro)
            {
                if (arriba) cola.Enqueue(arriba);
                if (abajo) cola.Enqueue(abajo);
                if (izq) cola.Enqueue(izq);
                if (der) cola.Enqueue(der);
            }
        }

        procesados.Clear();
    }

    // ============================================================
    // 7. Corrutina de colapso de fila (tipo Tetris)
    // ============================================================
    IEnumerator ColapsarFilaCoroutine(int filaObjetivo, string motivo)
    {
        if (filaObjetivo < 0 || filaObjetivo >= TotalFilas)
            yield break;

        if (filasEnColapso.Contains(filaObjetivo))
            yield break;

        filasEnColapso.Add(filaObjetivo);

        Debug.Log($"[TABLERO] >>> Fila {filaObjetivo} se destruye. Motivo: {motivo}");

        // 1) Borrar todos los bloques de esa fila (si aún quedaba alguno)
        List<BloqueBase> filaAEliminar = new List<BloqueBase>(filas[filaObjetivo]);

        foreach (var b in filaAEliminar)
        {
            if (!b) continue;

            bloques.Remove(b);
            filas[filaObjetivo].Remove(b);
            Destroy(b.gameObject);
        }

        // 2) Esperar antes de bajar
        if (tiempoAntesDeBajar > 0f)
            yield return new WaitForSeconds(tiempoAntesDeBajar);

        // 3) Preparar animación de bajada de contenido
        List<BloqueBase> bloquesAMover = new List<BloqueBase>();
        List<Vector3> starts = new List<Vector3>();
        List<Vector3> targets = new List<Vector3>();

        // Construir nueva estructura de filas
        List<List<BloqueBase>> nuevasFilas = new List<List<BloqueBase>>();
        for (int i = 0; i < TotalFilas; i++)
        {
            nuevasFilas.Add(new List<BloqueBase>());
        }

        for (int f = 0; f < TotalFilas; f++)
        {
            if (f < filaObjetivo)
            {
                // Filas por debajo quedan donde están
                foreach (var b in filas[f])
                {
                    if (!b) continue;
                    nuevasFilas[f].Add(b);
                }
            }
            else if (f > filaObjetivo)
            {
                // Filas por encima bajan una posición
                foreach (var b in filas[f])
                {
                    if (!b) continue;

                    int nuevaFila = f - 1;
                    nuevasFilas[nuevaFila].Add(b);
                    b.rowIndex = nuevaFila;

                    bloquesAMover.Add(b);
                    starts.Add(b.transform.position);
                    targets.Add(b.transform.position + new Vector3(0f, 0f, -1f)); // baja una unidad en Z
                }
            }
            // f == filaObjetivo: esa fila se "pierde", será rellenada por la de arriba
        }

        // La fila superior (ID más alto) queda vacía
        // (nuevasFilas[TotalFilas - 1] ya está vacía porque nadie se mueve "para arriba")

        // 4) Animación
        float t = 0f;
        if (tiempoBajada > 0f && bloquesAMover.Count > 0)
        {
            while (t < tiempoBajada)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / tiempoBajada);

                for (int i = 0; i < bloquesAMover.Count; i++)
                {
                    if (!bloquesAMover[i]) continue;
                    bloquesAMover[i].transform.position = Vector3.Lerp(starts[i], targets[i], a);
                }

                yield return null;
            }
        }

        // 5) Posición final por seguridad
        for (int i = 0; i < bloquesAMover.Count; i++)
        {
            if (!bloquesAMover[i]) continue;
            bloquesAMover[i].transform.position = targets[i];
        }

        // 6) Reemplazar estructura de filas
        filas = nuevasFilas;

        // 7) Recalcular vecinos
        DetectarVecinosPorDistancia();

        // 8) Log de estado final
        LogEstadoFilas($"Después de colapsar fila {filaObjetivo}");

        filasEnColapso.Remove(filaObjetivo);
    }

    // ============================================================
    // 8. Logs de estado de filas
    // ============================================================
    void LogEstadoFilas(string contexto)
    {
        List<int> llenas = new List<int>();
        List<int> vacias = new List<int>();

        for (int i = 0; i < TotalFilas; i++)
        {
            if (filas[i].Count > 0)
                llenas.Add(i);
            else
                vacias.Add(i);
        }

        string txtLlenas = llenas.Count > 0 ? string.Join(", ", llenas) : "ninguna";
        string txtVacias = vacias.Count > 0 ? string.Join(", ", vacias) : "ninguna";

        Debug.Log($"[TABLERO] {contexto} -> Filas con bloques: {txtLlenas} | Filas vacías: {txtVacias}");
    }
}
