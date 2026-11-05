using UnityEngine;
using TMPro; // Si usas Text (Legacy), cambia TMPro por UnityEngine.UI y TextMeshProUGUI por Text

public class Estanque : MonoBehaviour
{
    [Header("Configuración del Estanque")]
    [SerializeField] private int m_CapacidadMaxima = 20;
    [SerializeField] private float m_TiempoGeneracion = 2f;
    [SerializeField] private int m_AguaPorCiclo = 1;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoAguaEstanque;

    private int m_AguaDisponible = 0;

    // ENCAPSULATION: Protege el valor entre 0 y la capacidad máxima, y actualiza la UI al cambiar
    public int AguaDisponible
    {
        get { return m_AguaDisponible; }
        private set
        {
            m_AguaDisponible = Mathf.Clamp(value, 0, m_CapacidadMaxima);
            ActualizarUI(); // ABSTRACTION
        }
    }

    private float temporizador;

    private void Start()
    {
        ActualizarUI();
    }

    private void Update()
    {
        GenerarAguaConElTiempo(); // ABSTRACTION
    }

    // ABSTRACTION: Lógica de generación periódica de agua
    private void GenerarAguaConElTiempo()
    {
        if (AguaDisponible < m_CapacidadMaxima)
        {
            temporizador += Time.deltaTime;
            if (temporizador >= m_TiempoGeneracion)
            {
                AguaDisponible += m_AguaPorCiclo;
                temporizador = 0f;
            }
        }
    }

    // ABSTRACTION: El personaje llama a este método para sacar agua del estanque
    public int ExtraerAgua(int cantidadSolicitada)
    {
        int aguaExtraida = Mathf.Min(cantidadSolicitada, AguaDisponible);
        AguaDisponible -= aguaExtraida;
        return aguaExtraida;
    }

    private void ActualizarUI()
    {
        if (textoAguaEstanque != null)
        {
            textoAguaEstanque.text = $"Estanque: {AguaDisponible} / {m_CapacidadMaxima}";
        }
    }
}