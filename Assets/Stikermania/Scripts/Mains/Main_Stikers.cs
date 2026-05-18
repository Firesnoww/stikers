using UnityEngine;

public class Main_Stikers : MonoBehaviour
{
    [Header("Objetos físicos de la escena")]
    public GameObject[] objetosEscena;

    [Header("Iconos o imágenes UI")]
    public GameObject[] iconosUI;

    [Header("Cantidad de pares que se activan al iniciar")]
    public int cantidadAActivar = 3;

    void Start()
    {
        ActivarParesAleatorios();
    }

    public void ActivarParesAleatorios()
    {
        // Primero apagamos todos los objetos de escena
        for (int i = 0; i < objetosEscena.Length; i++)
        {
            if (objetosEscena[i] != null)
            {
                objetosEscena[i].SetActive(false);
            }
        }

        // Luego apagamos todos los iconos UI
        for (int i = 0; i < iconosUI.Length; i++)
        {
            if (iconosUI[i] != null)
            {
                iconosUI[i].SetActive(false);
            }
        }

        // Tomamos la cantidad real de pares posibles.
        // Se usa el menor tamaño para evitar errores si un array tiene más elementos que el otro.
        int totalPares = Mathf.Min(objetosEscena.Length, iconosUI.Length);

        // Evita intentar activar más pares de los que existen.
        int cantidadFinal = Mathf.Min(cantidadAActivar, totalPares);

        // Creamos un array con las posiciones disponibles.
        int[] posiciones = new int[totalPares];

        for (int i = 0; i < totalPares; i++)
        {
            posiciones[i] = i;
        }

        // Mezclamos las posiciones aleatoriamente.
        for (int i = 0; i < totalPares; i++)
        {
            int posicionAleatoria = Random.Range(i, totalPares);

            int temporal = posiciones[i];
            posiciones[i] = posiciones[posicionAleatoria];
            posiciones[posicionAleatoria] = temporal;
        }

        // Activamos solamente la cantidad de pares elegida.
        for (int i = 0; i < cantidadFinal; i++)
        {
            int indiceElegido = posiciones[i];

            if (objetosEscena[indiceElegido] != null)
            {
                objetosEscena[indiceElegido].SetActive(true);
            }

            if (iconosUI[indiceElegido] != null)
            {
                iconosUI[indiceElegido].SetActive(true);
            }
        }
    }
}