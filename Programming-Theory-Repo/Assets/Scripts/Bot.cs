using UnityEngine;

// INHERITANCE: Bot hereda todas las propiedades y movimiento de Personaje
public class Bot : Personaje
{
    private enum RutaBot
    {
        Ninguna,
        EstanqueABodega,
        BodegaAPlanta
    }

    [Header("Referencias del Bot")]
    [SerializeField] private Estanque estanqueObjetivo;
    [SerializeField] private Bodega bodegaObjetivo;
    [SerializeField] private Planta plantaObjetivo;

    private RutaBot rutaActual = RutaBot.Ninguna;

    protected override void Start()
    {
        base.Start();

        // Si no se asignaron en el Inspector, los busca automáticamente en la escena
        if (estanqueObjetivo == null) estanqueObjetivo = FindObjectOfType<Estanque>();
        if (bodegaObjetivo == null) bodegaObjetivo = FindObjectOfType<Bodega>();
        if (plantaObjetivo == null) plantaObjetivo = FindObjectOfType<Planta>();
    }

    // POLYMORPHISM: Si llega al Estanque, queda asignado a la ruta Estanque <-> Bodega
    protected override void RecolectarAgua(Estanque estanque)
    {
        rutaActual = RutaBot.EstanqueABodega;
        base.RecolectarAgua(estanque);

        if (AguaCargada > 0 && bodegaObjetivo != null)
        {
            IrHacia(bodegaObjetivo.transform.position);
        }
    }

    // POLYMORPHISM: Si llega a la Planta, queda asignado a la ruta Bodega <-> Planta
    protected override void RegarPlanta(Planta planta)
    {
        rutaActual = RutaBot.BodegaAPlanta;
        base.RegarPlanta(planta);

        // Cuando se queda sin agua (o la planta ya está llena), vuelve a la Bodega a buscar más
        if (AguaCargada == 0 && bodegaObjetivo != null)
        {
            IrHacia(bodegaObjetivo.transform.position);
        }
    }

    // POLYMORPHISM: En la Bodega actúa según la ruta en la que lo dejó trabajando el jugador
    protected override void InteractuarConBodega(Bodega bodega)
    {
        if (rutaActual == RutaBot.BodegaAPlanta)
        {
            // En la ruta Bodega <-> Planta: saca agua de la Bodega y la lleva a la Planta
            int espacioLibre = m_CapacidadAgua - AguaCargada;
            if (espacioLibre > 0)
            {
                AguaCargada += bodega.ExtraerAgua(espacioLibre);
            }

            if (AguaCargada > 0 && plantaObjetivo != null)
            {
                IrHacia(plantaObjetivo.transform.position);
            }
        }
        else
        {
            // En la ruta Estanque <-> Bodega: deposita el agua en la Bodega y vuelve al Estanque
            rutaActual = RutaBot.EstanqueABodega;
            if (AguaCargada > 0)
            {
                bodega.DepositarAgua(AguaCargada);
                AguaCargada = 0;
            }

            if (estanqueObjetivo != null)
            {
                IrHacia(estanqueObjetivo.transform.position);
            }
        }
    }

    // Si está esperando en un edificio porque no había agua o espacio, reintenta apenas haya
    private void OnCollisionStay(Collision collision)
    {
        if (tieneDestino) return;

        if (rutaActual == RutaBot.EstanqueABodega && AguaCargada == 0)
        {
            if (collision.gameObject.TryGetComponent<Estanque>(out Estanque estanque) && estanque.AguaDisponible > 0)
            {
                RecolectarAgua(estanque);
            }
        }
        else if (rutaActual == RutaBot.BodegaAPlanta)
        {
            if (AguaCargada == 0 && collision.gameObject.TryGetComponent<Bodega>(out Bodega bodega) && bodega.AguaAlmacenada > 0)
            {
                InteractuarConBodega(bodega);
            }
            else if (AguaCargada > 0 && collision.gameObject.TryGetComponent<Planta>(out Planta planta) && planta.EspacioLibre > 0)
            {
                RegarPlanta(planta);
            }
        }
    }

    // ABSTRACTION: Fija un nuevo destino para el Bot manteniendo su altura Y
    private void IrHacia(Vector3 posicionObjetivo)
    {
        destino = new Vector3(posicionObjetivo.x, transform.position.y, posicionObjetivo.z);
        tieneDestino = true;
    }
}
