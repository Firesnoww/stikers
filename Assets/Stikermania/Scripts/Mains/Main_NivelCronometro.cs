using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Main_NivelCronometro : MonoBehaviour
{
    [Header("Tiempo del nivel")]
    public float tiempoLimite = 60f;

    [Header("Puntos que deben completarse")]
    public int puntosNecesarios = 3;

    [Header("Interfaz de tiempo")]
    public TMP_Text textoTiempo;

    [Header("Tutorial inicial")]
    public GameObject panelTutorial;
    public bool usarTutorialInicial = false;

    [Header("Menús finales")]
    public GameObject menuVictoria;
    public GameObject menuDerrota;

    [Header("Sonidos finales")]
    public AudioSource audioSource;
    public AudioClip sonidoVictoria;
    public AudioClip sonidoDerrota;

    [Header("Nombre de la siguiente escena")]
    public string nombreSiguienteEscena;

    [Header("Nombre de la escena del menú principal")]
    public string nombreMenuPrincipal = "MenuPrincipal";

    private float tiempoActual;
    private int puntosCompletados = 0;
    private bool nivelTerminado = false;
    private bool nivelIniciado = false;

    void Start()
    {
        Time.timeScale = 1f;

        tiempoActual = tiempoLimite;

        if (menuVictoria != null)
        {
            menuVictoria.SetActive(false);
        }

        if (menuDerrota != null)
        {
            menuDerrota.SetActive(false);
        }

        ActualizarTextoTiempo();

        if (usarTutorialInicial && panelTutorial != null)
        {
            MostrarTutorialInicial();
        }
        else
        {
            IniciarNivel();
        }
    }

    void Update()
    {
        if (!nivelIniciado)
        {
            return;
        }

        if (nivelTerminado)
        {
            return;
        }

        tiempoActual -= Time.deltaTime;

        if (tiempoActual <= 0)
        {
            tiempoActual = 0;
            PerderNivel();
        }

        ActualizarTextoTiempo();
    }

    private void MostrarTutorialInicial()
    {
        nivelIniciado = false;

        panelTutorial.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CerrarTutorialEIniciarNivel()
    {
        if (panelTutorial != null)
        {
            panelTutorial.SetActive(false);
        }

        IniciarNivel();
    }

    private void IniciarNivel()
    {
        nivelIniciado = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void RegistrarPuntoCompletado()
    {
        if (!nivelIniciado)
        {
            return;
        }

        if (nivelTerminado)
        {
            return;
        }

        puntosCompletados++;

        Debug.Log("Puntos completados: " + puntosCompletados + " / " + puntosNecesarios);

        if (puntosCompletados >= puntosNecesarios)
        {
            GanarNivel();
        }
    }

    private void GanarNivel()
    {
        nivelTerminado = true;

        if (menuVictoria != null)
        {
            menuVictoria.SetActive(true);
        }

        ReproducirSonidoVictoria();

        ActivarMouseParaMenu();

        Time.timeScale = 0f;

        Debug.Log("Nivel completado.");
    }

    private void PerderNivel()
    {
        nivelTerminado = true;

        if (menuDerrota != null)
        {
            menuDerrota.SetActive(true);
        }

        ReproducirSonidoDerrota();

        ActivarMouseParaMenu();

        Time.timeScale = 0f;

        Debug.Log("Tiempo agotado. Nivel perdido.");
    }

    private void ReproducirSonidoVictoria()
    {
        if (audioSource != null && sonidoVictoria != null)
        {
            audioSource.PlayOneShot(sonidoVictoria);
        }
    }

    private void ReproducirSonidoDerrota()
    {
        if (audioSource != null && sonidoDerrota != null)
        {
            audioSource.PlayOneShot(sonidoDerrota);
        }
    }

    private void ActivarMouseParaMenu()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ActualizarTextoTiempo()
    {
        if (textoTiempo == null)
        {
            return;
        }

        int minutos = Mathf.FloorToInt(tiempoActual / 60f);
        int segundos = Mathf.FloorToInt(tiempoActual % 60f);

        textoTiempo.text = minutos.ToString("00") + ":" + segundos.ToString("00");
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(nombreMenuPrincipal);
    }

    public void IrAlSiguienteNivel()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nombreSiguienteEscena))
        {
            SceneManager.LoadScene(nombreSiguienteEscena);
        }
        else
        {
            Debug.LogWarning("No hay nombre de siguiente escena asignado.");
        }
    }
}