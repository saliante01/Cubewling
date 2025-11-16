using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class VentanaPrincipalBoton : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private AudioClip clickSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    public void LoadScene()
    {
        StartCoroutine(PlaySoundAndLoad());
    }

    private IEnumerator PlaySoundAndLoad()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
            yield return new WaitForSeconds(clickSound.length);
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
        else
            Debug.LogWarning("No has asignado ninguna escena en el Inspector.");
    }
}
