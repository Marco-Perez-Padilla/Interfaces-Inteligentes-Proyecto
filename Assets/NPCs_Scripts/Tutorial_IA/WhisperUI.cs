using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

/**
 * @file: WhisperUI.cs
 * @brief: Gestiona la grabación de audio del jugador y la transcripción usando la API Whisper.
 *
 * Notas:
 * - Permite iniciar/detener la grabación desde un botón UI o la tecla V.
 * - Convierte el audio grabado en WAV y lo envía a Whisper para obtener transcripción.
 * - Dispara eventos OnWhisperError y OnWhisperTranscription según el resultado.
 */
public class WhisperUI : MonoBehaviour
{
    [Header("UI")]
    public Button recordButton;                // Botón para iniciar/detener grabación
    public Text statusText;                    // Texto de estado de grabación
    public string outputText { get; private set; }  // Última transcripción recibida

    [Header("Whisper API")]
    public string whisperUrl = "http://gpu2.esit.ull.es:8000/v1/audio/transcriptions"; // Endpoint Whisper
    public string model = "medium";           // Modelo de transcripción

    public delegate void WhisperError();
    public event WhisperError OnWhisperError;             // Evento disparado si hay fallo
    public delegate void WhisperTranscription();
    public event WhisperTranscription OnWhisperTranscription; // Evento disparado al recibir transcripción

    private AudioClip recordedClip;            // Audio grabado en memoria
    private string selectedMic;                // Micrófono seleccionado
    private bool isRecording = false;          // Estado de grabación
    private const int sampleRate = 16000;      // Frecuencia de muestreo del micrófono

    void Start()
    {
        // Selecciona el primer micrófono disponible
        selectedMic = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;

        // Suscribir listener al botón de grabación
        recordButton.onClick.AddListener(ToggleRecording);

        statusText.text = "Listo (pulsa V para iniciar grabación)";
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // Tecla V inicia/detiene grabación
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            if (!isRecording)
                StartRecording();
            else
                StopRecording();
        }
    }

    // Alternar grabación desde botón UI
    void ToggleRecording()
    {
        if (!isRecording)
            StartRecording();
        else
            StopRecording();
    }

    // Inicia la grabación de audio
    void StartRecording()
    {
        if (isRecording || selectedMic == null) return;

        recordedClip = Microphone.Start(selectedMic, false, 10, sampleRate);  // Graba 10s máximo
        isRecording = true;
        statusText.text = "Grabando...";
    }

    // Detiene la grabación y envía a Whisper
    void StopRecording()
    {
        if (!isRecording) return;

        Microphone.End(selectedMic);   // Detener grabación
        isRecording = false;
        statusText.text = "Procesando...";

        StartCoroutine(SendToWhisper(recordedClip));  // Enviar audio para transcripción
    }

    // Corrutina que convierte audio a WAV y lo envía al servidor Whisper
    IEnumerator SendToWhisper(AudioClip clip)
    {
        byte[] wavData = SavWavMemory.FromAudioClip(clip); // Convierte AudioClip a WAV

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", wavData, "audio.wav", "audio/wav"); // Añadir archivo al form
        form.AddField("model", model);                                // Modelo de transcripción

        UnityWebRequest request = UnityWebRequest.Post(whisperUrl, form);
        request.SetRequestHeader("Authorization", "Bearer sk-1234");   // Header de autenticación

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            statusText.text = "Error del servidor.";
            OnWhisperError?.Invoke();  // Disparar evento de error
            Debug.LogError(request.error);
        }
        else
        {
            string json = request.downloadHandler.text;
            statusText.text = "Transcrito";  // Actualizar UI
            outputText = json;               // Guardar transcripción
            Debug.LogError(json);
            OnWhisperTranscription?.Invoke(); // Disparar evento de transcripción
        }
    }
}
