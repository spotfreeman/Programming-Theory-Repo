using UnityEngine;
using TMPro;

public class Bodega : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoAguaBodega;

    private int m_AguaAlmacenada = 0;

    // ENCAPSULATION: Evita valores negativos y actualiza la UI automáticamente
    public int AguaAlmacenada
    {
        get { return m_AguaAlmacenada; }
        private set
        {
            m_AguaAlmacenada = Mathf.Max(0, value);
            ActualizarUI(); // ABSTRACTION
        }
    }

    private void Start()
    {
        ActualizarUI();
    }

    // ABSTRACTION: El personaje llama a este método para entregar el agua que lleva
    public void DepositarAgua(int cantidad)
    {
        if (cantidad > 0)
        {
            AguaAlmacenada += cantidad;
        }
    }

    // ABSTRACTION: Permite sacar agua de la bodega para llevarla a la Planta
    public int ExtraerAgua(int cantidadSolicitada)
    {
        int aguaExtraida = Mathf.Min(cantidadSolicitada, AguaAlmacenada);
        AguaAlmacenada -= aguaExtraida;
        return aguaExtraida;
    }

    private void ActualizarUI()
    {
        if (textoAguaBodega != null)
        {
            textoAguaBodega.text = $"Bodega: {AguaAlmacenada}";
        }
    }
}