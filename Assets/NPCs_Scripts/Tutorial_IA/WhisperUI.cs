using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Android;

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

    void Start()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            Permission.RequestUserPermission(Permission.Microphone);

        selectedMic = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;

        if (recordButton != null)
            recordButton.onClick.AddListener(ToggleRecording);

        SetStatus("Listo");
    }

    void OnEnable()
    {
        if (recordAction != null)
        {
            recordAction.action.performed += OnRecordPressed;
            recordAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (recordAction != null)
        {
            recordAction.action.performed -= OnRecordPressed;
            recordAction.action.Disable();
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.vKey.wasPressedThisFrame)
            ToggleRecording();
    }

    private void OnRecordPressed(InputAction.CallbackContext context) => ToggleRecording();

    void ToggleRecording()
    {
        if (!isRecording) StartRecording();
        else StopRecording();
    }

    void SetStatus(string status)
    {
        if (statusText != null)
            statusText.text = RECORD_CONTROLS + status;
    }

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

    void StopRecording()
    {
        if (!isRecording) return;

        Microphone.End(selectedMic);
        isRecording = false;
        SetStatus("Procesando...");

        StartCoroutine(SendToWhisper(recordedClip));
    }

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