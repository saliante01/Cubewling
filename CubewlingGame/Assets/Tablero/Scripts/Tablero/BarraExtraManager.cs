using System.Collections;
using UnityEngine;

public class BarraExtraManager : MonoBehaviour
{
    public TableroManager tablero;

    public GameObject prefabNormal;
    public GameObject prefabAmarillo;
    public GameObject prefabHierro;

    private GameObject[] barraActual = new GameObject[6];
    public float alturaExtra = 7f;
    public float velocidadCaida = 4f;

    private Transform root;

    private void Awake()
    {
        root = transform.parent;
    }

    private void Start()
    {
        CrearBarraExtra();
    }

    // ============================================================
    // CREACIÓN DE BARRA EXTRA
    // ============================================================

    void CrearBarraExtra()
    {
        for (int x = 0; x < 6; x++)
        {
            GameObject prefab = tablero.ElegirPrefabAleatorioExternamente(
                prefabNormal, prefabAmarillo, prefabHierro
            );

            Vector3 pos = new Vector3(x, 0, tablero.height + alturaExtra);

            GameObject obj = Instantiate(prefab, pos, Quaternion.identity, root);

            // Desactivar interacción
            obj.GetComponent<Collider>().enabled = false;

            barraActual[x] = obj;
        }
    }

    // ============================================================
    // CAÍDA SUAVE
    // ============================================================

    public void BajarBarraHastaFila(int filaDestino)
    {
        StartCoroutine(CaidaSuave(filaDestino));
    }

    private IEnumerator CaidaSuave(int fila)
    {
        bool terminada = false;

        while (!terminada)
        {
            terminada = true;

            for (int x = 0; x < 6; x++)
            {
                if (barraActual[x] == null) continue;

                Vector3 objetivo = new Vector3(x, 0, fila);

                barraActual[x].transform.position = Vector3.MoveTowards(
                    barraActual[x].transform.position,
                    objetivo,
                    velocidadCaida * Time.deltaTime
                );

                if (Vector3.Distance(barraActual[x].transform.position, objetivo) > 0.01f)
                    terminada = false;
            }

            yield return null;
        }

        // Activar interacción
        for (int x = 0; x < 6; x++)
            barraActual[x].GetComponent<Collider>().enabled = true;

        // Integrar al tablero
        tablero.IntegrarBarra(barraActual);

        // Crear nueva barra
        CrearBarraExtra();
    }
}
