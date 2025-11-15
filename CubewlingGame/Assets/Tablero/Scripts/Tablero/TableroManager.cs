using UnityEngine;

public class TableroManager : MonoBehaviour
{
    public int width = 6;
    public int height = 6;

    public BarraExtraManager barraExtra;

    public GameObject prefabNormal;
    public GameObject prefabAmarillo;
    public GameObject prefabHierro;

    private CajaTablero[,] grid;
    private Transform root;

    private void Awake()
    {
        root = transform.parent;
    }

    void Start()
    {
        grid = new CajaTablero[width, height + 2];
        EventBus.OnExplode += RecibirExplosion;
        GenerarTableroInicial();
    }

    void RecibirExplosion(CajaTablero caja)
    {
        caja.EjecutarExplosion();
    }

    // ============================================================
    // GENERAR TABLERO
    // ============================================================

    void GenerarTableroInicial()
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                CrearCajaAleatoria(x, y, PosicionMundo(x, y));
    }

    void CrearCajaAleatoria(int x, int y, Vector3 pos)
    {
        float r = Random.value;

        GameObject prefab =
            (r < 0.05f) ? prefabHierro :
            (r < 0.25f) ? prefabAmarillo :
                          prefabNormal;

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity, root);

        var caja = obj.GetComponent<CajaTablero>();
        caja.x = x;
        caja.y = y;
        caja.manager = this;

        grid[x, y] = caja;
    }

    // ============================================================
    // PREFAB ALEATORIO PARA BARRA EXTRA
    // ============================================================

    public GameObject ElegirPrefabAleatorioExternamente(
        GameObject normal, GameObject amarillo, GameObject hierro)
    {
        float r = Random.value;

        if (r < 0.05f) return hierro;
        if (r < 0.25f) return amarillo;
        return normal;
    }

    // ============================================================
    // POSICIÓN MUNDO
    // ============================================================

    public Vector3 PosicionMundo(int x, int y)
    {
        return new Vector3(x, 0, y);
    }

    bool EsCeldaValida(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public void DestruirCaja(int x, int y)
    {
        if (!EsCeldaValida(x, y)) return;
        if (grid[x, y] == null) return;

        Destroy(grid[x, y].gameObject);
        grid[x, y] = null;
    }

    // ============================================================
    // EXPLOSIÓN AMARILLO
    // ============================================================

    public void ExplotarFila(int fila)
    {
        for (int x = 0; x < width; x++)
            DestruirCaja(x, fila);

        AplicarGravedad();
        barraExtra.BajarBarraHastaFila(fila);
    }

    // ============================================================
    // EXPLOSIÓN HIERRO (solo adyacentes, sin gravedad)
    // ============================================================

    public void ExplotarHierro(int x, int y)
    {
        if (!EsCeldaValida(x, y)) return;
        if (grid[x, y] == null) return;

        Vector2Int[] vecinos = new Vector2Int[]
        {
            new Vector2Int(x - 1, y),
            new Vector2Int(x + 1, y),
            new Vector2Int(x, y - 1),
            new Vector2Int(x, y + 1)
        };

        DestruirCaja(x, y);

        foreach (var v in vecinos)
        {
            if (!EsCeldaValida(v.x, v.y)) continue;
            if (grid[v.x, v.y] == null) continue;

            CajaTablero cajaVecina = grid[v.x, v.y];

            if (cajaVecina is MarcoAmarillo)
            {
                ExplotarFila(v.y);
                return;
            }

            DestruirCaja(v.x, v.y);
        }
    }

    // ============================================================
    // GRAVEDAD (solo cuando se explota una fila)
    // ============================================================

    void AplicarGravedad()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y < height; y++)
            {
                if (grid[x, y] != null && grid[x, y - 1] == null)
                {
                    BajaCaja(x, y);
                    y = 0;
                }
            }
        }
    }

    void BajaCaja(int x, int y)
    {
        var caja = grid[x, y];
        int newY = y;

        while (newY > 0 && grid[x, newY - 1] == null)
            newY--;

        grid[x, y] = null;
        grid[x, newY] = caja;

        caja.y = newY;
        caja.transform.position = PosicionMundo(x, newY);
    }

    // ============================================================
    // INTEGRAR BARRA EXTRA
    // ============================================================

    public void IntegrarBarra(GameObject[] barra)
    {
        int fila = height - 1;

        for (int x = 0; x < width; x++)
        {
            var caja = barra[x].GetComponent<CajaTablero>();
            caja.x = x;
            caja.y = fila;
            caja.manager = this;

            grid[x, fila] = caja;
        }
    }
}
