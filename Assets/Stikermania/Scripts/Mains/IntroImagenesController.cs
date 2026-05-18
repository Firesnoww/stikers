using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroImagenesController : MonoBehaviour
{
    [Header("Imagen donde se mostrará la intro")]
    public Image imagenIntro;

    [Header("Sprites de la intro en orden")]
    public Sprite[] imagenesIntro;

    [Header("Botón para avanzar")]
    public Button botonSiguiente;

    [Header("Audio de la intro")]
    public AudioSource audioSource;
    public AudioClip sonidoIntro;
    [Range(0f, 1f)] public float volumenIntro = 1f;

    [Header("Escena a cargar al terminar")]
    public string nombreEscenaSiguiente = "Menu inicio";

    private int indiceActual = 0;
    private bool yaCambioEscena = false;

    void Start()
    {
        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        ConfigurarBoton();
        ReproducirSonidoIntro();
        MostrarImagenActual();
    }

    private void ConfigurarBoton()
    {
        if (botonSiguiente != null)
        {
            botonSiguiente.onClick.RemoveAllListeners();
            botonSiguiente.onClick.AddListener(AvanzarIntro);
        }
    }

    private void ReproducirSonidoIntro()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            Debug.LogWarning("No hay AudioSource asignado para la intro.");
            return;
        }

        if (sonidoIntro == null)
        {
            Debug.LogWarning("No hay AudioClip asignado para la intro.");
            return;
        }

        audioSource.clip = sonidoIntro;
        audioSource.volume = volumenIntro;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.Play();
    }

    public void AvanzarIntro()
    {
        if (yaCambioEscena)
        {
            return;
        }

        if (imagenesIntro == null || imagenesIntro.Length == 0)
        {
            CargarSiguienteEscena();
            return;
        }

        if (indiceActual < imagenesIntro.Length - 1)
        {
            indiceActual++;
            MostrarImagenActual();
        }
        else
        {
            CargarSiguienteEscena();
        }
    }

    private void MostrarImagenActual()
    {
        if (imagenesIntro == null || imagenesIntro.Length == 0)
        {
            Debug.LogWarning("No hay imágenes asignadas para la intro.");
            return;
        }

        if (imagenIntro != null)
        {
            imagenIntro.sprite = imagenesIntro[indiceActual];
        }
    }

    private void CargarSiguienteEscena()
    {
        yaCambioEscena = true;

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        SceneManager.LoadScene(nombreEscenaSiguiente);
    }
}