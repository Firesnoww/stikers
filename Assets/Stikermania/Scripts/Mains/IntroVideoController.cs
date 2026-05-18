using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.InputSystem;

public class IntroVideoController : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;

    [Header("Audio separado")]
    public AudioSource audioSource;

    [Header("Escena siguiente")]
    public string nombreEscenaSiguiente = "Menu inicio";

    [Header("Opciones")]
    public bool permitirSaltar = true;

    private bool yaInicio = false;
    private bool yaCambioEscena = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (videoPlayer == null)
        {
            Debug.LogWarning("No hay VideoPlayer asignado.");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogWarning("No hay AudioSource asignado.");
            return;
        }

        // El video no manejará audio.
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        // Configuración segura.
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // Cuando el video termina, pasa a la siguiente escena.
        videoPlayer.loopPointReached += TerminarIntro;

        // Primero prepara el video.
        videoPlayer.prepareCompleted += IniciarVideoYAudio;
        videoPlayer.Prepare();
    }

    void Update()
    {
        if (yaCambioEscena)
        {
            return;
        }

        if (permitirSaltar && Keyboard.current != null)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                CambiarEscena();
            }
        }

        // Seguridad extra: si el audio termina y el video por alguna razón no dispara el evento,
        // también puede pasar al menú.
        if (yaInicio && !videoPlayer.isPlaying && !audioSource.isPlaying && !yaCambioEscena)
        {
            CambiarEscena();
        }
    }

    private void IniciarVideoYAudio(VideoPlayer vp)
    {
        if (yaInicio)
        {
            return;
        }

        yaInicio = true;

        // Reinicia ambos desde el inicio.
        videoPlayer.time = 0;
        audioSource.time = 0;

        // Reproduce los dos casi al mismo tiempo.
        videoPlayer.Play();
        audioSource.Play();
    }

    private void TerminarIntro(VideoPlayer vp)
    {
        CambiarEscena();
    }

    private void CambiarEscena()
    {
        if (yaCambioEscena)
        {
            return;
        }

        yaCambioEscena = true;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(nombreEscenaSiguiente);
    }
}