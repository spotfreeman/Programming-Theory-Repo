using UnityEngine;

public class Personaje : MonoBehaviour
{
    // ENCAPSULATION: Referencia estática privada con propiedad pública de solo lectura externa
    private static Personaje s_PersonajeSeleccionado;
    public static Personaje PersonajeSeleccionado
    {
        get { return s_PersonajeSeleccionado; }
        private set { s_PersonajeSeleccionado = value; }
    }

    // ENCAPSULATION: Velocidad protegida con validación en el setter
    [SerializeField] private float m_Velocidad = 5f;
    public float Velocidad
    {
        get { return m_Velocidad; }
        set { m_Velocidad = Mathf.Max(0f, value); }
    }

    // ENCAPSULATION: Capacidad y agua que lleva actualmente el personaje
    [SerializeField] protected int m_CapacidadAgua = 5;
    private int m_AguaCargada = 0;

    public int AguaCargada
    {
        get { return m_AguaCargada; }
        protected set { m_AguaCargada = Mathf.Clamp(value, 0, m_CapacidadAgua); }
    }

    protected Vector3 destino;
    protected bool tieneDestino = false;
    protected Rigidbody rb;

    protected virtual void Start()
    {
        destino = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update()
    {
        if (PersonajeSeleccionado == this)
        {
            DetectarClickDestino(); // ABSTRACTION
        }
    }

    protected virtual void FixedUpdate()
    {
        if (tieneDestino)
        {
            Mover(); // ABSTRACTION & POLYMORPHISM
        }
        else if (rb != null)
        {
            // Evita que el personaje se quede deslizándose solo por fuerzas residuales
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void OnMouseDown()
    {
        SeleccionarPersonaje(); // ABSTRACTION
    }

    protected virtual void SeleccionarPersonaje()
    {
        PersonajeSeleccionado = this;
        Debug.Log("Personaje seleccionado: " + gameObject.name);
    }

    private void DetectarClickDestino()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray rayo = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(rayo, out RaycastHit hit))
            {
                destino = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                tieneDestino = true;
            }
        }
    }

    // POLYMORPHISM: "virtual" permite que las clases hijas cambien cómo se mueven
    protected virtual void Mover()
    {
        Vector3 nuevaPosicion = Vector3.MoveTowards(transform.position, destino, Velocidad * Time.fixedDeltaTime);

        if (rb != null)
        {
            rb.MovePosition(nuevaPosicion);
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            transform.position = nuevaPosicion;
        }

        if (Vector3.Distance(transform.position, destino) < 0.05f)
        {
            tieneDestino = false;
        }
    }

    // Si choca con otro objeto que no sea el suelo, frena e interactúa con Estanque, Bodega o Planta
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Ground") && collision.gameObject.name != "Plane")
        {
            tieneDestino = false;

            // Si choca con el Estanque, extrae agua
            if (collision.gameObject.TryGetComponent<Estanque>(out Estanque estanque))
            {
                RecolectarAgua(estanque); // ABSTRACTION
            }
            // Si choca con la Bodega, deposita o retira agua
            else if (collision.gameObject.TryGetComponent<Bodega>(out Bodega bodega))
            {
                InteractuarConBodega(bodega); // ABSTRACTION
            }
            // Si choca con la Planta, la riega con el agua que lleva
            else if (collision.gameObject.TryGetComponent<Planta>(out Planta planta))
            {
                RegarPlanta(planta); // ABSTRACTION
            }
        }
    }

    // ABSTRACTION & POLYMORPHISM: Permite recoger agua del estanque
    protected virtual void RecolectarAgua(Estanque estanque)
    {
        int espacioLibre = m_CapacidadAgua - AguaCargada;
        if (espacioLibre > 0)
        {
            int extraida = estanque.ExtraerAgua(espacioLibre);
            AguaCargada += extraida;
            Debug.Log($"{gameObject.name} recolectó {extraida} de agua del Estanque. Lleva: {AguaCargada}/{m_CapacidadAgua}");
        }
    }

    // ABSTRACTION & POLYMORPHISM: Si lleva agua la deposita en la bodega; si va vacío, saca agua de la bodega
    protected virtual void InteractuarConBodega(Bodega bodega)
    {
        if (AguaCargada > 0)
        {
            bodega.DepositarAgua(AguaCargada);
            Debug.Log($"{gameObject.name} depositó {AguaCargada} de agua en la Bodega.");
            AguaCargada = 0;
        }
        else
        {
            int extraida = bodega.ExtraerAgua(m_CapacidadAgua);
            AguaCargada += extraida;
            Debug.Log($"{gameObject.name} retiró {extraida} de agua de la Bodega. Lleva: {AguaCargada}/{m_CapacidadAgua}");
        }
    }

    // ABSTRACTION & POLYMORPHISM: Riega la planta con el agua que lleva el personaje
    protected virtual void RegarPlanta(Planta planta)
    {
        if (AguaCargada > 0)
        {
            int usada = planta.Regar(AguaCargada);
            AguaCargada -= usada;
            Debug.Log($"{gameObject.name} regó la Planta con {usada} de agua. Le queda: {AguaCargada}/{m_CapacidadAgua}");
        }
    }
}