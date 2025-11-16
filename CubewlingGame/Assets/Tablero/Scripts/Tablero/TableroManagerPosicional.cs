using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableroManagerPosicional : MonoBehaviour
{
    [Header("Contenedor principal: hijos = filas (0 abajo, N-1 arriba)")]
    public Transform contenedorPrincipalFilas;

    [Header("Prefabs")]
    public GameObject prefabHierro;
    public GameObject prefabNormal;
    public GameObject prefabAmarillo;

    [Header("Probabilidades")]
    [Range(0, 1f)] public float probabilidadHierro = 0.3f;
    [Range(0, 1f)] public float probabilidadAmarillo = 0.1f;

    [Header("Tiempos")]
    public float tiempoAntesDeBajar = 0.25f;
    public float tiempoBajada = 0.15f;

    [Header("Efectos Visuales")]
    public GameObject vfxExplosion;

    [Header("Audio Global")]
    public AudioClip sonidoAmarilloGlobal;
    public float volumenAmarilloGlobal = 1f;

    public List<BloqueBase> bloques = new List<BloqueBase>();

    private List<List<BloqueBase>> filas = new List<List<BloqueBase>>();
    private List<List<Transform>> marcadores = new List<List<Transform>>();

    private Queue<BloqueBase> cola = new Queue<BloqueBase>();
    private HashSet<BloqueBase> procesados = new HashSet<BloqueBase>();
    private HashSet<int> filasEnColapso = new HashSet<int>();

    int TotalFilas => filas.Count;

    void Start()
    {
        InicializarEstructuraFilas();
        InstanciarBloquesTablero();
        DetectarVecinosPorDistancia();
        LogEstadoFilas("Estado inicial");
    }

    void InicializarEstructuraFilas()
    {
        filas.Clear();
        marcadores.Clear();

        int total = contenedorPrincipalFilas.childCount;

        for (int filaID = 0; filaID < total; filaID++)
        {
            filas.Add(new List<BloqueBase>());

            List<Transform> lista = new List<Transform>();
            foreach (Transform m in contenedorPrincipalFilas.GetChild(filaID))
                lista.Add(m);

            marcadores.Add(lista);
        }
    }

    void InstanciarBloquesTablero()
    {
        bloques.Clear();

        for (int filaID = 0; filaID < TotalFilas; filaID++)
        {
            foreach (Transform m in marcadores[filaID])
            {
                BloqueBase nuevo = CrearBloqueEnFila(filaID, m.position, m);
                filas[filaID].Add(nuevo);
                bloques.Add(nuevo);
            }
        }
    }

    BloqueBase CrearBloqueEnFila(int filaID, Vector3 pos, Transform marcador)
    {
        GameObject prefab = ElegirPrefabAleatorio();
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);

        obj.transform.localScale = marcador.localScale;

        BloqueBase b = obj.GetComponent<BloqueBase>();
        b.tablero = this;
        b.rowIndex = filaID;

        return b;
    }

    GameObject ElegirPrefabAleatorio()
    {
        float r = Random.value;

        if (r < probabilidadAmarillo) return prefabAmarillo;
        if (r < probabilidadAmarillo + probabilidadHierro) return prefabHierro;

        return prefabNormal;
    }

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
    // Destrucción normal + VFX + Sonido
    // ============================================================
    public void NotificarBloqueDestruido(BloqueBase b)
    {
        int f = b.rowIndex;

        bloques.Remove(b);
        filas[f].Remove(b);

        // ==== 💥 VFX ====
        if (vfxExplosion != null)
        {
            GameObject fx = Instantiate(vfxExplosion, b.transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        // ==== 🔊 Sonido individual del bloque ====
        if (b.sonidoExplosion != null)
        {
            AudioSource.PlayClipAtPoint(b.sonidoExplosion, b.transform.position, b.volumenSonido);
        }

        // ==== 🔊 Sonido global adicional SOLO si es amarillo ====
        if (b is AmarilloCube && sonidoAmarilloGlobal != null)
        {
            AudioSource.PlayClipAtPoint(
                sonidoAmarilloGlobal,
                Camera.main.transform.position,
                volumenAmarilloGlobal
            );
        }

        Destroy(b.gameObject);

        Debug.Log($"[TABLERO] Bloque destruido en fila {f}. Quedan {filas[f].Count}.");

        if (filas[f].Count == 0)
            StartCoroutine(ColapsarFilaCoroutine(f, "FILA VACÍA"));
    }

    // ============================================================
    // Amarillo rompe fila COMPLETA
    // ============================================================
    public void RomperFilaDesdeAmarillo(BloqueBase amarillo)
    {
        int f = amarillo.rowIndex;
        StartCoroutine(ColapsarFilaCoroutine(f, "AMARILLO"));
    }

    // ============================================================
    // Explosión en cadena (Hierro)
    // ============================================================
    public void PedirExplosion(BloqueBase b)
    {
        cola.Enqueue(b);
        ProcesarExplosiones();
    }

    void ProcesarExplosiones()
    {
        while (cola.Count > 0)
        {
            BloqueBase actual = cola.Dequeue();
            if (!actual || procesados.Contains(actual)) continue;

            procesados.Add(actual);

            if (actual is AmarilloCube)
            {
                RomperFilaDesdeAmarillo(actual);
                continue;
            }

            BloqueBase up = actual.vecinoArriba;
            BloqueBase dn = actual.vecinoAbajo;
            BloqueBase iz = actual.vecinoIzquierda;
            BloqueBase de = actual.vecinoDerecha;

            NotificarBloqueDestruido(actual);

            if (actual is BloqueHierro)
            {
                if (up) cola.Enqueue(up);
                if (dn) cola.Enqueue(dn);
                if (iz) cola.Enqueue(iz);
                if (de) cola.Enqueue(de);
            }
        }

        procesados.Clear();
    }

    // ============================================================
    // Colapso tipo Tetris (activando hierros antes de destruir)
    // ============================================================
    IEnumerator ColapsarFilaCoroutine(int filaObjetivo, string motivo)
    {
        if (filasEnColapso.Contains(filaObjetivo))
            yield break;

        filasEnColapso.Add(filaObjetivo);

        Debug.Log($"[TABLERO] >>> Fila {filaObjetivo} se destruye ({motivo})");

        // ==== 1) Activar hierros antes ====
        foreach (var b in new List<BloqueBase>(filas[filaObjetivo]))
        {
            if (b is BloqueHierro)
            {
                b.Activar();
            }
        }

        yield return null;

        // ==== 2) Destruir lo que quede ====
        foreach (var b in new List<BloqueBase>(filas[filaObjetivo]))
        {
            if (b != null)
                NotificarBloqueDestruido(b);
        }

        if (tiempoAntesDeBajar > 0f)
            yield return new WaitForSeconds(tiempoAntesDeBajar);

        // ==== Bajada ====
        List<BloqueBase> movidos = new List<BloqueBase>();
        List<Vector3> start = new List<Vector3>();
        List<Vector3> target = new List<Vector3>();

        List<List<BloqueBase>> nueva = new List<List<BloqueBase>>();

        for (int i = 0; i < TotalFilas; i++)
            nueva.Add(new List<BloqueBase>());

        for (int f = 0; f < filaObjetivo; f++)
            foreach (var b in filas[f])
                nueva[f].Add(b);

        for (int f = filaObjetivo + 1; f < TotalFilas; f++)
        {
            int nuevoID = f - 1;

            for (int col = 0; col < filas[f].Count; col++)
            {
                BloqueBase b = filas[f][col];
                b.rowIndex = nuevoID;

                movidos.Add(b);
                start.Add(b.transform.position);

                Transform destino = marcadores[nuevoID][col];
                target.Add(destino.position);

                nueva[nuevoID].Add(b);
            }
        }

        filas = nueva;

        float t = 0;
        while (t < tiempoBajada)
        {
            t += Time.deltaTime;
            float a = t / tiempoBajada;

            for (int i = 0; i < movidos.Count; i++)
                movidos[i].transform.position = Vector3.Lerp(start[i], target[i], a);

            yield return null;
        }

        for (int i = 0; i < movidos.Count; i++)
            movidos[i].transform.position = target[i];

        DetectarVecinosPorDistancia();
        LogEstadoFilas($"Después de colapsar fila {filaObjetivo}");

        filasEnColapso.Remove(filaObjetivo);

        StartCoroutine(RefillFilasVacias());
    }

    // ============================================================
    // Refill
    // ============================================================
    IEnumerator RefillFilasVacias()
    {
        List<int> vacias = new List<int>();

        for (int i = 0; i < TotalFilas; i++)
            if (filas[i].Count == 0)
                vacias.Add(i);

        if (vacias.Count == 0)
            yield break;

        Debug.Log($"[TABLERO] Rellenando filas vacías: {string.Join(",", vacias)}");

        foreach (int filaID in vacias)
        {
            foreach (Transform m in marcadores[filaID])
            {
                BloqueBase nuevo = CrearBloqueEnFila(filaID, m.position, m);

                Vector3 escalaFinal = m.localScale;
                nuevo.transform.localScale = Vector3.zero;

                StartCoroutine(AnimarSpawn(nuevo.transform, escalaFinal));

                filas[filaID].Add(nuevo);
                bloques.Add(nuevo);
            }
        }

        DetectarVecinosPorDistancia();
        LogEstadoFilas("Después del refill");
    }

    // Animación pop-in
    IEnumerator AnimarSpawn(Transform t, Vector3 escalaFinal)
    {
        Vector3 escalaInicial = Vector3.zero;

        float dur = 0.2f;
        float tiempo = 0f;

        while (tiempo < dur)
        {
            tiempo += Time.deltaTime;
            float a = tiempo / dur;

            t.localScale = Vector3.Lerp(escalaInicial, escalaFinal, a);

            yield return null;
        }

        t.localScale = escalaFinal;
    }

    void LogEstadoFilas(string msg)
    {
        List<int> llenas = new List<int>();
        List<int> vacias = new List<int>();

        for (int i = 0; i < TotalFilas; i++)
        {
            if (filas[i].Count > 0) llenas.Add(i);
            else vacias.Add(i);
        }

        Debug.Log($"[TABLERO] {msg} → Llenas: {string.Join(",", llenas)} | Vacías: {string.Join(",", vacias)}");
    }
}
