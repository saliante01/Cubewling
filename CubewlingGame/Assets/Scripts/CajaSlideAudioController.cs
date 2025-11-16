using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CajaSlideAudioController : MonoBehaviour
{
    [Header("Sonidos de deslizamiento")]
    public AudioClip deslizarEntrada;
    public AudioClip deslizarLoop;
    public AudioClip deslizarSalida;

    [Header("Configuración")]
    public float velocidadLoopThreshold = 0.6f;  // Velocidad mínima donde suena el LOOP
    public float velocidadSalidaObjective = 0.1f; // Meta usada para pitch dinámico del "salida"
    public float volumenEntrada = 1f;
    public float volumenLoop = 1f;
    public float volumenSalida = 1f;

    private Rigidbody rb;
    private AudioSource loopSource;
    private bool entradaReproducida = false;
    private bool salidaReproducida = false;

    private bool lanzada = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Fuente exclusiva para el loop (no molestar otros audios)
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.loop = true;
        loopSource.playOnAwake = false;
    }

    public void OnLaunch(float fuerzaLanzamiento)
    {
        lanzada = true;
        entradaReproducida = false;
        salidaReproducida = false;

        // === 1. Sonido de ENTRADA ===
        if (deslizarEntrada != null)
        {
            float pitch = Mathf.Lerp(0.85f, 1.25f, Mathf.InverseLerp(0f, 20f, fuerzaLanzamiento));
            AudioSource.PlayClipAtPoint(deslizarEntrada, transform.position, volumenEntrada * pitch);
        }

        entradaReproducida = true;
    }

    private void Update()
    {
        if (!lanzada)
            return;

        float vel = rb.linearVelocity.magnitude;

        // === 2. Lógica del LOOP ===
        if (vel >= velocidadLoopThreshold)
        {
            if (!loopSource.isPlaying)
            {
                loopSource.clip = deslizarLoop;
                loopSource.volume = volumenLoop;
                loopSource.pitch = Mathf.Lerp(0.9f, 1.2f, vel / 10f);
                loopSource.Play();
            }
            else
            {
                // Ajustar pitch dinámico mientras corre
                loopSource.pitch = Mathf.Lerp(0.9f, 1.2f, vel / 10f);
            }
        }
        else
        {
            // Si estaba sonando el loop y bajó la velocidad → detener el loop y reproducir salida
            if (loopSource.isPlaying)
                loopSource.Stop();

            if (!salidaReproducida)
                ReproducirSalida(vel);
        }
    }

    private void ReproducirSalida(float velocidadActual)
    {
        salidaReproducida = true;

        if (deslizarSalida == null)
            return;

        // Calcular pitch dinámico según qué tan cerca está de detenerse
        float t = Mathf.InverseLerp(velocidadLoopThreshold, velocidadSalidaObjective, velocidadActual);
        float pitch = Mathf.Lerp(1.2f, 0.8f, t);

        AudioSource.PlayClipAtPoint(deslizarSalida, transform.position, volumenSalida * pitch);
    }

    public void OnReset()
    {
        lanzada = false;

        if (loopSource.isPlaying)
            loopSource.Stop();
    }
}
