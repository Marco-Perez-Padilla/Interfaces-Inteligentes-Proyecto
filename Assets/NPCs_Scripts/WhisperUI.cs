using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class WhisperUI : MonoBehaviour
{
    [Header("UI")]
    public Button recordButton;
    public Text statusText;
    public string outputText { get; private set; }

    [Header("Whisper API")]
    public string whisperUrl = "http://gpu2.esit.ull.es:8000/v1/audio/transcriptions";
    public string model = "medium";

    public delegate void WhisperError();
    public event WhisperError OnWhisperError;
    public delegate void WhisperTranscription();
    public event WhisperTranscription OnWhisperTranscription;

    private AudioClip recordedClip;
    private string selectedMic;
    private bool isRecording = false;
    private const int sampleRate = 16000;

    void Start()
    {
        selectedMic = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        recordButton.onClick.AddListener(ToggleRecording);
        statusText.text = "Listo (pulsa V para iniciar grabación)";
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            if (!isRecording)
                StartRecording();
            else
                StopRecording();
        }
    }

    void ToggleRecording()
    {
        if (!isRecording)
            StartRecording();
        else
            StopRecording();
    }

    void StartRecording()
    {
        if (isRecording || selectedMic == null) return;

        recordedClip = Microphone.Start(selectedMic, false, 10, sampleRate);
        isRecording = true;
        statusText.text = "Grabando...";
    }

    void StopRecording()
    {
        if (!isRecording) return;

        Microphone.End(selectedMic);
        isRecording = false;
        statusText.text = "Procesando...";

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
            statusText.text = "Error del servidor.";
            OnWhisperError?.Invoke();
            Debug.LogError(request.error);
        }
        else
        {
            string json = request.downloadHandler.text;
            statusText.text = "Transcrito";
            outputText = json;
            Debug.LogError(json);
            OnWhisperTranscription?.Invoke();
        }
    }
}
