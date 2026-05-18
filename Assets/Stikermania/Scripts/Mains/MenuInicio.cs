using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicio : MonoBehaviour
{
    [Header("Escenas")]
    public string nombreNivel1 = "Nivel_1";

    [Header("Interfaces")]
    public GameObject panelMenuPrincipal;
    public GameObject panelCreditos;

    void Start()
    {
        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        MostrarMenuPrincipal();
    }

    public void IniciarJuego()
    {
        SceneManager.LoadScene(nombreNivel1);
    }

    public void AbrirCreditos()
    {
        if (panelMenuPrincipal != null)
        {
            panelMenuPrincipal.SetActive(false);
        }

        if (panelCreditos != null)
        {
            panelCreditos.SetActive(true);
        }
    }

    public void CerrarCreditos()
    {
        MostrarMenuPrincipal();
    }

    public void MostrarMenuPrincipal()
    {
        if (panelMenuPrincipal != null)
        {
            panelMenuPrincipal.SetActive(true);
        }

        if (panelCreditos != null)
        {
            panelCreditos.SetActive(false);
        }
    }

    public void SalirDelJuego()
    {
        Debug.Log("Salir del juego");

        Application.Quit();
    }
}