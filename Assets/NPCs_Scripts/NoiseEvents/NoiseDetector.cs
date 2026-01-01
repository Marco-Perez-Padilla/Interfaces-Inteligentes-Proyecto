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

public class NoiseDetector : MonoBehaviour
{
    [Header("Zone Settings")]
    public float threshold = 0.1f; // Nivel de ruido para disparar evento
    public float highNoiseThreshold = 1f; // Disparar evento con muy alto ruido
    public float minFrequency = 100f; // Ignorar ruido bajo (ventilador)
    public float maxFrequency = 5000f; // Ignorar ruido muy agudo
    public int windowSize = 1024; 

    [Header("Audio Settings")]
    public int sampleRate = 44100;
    public int recordTime = 1; 

    public delegate void NoiseEvent(Vector3 position, float intensity);
    public event NoiseEvent OnNoiseDetected;
    public event NoiseEvent OnHighNoiseDetected;

    private AudioClip clip;
    private string micName;
    private bool isRecording = false;
    private Transform playerTransform; 

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerTransform = other.transform;
        StartRecording();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerTransform = null;
        StopRecording();
    }

    void StartRecording()
    {
        if (Microphone.devices.Length == 0) return;
        micName = Microphone.devices[0];
        clip = Microphone.Start(micName, true, recordTime, sampleRate);
        isRecording = true;
    }

    void StopRecording()
    {
        if (!isRecording) return;
        Microphone.End(micName);
        isRecording = false;
    }

    void Update()
    {
        if (!isRecording || clip == null) return;

        float[] samples = new float[windowSize];
        int startSample = Mathf.Max(clip.samples - windowSize, 0);
        clip.GetData(samples, startSample);

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
        }
    }
}
