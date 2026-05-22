using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Android;
/**
 * @file: WhisperUI.cs
 * @brief: Gestiona la interacción con el sistema de reconocimiento de voz Whisper para NPCs interactivos.
 *
 * Notas:
 * - Permite al jugador grabar su voz usando un botón o una acción de entrada.
 * - Envía la grabación a un servidor de Whisper para su transcripción.
 * - Dispara eventos con la transcripción o en caso de error para que otros sistemas (como OllamaUI) puedan reaccionar.
 * - Muestra el estado actual (listo, grabando, procesando) en un Text UI.
 * - Requiere permisos de micrófono en Android.
 */
public class WhisperUI : MonoBehaviour
{
    [Header("UI")]
    public Button recordButton;
    public Text statusText;
    public string outputText { get; private set; }

    [Header("Whisper API")]
    public string whisperUrl = "http://gpu2.esit.ull.es:8000/v1/audio/transcriptions";
    public string model = "medium";

    [Header("VR Input")]
    public InputActionReference recordAction;

    public delegate void WhisperError();
    public event WhisperError OnWhisperError;
    public delegate void WhisperTranscription();
    public event WhisperTranscription OnWhisperTranscription;

    private AudioClip recordedClip;
    private string selectedMic;
    private bool isRecording = false;
    private const int sampleRate = 16000;

    private const string RECORD_CONTROLS = "Grabar/Parar: Grip izq, \'X\' o Button East\n\n";

    /// <summary>
    /// Inicializa el estado del UI y solicita permisos de micrófono si es necesario.
    /// Configura el botón de grabación para alternar entre grabar y detener la grabación.
    /// </summary>
    void Start()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            Permission.RequestUserPermission(Permission.Microphone);

        selectedMic = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;

        if (recordButton != null)
            recordButton.onClick.AddListener(ToggleRecording);

        SetStatus("Listo");
    }

    ///  <summary>
    /// Se suscribe a la acción de entrada para mostrar/ocultar el panel.
    /// </summary>
    void OnEnable()
    {
        if (recordAction != null)
        {
            recordAction.action.performed += OnRecordPressed;
            recordAction.action.Enable();
        }
    }

    /// <summary>
    /// Se desuscribe de la acción de entrada para evitar fugas de memoria.
    /// </summary>
    void OnDisable()
    {
        if (recordAction != null)
        {
            recordAction.action.performed -= OnRecordPressed;
            recordAction.action.Disable();
        }
    }

    ///  <summary>
    /// Permite usar la tecla 'V' para alternar la grabación, útil para pruebas en PC sin VR.
    /// </summary>
    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.vKey.wasPressedThisFrame)
            ToggleRecording();
    }

    /// <summary> 
    /// Maneja la acción de entrada para grabar o detener la grabación.
    /// </summary> 
    private void OnRecordPressed(InputAction.CallbackContext context) => ToggleRecording();

    ///  <summary>
    /// Alterna entre iniciar y detener la grabación de audio.
    /// Inicia la grabación usando el micrófono seleccionado y envía el audio a Whisper al detenerse.
    /// </summary>
    void ToggleRecording()
    {
        if (!isRecording) StartRecording();
        else StopRecording();
    }

    /// <summary>
    /// Actualiza el texto de estado en la UI para informar al jugador sobre el estado actual (listo, grabando, procesando).
    /// </summary>
    void SetStatus(string status)
    {
        if (statusText != null)
            statusText.text = RECORD_CONTROLS + status;
    }

    ///  <summary>
    /// Inicia la grabación de audio desde el micrófono seleccionado. Verifica permisos y disponibilidad del micrófono antes de comenzar.
    /// </summary>
    void StartRecording()
    {
        if (isRecording) return;
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone)) return;
        if (selectedMic == null)
        {
            selectedMic = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
            if (selectedMic == null) return;
        }

        recordedClip = Microphone.Start(selectedMic, false, 10, sampleRate);
        isRecording = true;
        SetStatus("Grabando...");
    }

    /// <summary>
    /// Detiene la grabación de audio, procesa el clip grabado y lo envía a Whisper para su transcripción. Actualiza el estado a "Procesando..." mientras espera la respuesta.
    /// </summary>
    void StopRecording()
    {
        if (!isRecording) return;

        Microphone.End(selectedMic);
        isRecording = false;
        SetStatus("Procesando...");

        StartCoroutine(SendToWhisper(recordedClip));
    }

    /// <summary>
    /// Corrutina que convierte el AudioClip grabado a formato WAV, lo envía a la API de Whisper usando UnityWebRequest, y maneja la respuesta o el error. Dispara eventos según el resultado para que otros sistemas puedan reaccionar.
    /// </summary>
    IEnumerator SendToWhisper(AudioClip clip)
    {
        byte[] wavData = SavWavMemory.FromAudioClip(clip);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", wavData, "audio.wav", "audio/wav");
        form.AddField("model", model);

        UnityWebRequest request = UnityWebRequest.Post(whisperUrl, form);
        request.SetRequestHeader("Authorization", "Bearer sk-1234");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            OnWhisperError?.Invoke();
            Debug.LogError(request.error);
            SetStatus("Error del servidor.");
        }
        else
        {
            string json = request.downloadHandler.text;
            outputText = json;
            Debug.LogError(json);
            OnWhisperTranscription?.Invoke();
            SetStatus("Transcrito");
        }
    }
}