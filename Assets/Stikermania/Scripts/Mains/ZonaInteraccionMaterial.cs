using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ZonaInteraccionMaterial : MonoBehaviour
{
    [Header("Material del Shader Graph")]
    public Material materialObjetivo;

    [Header("Nombre de la propiedad de color del Shader Graph")]
    public string nombreColor = "_Color";

    [Header("Color inicial del material")]
    [ColorUsage(true, true)]
    public Color colorInicial = Color.white;

    [Header("Color cuando el jugador entra")]
    [ColorUsage(true, true)]
    public Color colorAlEntrar = Color.cyan;

    [Header("Duración del cambio de color")]
    public float duracionCambioColor = 0.5f;

    [Header("Objetos que se activan al presionar E")]
    public GameObject[] objetosAEncender;

    [Header("Efectos al completar")]
    public AudioSource audioSource;
    public AudioClip sonidoCompletar;
    public ParticleSystem particulasCompletar;

    [Header("Controlador del nivel")]
    public Main_NivelCronometro controladorNivel;

    private bool jugadorDentro = false;
    private bool yaFueUsado = false;

    private Coroutine rutinaColor;

    void Start()
    {
        CambiarColorInmediato(colorInicial);
        ApagarObjetosIniciales();
    }

    void Update()
    {
        if (jugadorDentro && !yaFueUsado && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ActivarUnaSolaVez();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !yaFueUsado)
        {
            jugadorDentro = true;
            CambiarColorSuave(colorAlEntrar);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !yaFueUsado)
        {
            jugadorDentro = false;
            CambiarColorSuave(colorInicial);
        }
    }

    private void ActivarUnaSolaVez()
    {
        yaFueUsado = true;
        jugadorDentro = false;

        CambiarColorSuave(colorAlEntrar);

        EncenderObjetos();

        ReproducirSonido();

        ReproducirParticulas();

        if (controladorNivel != null)
        {
            controladorNivel.RegistrarPuntoCompletado();
        }

        Debug.Log("Interacción completada. Esta zona ya no se puede volver a usar.");
    }

    private void ApagarObjetosIniciales()
    {
        for (int i = 0; i < objetosAEncender.Length; i++)
        {
            if (objetosAEncender[i] != null)
            {
                objetosAEncender[i].SetActive(false);
            }
        }
    }

    private void EncenderObjetos()
    {
        for (int i = 0; i < objetosAEncender.Length; i++)
        {
            if (objetosAEncender[i] != null)
            {
                objetosAEncender[i].SetActive(true);
            }
        }
    }

    private void ReproducirSonido()
    {
        if (audioSource != null && sonidoCompletar != null)
        {
            audioSource.PlayOneShot(sonidoCompletar);
        }
    }

    private void ReproducirParticulas()
    {
        if (particulasCompletar != null)
        {
            particulasCompletar.Play();
        }
    }

    private void CambiarColorInmediato(Color nuevoColor)
    {
        if (materialObjetivo != null)
        {
            materialObjetivo.SetColor(nombreColor, nuevoColor);
        }
    }

    private void CambiarColorSuave(Color colorDestino)
    {
        if (materialObjetivo == null)
        {
            Debug.LogWarning("No hay material asignado en ZonaInteraccionMaterial.");
            return;
        }

        if (rutinaColor != null)
        {
            StopCoroutine(rutinaColor);
        }

        rutinaColor = StartCoroutine(AnimarCambioColor(colorDestino));
    }

    private IEnumerator AnimarCambioColor(Color colorDestino)
    {
        Color colorInicio = materialObjetivo.GetColor(nombreColor);

        float tiempo = 0f;

        while (tiempo < duracionCambioColor)
        {
            tiempo += Time.deltaTime;

            float progreso = tiempo / duracionCambioColor;

            Color colorActual = Color.Lerp(colorInicio, colorDestino, progreso);

            materialObjetivo.SetColor(nombreColor, colorActual);

            yield return null;
        }

        materialObjetivo.SetColor(nombreColor, colorDestino);
    }
}