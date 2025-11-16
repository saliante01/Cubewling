using UnityEngine;
using System.Collections;

public class BotonSalir : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    public void ExitGame()
    {
        StartCoroutine(PlaySoundAndExit());
    }

    private IEnumerator PlaySoundAndExit()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
            yield return new WaitForSeconds(clickSound.length);
        }

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
