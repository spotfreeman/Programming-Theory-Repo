using UnityEngine;

// INHERITANCE: Bot hereda todas las propiedades y movimiento de Personaje
public class Bot : Personaje
{
    public enum RutaBot
    {
        Ninguna,
        EstanqueABodega,
        BodegaAPlanta,
        EstanqueAPlanta
    }

    [Header("Configuración de Ruta")]
    [SerializeField] private RutaBot rutaActual = RutaBot.Ninguna;

    [Header("Referencias del Bot")]
    [SerializeField] private Estanque estanqueObjetivo;
    [SerializeField] private Bodega bodegaObjetivo;
    [SerializeField] private Planta plantaObjetivo;

    // Guarda el último edificio visitado o clickeado para armar rutas combinadas (ej. Estanque -> Planta)
    private GameObject ultimoEdificioObjetivo;

    protected override void Start()
    {
        base.Start();

        // Si no se asignaron en el Inspector, los busca automáticamente en la escena
        if (estanqueObjetivo == null) estanqueObjetivo = FindObjectOfType<Estanque>();
        if (bodegaObjetivo == null) bodegaObjetivo = FindObjectOfType<Bodega>();
        if (plantaObjetivo == null) plantaObjetivo = FindObjectOfType<Planta>();
    }

    // POLYMORPHISM: Detecta sobre qué edificio hizo clic derecho el jugador para definir la ruta del Bot
    protected override void DetectarClickDestino()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray rayo = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(rayo, out RaycastHit hit))
            {
                destino = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                tieneDestino = true;

                ActualizarRutaSegunClick(hit.collider.gameObject);
            }
        }
    }

    // ABSTRACTION: Determina cuál de las 3 rutas hará el Bot según los edificios clickeados
    private void ActualizarRutaSegunClick(GameObject objetoClickeado)
    {
        bool clickEstanque = objetoClickeado.GetComponent<Estanque>() != null;
        bool clickBodega = objetoClickeado.GetComponent<Bodega>() != null;
        bool clickPlanta = objetoClickeado.GetComponent<Planta>() != null;

        if (!clickEstanque && !clickBodega && !clickPlanta) return;

        if (ultimoEdificioObjetivo != null && ultimoEdificioObjetivo != objetoClickeado)
        {
            bool anteriorEstanque = ultimoEdificioObjetivo.GetComponent<Estanque>() != null;
            bool anteriorBodega = ultimoEdificioObjetivo.GetComponent<Bodega>() != null;
            bool anteriorPlanta = ultimoEdificioObjetivo.GetComponent<Planta>() != null;

            if ((anteriorEstanque && clickPlanta) || (anteriorPlanta && clickEstanque))
            {
                rutaActual = RutaBot.EstanqueAPlanta;
            }
            else if ((anteriorEstanque && clickBodega) || (anteriorBodega && clickEstanque))
            {
                rutaActual = RutaBot.EstanqueABodega;
            }
            else if ((anteriorBodega && clickPlanta) || (anteriorPlanta && clickBodega))
            {
                rutaActual = RutaBot.BodegaAPlanta;
            }
        }
        else if (rutaActual == RutaBot.Ninguna)
        {
            // Ruta por defecto si es el primer clic
            if (clickEstanque || clickBodega) rutaActual = RutaBot.EstanqueABodega;
            else if (clickPlanta) rutaActual = RutaBot.BodegaAPlanta;
        }

        ultimoEdificioObjetivo = objetoClickeado;
        Debug.Log($"Ruta del Bot actualizada a: {rutaActual}");
    }

    // POLYMORPHISM: Cuando recolecta agua en el Estanque, va a la Planta o a la Bodega según su ruta
    protected override void RecolectarAgua(Estanque estanque)
    {
        ultimoEdificioObjetivo = estanque.gameObject;
        if (rutaActual == RutaBot.Ninguna) rutaActual = RutaBot.EstanqueABodega;

        base.RecolectarAgua(estanque);

        if (AguaCargada > 0)
        {
            if (rutaActual == RutaBot.EstanqueAPlanta && plantaObjetivo != null)
            {
                IrHacia(plantaObjetivo.transform.position);
            }
            else if (bodegaObjetivo != null)
            {
                rutaActual = RutaBot.EstanqueABodega;
                IrHacia(bodegaObjetivo.transform.position);
            }
        }
    }

    // POLYMORPHISM: Cuando riega la Planta, vuelve al Estanque o a la Bodega según su ruta
    protected override void RegarPlanta(Planta planta)
    {
        ultimoEdificioObjetivo = planta.gameObject;
        if (rutaActual == RutaBot.Ninguna) rutaActual = RutaBot.BodegaAPlanta;

        base.RegarPlanta(planta);

        if (AguaCargada == 0)
        {
            if (rutaActual == RutaBot.EstanqueAPlanta && estanqueObjetivo != null)
            {
                IrHacia(estanqueObjetivo.transform.position);
            }
            else if (bodegaObjetivo != null)
            {
                rutaActual = RutaBot.BodegaAPlanta;
                IrHacia(bodegaObjetivo.transform.position);
            }
        }
    }

    // POLYMORPHISM: En la Bodega actúa según la ruta activa (Estanque <-> Bodega o Bodega <-> Planta)
    protected override void InteractuarConBodega(Bodega bodega)
    {
        ultimoEdificioObjetivo = bodega.gameObject;

        if (rutaActual == RutaBot.BodegaAPlanta)
        {
            // Saca agua de la Bodega y la lleva a la Planta
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
            // Deposita el agua en la Bodega y vuelve al Estanque
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

        if ((rutaActual == RutaBot.EstanqueABodega || rutaActual == RutaBot.EstanqueAPlanta) && AguaCargada == 0)
        {
            if (collision.gameObject.TryGetComponent<Estanque>(out Estanque estanque) && estanque.AguaDisponible > 0)
            {
                RecolectarAgua(estanque);
            }
        }
        else if (rutaActual == RutaBot.BodegaAPlanta && AguaCargada == 0)
        {
            if (collision.gameObject.TryGetComponent<Bodega>(out Bodega bodega) && bodega.AguaAlmacenada > 0)
            {
                InteractuarConBodega(bodega);
            }
        }

        if ((rutaActual == RutaBot.BodegaAPlanta || rutaActual == RutaBot.EstanqueAPlanta) && AguaCargada > 0)
        {
            if (collision.gameObject.TryGetComponent<Planta>(out Planta planta) && planta.EspacioLibre > 0)
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
