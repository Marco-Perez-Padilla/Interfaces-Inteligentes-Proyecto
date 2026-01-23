using UnityEngine;
using System;
using System.Linq;

/**
 * @file: NoiseDetector.cs
 * @brief: Detecta ruido del jugador dentro de una zona trigger usando el micrófono del sistema.
 * Dispara eventos cuando el nivel de ruido supera umbrales definidos (normal o alto). Otros scripts
 * pueden suscribirse a estos eventos para reaccionar al ruido del jugador.
 *
 * Notas: La detección es continua mientras el jugador esté dentro del trigger. Los valores de
 * minFrequency y maxFrequency son informativos, el filtrado real requeriría FFT. El análisis se
 * realiza mediante RMS (Root Mean Square) del audio capturado.
 */
public class NoiseDetector : MonoBehaviour
{
    [Header("Zone Settings")]
    public float threshold = 0.05f;                // Nivel de ruido para disparar evento normal
    public float highNoiseThreshold = 0.05f;       // Nivel de ruido para disparar evento de alto ruido
    public float minFrequency = 100f;              // Frecuencia mínima (info, no implementado)
    public float maxFrequency = 5000f;             // Frecuencia máxima (info, no implementado)
    public int windowSize = 1024;                  // Tamaño de ventana para análisis de audio

    [Header("Audio Settings")]
    public int sampleRate = 44100;                 // Frecuencia de muestreo del micrófono
    public int recordTime = 1;                     // Tiempo de grabación en segundos

    // Eventos que se disparan cuando se detecta ruido
    public delegate void NoiseEvent(Vector3 position, float intensity);
    public event NoiseEvent OnNoiseDetected;       // Evento de ruido normal
    public event NoiseEvent OnHighNoiseDetected;   // Evento de ruido alto

    private AudioClip clip;
    private string micName;
    private bool isRecording = false;
    private Transform playerTransform; 

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    // Iniciar grabación cuando el jugador entra en la zona
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerTransform = other.transform;
        StartRecording();
    }

    // Detener grabación cuando el jugador sale de la zona
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerTransform = null;
        StopRecording();
    }

    // Iniciar captura del micrófono
    void StartRecording()
    {
        if (Microphone.devices.Length == 0) return;
        micName = Microphone.devices[0];
        clip = Microphone.Start(micName, true, recordTime, sampleRate);
        isRecording = true;
    }

    // Detener captura del micrófono
    void StopRecording()
    {
        if (!isRecording) return;
        Microphone.End(micName);
        isRecording = false;
    }

    // Análisis continuo del nivel de ruido
    void Update()
    {
        if (!isRecording || clip == null) return;

        // Obtener muestras de audio
        float[] samples = new float[windowSize];
        int startSample = Mathf.Max(clip.samples - windowSize, 0);
        clip.GetData(samples, startSample);

        // Calcular RMS (Root Mean Square) como medida de intensidad
        float rms = Mathf.Sqrt(samples.Average(s => s * s));

        float frequencyFilteredRMS = rms; 
        
        // Disparar evento de ruido alto si supera el umbral
        if(frequencyFilteredRMS > highNoiseThreshold)
        {
            Vector3 noisePosition = playerTransform != null ? playerTransform.position : transform.position;
            OnHighNoiseDetected?.Invoke(noisePosition, frequencyFilteredRMS);
            return;
        }

        // Disparar evento de ruido normal si supera el umbral
        if(frequencyFilteredRMS > threshold)
        {
            Vector3 noisePosition = playerTransform != null ? playerTransform.position : transform.position;
            OnNoiseDetected?.Invoke(noisePosition, frequencyFilteredRMS);
        }
    }
}