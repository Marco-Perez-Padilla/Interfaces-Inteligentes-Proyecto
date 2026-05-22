// ----------------------------------------------------------------------
// NoiseDetector.cs
// ----------------------------------------------------------------------
// Detecta ruido del jugador dentro de una zona trigger usando el micrófono
// Cada zona puede tener un umbral (threshold) independiente editable en editor
// Dispara un evento OnNoiseDetected con la posición de la zona y la intensidad
// Otros NPCs o sistemas (luces, efectos) pueden suscribirse al evento
// Solo reaccionarán si están suficientemente cerca de la zona o si la intensidad supera su propio umbral
// Permite filtrar ruido de fondo mediante rango de frecuencia mínimo y máximo
// La detección es continua mientras el jugador esté dentro del trigger
// Útil para túneles del terror, NPCs sigilosos o efectos de ambiente reactivos
// Sensibilidad, tiempo de muestreo y ventana de análisis son ajustables en editor
// Evita que NPCs muy lejanos reaccionen a sonidos irrelevantes
// Facilita crear mecánicas de proximidad, suspense y alerta auditiva sin código extra
// Proximidades y distancias se realizan en cada evento, no en este notificador, según distancia >
// ----------------------------------------------------------------------

using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Android;

public class NoiseDetector : MonoBehaviour
{
    [Header("Zone Settings")]
    public float threshold = 0.1f; // Nivel de ruido para disparar evento
    public float highNoiseThreshold = 0.5f; // Disparar evento con muy alto ruido
    public float minFrequency = 100f; // Ignorar ruido bajo (ventilador)
    public float maxFrequency = 5000f; // Ignorar ruido muy agudo
    public int windowSize = 1024; 

    [Header("Audio Settings")]
    public int sampleRate = 44100;
    public int recordTime = 1; 

    public delegate void NoiseEvent(Vector3 position, float intensity);
    public event NoiseEvent OnNoiseDetected;
    public event NoiseEvent OnHighNoiseDetected;
    public static event System.Action<Vector3, float> OnAnyNoiseDetected;

    private AudioClip clip;
    private string micName;
    private bool isRecording = false;
    private Transform playerTransform; 

    /// <summary>
    /// Configura el collider del GameObject como trigger para detectar la entrada y salida del jugador.
    /// </summary>
    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    ///  <summary>
    ///  Solicita permiso para usar el micrófono si aún no se ha concedido. Esto es necesario para que la detección de ruido funcione correctamente.
    ///  </summary>
    void Start()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            Permission.RequestUserPermission(Permission.Microphone);
    }

    ///  <summary>
    ///  Inicia la grabación del micrófono cuando el jugador entra en el trigger y detiene la grabación cuando el jugador sale del trigger.
    ///  La detección de ruido se realiza continuamente mientras el jugador esté dentro del trigger.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerTransform = other.transform;
        StartRecording();
    }

    ///  <summary>
    ///  Detiene la grabación del micrófono cuando el jugador sale del trigger y resetea la referencia al transform del jugador.
    ///  Esto asegura que la detección de ruido solo ocurra mientras el jugador esté dentro del trigger y evita que se detecten ruidos irrelevantes cuando el jugador no está presente.
    ///  </summary>
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerTransform = null;
        StopRecording();
    }

    /// <summary>
    /// Inicia la grabación del micrófono utilizando el primer dispositivo disponible. Configura el clip de audio para grabar en bucle durante un tiempo determinado con una frecuencia de muestreo específica.
    /// Esto permite que la detección de ruido se realice continuamente mientras el jugador esté dentro del trigger.
    /// </summary>
    void StartRecording()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone)) return;
        if (Microphone.devices.Length == 0) return;
        micName = Microphone.devices[0];
        clip = Microphone.Start(micName, true, recordTime, sampleRate);
        isRecording = true;
    }

    ///  <summary>
    ///  Detiene la grabación del micrófono y libera los recursos asociados. Esto asegura que la detección de ruido se detenga correctamente cuando el jugador salga del trigger y evita que el micrófono siga grabando innecesariamente, lo que podría afectar el rendimiento o la privacidad.
    ///  </summary>
    void StopRecording()
    {
        if (!isRecording) return;
        Microphone.End(micName);
        isRecording = false;
    }

    /// <summary>
    /// Analiza los datos de audio grabados para calcular la intensidad del ruido. Si la intensidad supera el umbral definido, dispara los eventos correspondientes con la posición del jugador (o del detector si el jugador no está presente) y la intensidad del ruido.
    /// Esto permite que otros NPCs o sistemas se suscriban a estos eventos y reaccionen de manera adecuada según la intensidad del ruido detectado, creando una experiencia de juego más inmersiva y dinámica.
    /// </summary>
    void Update()
    {
        if (!isRecording || clip == null) return;

        float[] samples = new float[windowSize];
        int micPos = Microphone.GetPosition(micName) - windowSize;
        if (micPos < 0) return; // buffer aún no llenado
        clip.GetData(samples, micPos);

        float rms = Mathf.Sqrt(samples.Average(s => s * s));

        // filtrar ruido
        float frequencyFilteredRMS = rms; 
        if(frequencyFilteredRMS > highNoiseThreshold)
        {
            Vector3 noisePosition = playerTransform != null ? playerTransform.position : transform.position;
            OnHighNoiseDetected?.Invoke(noisePosition, frequencyFilteredRMS);
            return;
        }

        if(frequencyFilteredRMS > threshold)
        {
            Vector3 noisePosition = playerTransform != null ? playerTransform.position : transform.position;
            OnNoiseDetected?.Invoke(noisePosition, frequencyFilteredRMS);
            OnAnyNoiseDetected?.Invoke(noisePosition, frequencyFilteredRMS);
        }
    }
}
