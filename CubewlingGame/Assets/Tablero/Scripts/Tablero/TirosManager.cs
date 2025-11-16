using UnityEngine;

public class TirosManager : MonoBehaviour
{
    public static TirosManager Instance;

    public int tirosMaximos = 5;
    public int tirosActuales;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        tirosActuales = tirosMaximos;
    }

    public bool CanShoot()
    {
        return tirosActuales > 0;
    }

    public void ConsumirTiro()
    {
        if (tirosActuales > 0)
            tirosActuales--;
    }

    public void RecuperarTiro()
    {
        tirosActuales++;
        if (tirosActuales > tirosMaximos)
            tirosActuales = tirosMaximos;
    }
}
