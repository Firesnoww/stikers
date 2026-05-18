using UnityEngine;

public class MusicaFondo : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Canción de fondo")]
    public AudioClip musicaFondo;

    [Header("Configuración")]
    [Range(0f, 1f)]
    public float volumen = 0.5f;

    public bool reproducirAlIniciar = true;

    void Start()
    {
        ConfigurarMusica();

        if (reproducirAlIniciar)
        {
            ReproducirMusica();
        }
    }

    private void ConfigurarMusica()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            Debug.LogWarning("No hay AudioSource asignado para la música de fondo.");
            return;
        }

        audioSource.clip = musicaFondo;
        audioSource.volume = volumen;

        // Esto hace que la música se repita constantemente
        audioSource.loop = true;

        // Evita que suene sola si no queremos
        audioSource.playOnAwake = false;
    }

    public void ReproducirMusica()
    {
        if (audioSource == null || musicaFondo == null)
        {
            Debug.LogWarning("Falta AudioSource o AudioClip de música.");
            return;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void DetenerMusica()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public void PausarMusica()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
        }
    }

    public void ReanudarMusica()
    {
        if (audioSource != null)
        {
            audioSource.UnPause();
        }
    }

    public void CambiarVolumen(float nuevoVolumen)
    {
        volumen = Mathf.Clamp01(nuevoVolumen);

        if (audioSource != null)
        {
            audioSource.volume = volumen;
        }
    }
}