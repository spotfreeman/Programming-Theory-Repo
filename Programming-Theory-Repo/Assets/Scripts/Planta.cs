using UnityEngine;
using TMPro;

public class Planta : MonoBehaviour
{
    [Header("Configuración de la Planta")]
    [SerializeField] private int m_AguaMaxima = 20;
    [SerializeField] private int m_AguaInicial = 15;
    [SerializeField] private float m_TiempoConsumo = 3f;
    [SerializeField] private int m_ConsumoPorCiclo = 1;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoAguaPlanta;
    [SerializeField] private float m_AlturaTexto = 1.5f;

    private int m_AguaActual;

    // ENCAPSULATION: Protege el nivel de hidratación de la planta y actualiza la UI
    public int AguaActual
    {
        get { return m_AguaActual; }
        private set
        {
            m_AguaActual = Mathf.Clamp(value, 0, m_AguaMaxima);
            ActualizarUI(); // ABSTRACTION
        }
    }

    public int EspacioLibre => m_AguaMaxima - AguaActual;

    private float temporizador;

    private void Start()
    {
        PosicionarTextoUI();
        AguaActual = m_AguaInicial;
    }

    private void Update()
    {
        ConsumirAguaConElTiempo(); // ABSTRACTION
        PosicionarTextoUI();      // ABSTRACTION
    }

    // ABSTRACTION: La planta consume agua periódicamente
    private void ConsumirAguaConElTiempo()
    {
        if (AguaActual > 0)
        {
            temporizador += Time.deltaTime;
            if (temporizador >= m_TiempoConsumo)
            {
                AguaActual -= m_ConsumoPorCiclo;
                temporizador = 0f;
            }
        }
    }

    // ABSTRACTION: Recibe agua de un personaje y devuelve cuánta agua aceptó
    public int Regar(int cantidadDisponible)
    {
        int aguaAceptada = Mathf.Min(cantidadDisponible, EspacioLibre);
        AguaActual += aguaAceptada;
        return aguaAceptada;
    }

    private void ActualizarUI()
    {
        if (textoAguaPlanta != null)
        {
            if (AguaActual > 0)
            {
                textoAguaPlanta.text = $"Planta\n{AguaActual} / {m_AguaMaxima}";
            }
            else
            {
                textoAguaPlanta.text = "Planta\n¡SECA! (0)";
            }
        }
    }

    // ABSTRACTION: Ubica el texto del Canvas justo encima del objeto 3D en pantalla
    private void PosicionarTextoUI()
    {
        if (textoAguaPlanta != null && Camera.main != null)
        {
            textoAguaPlanta.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * m_AlturaTexto);
        }
    }
}
