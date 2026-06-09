using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Adicione este script diretamente no objeto interagível.
/// Ele cria o Canvas/UI automaticamente e gerencia toda a lógica.
/// </summary>
public class JumpscareInteractable : MonoBehaviour
{
    [Header("Jumpscare Assets")]
    public Sprite jumpscareImage;

    [Header("FMOD")]
    [EventRef]
    public string jumpscareSoundEvent; // ex: "event:/SFX/Jumpscare"

    [Header("Interação")]
    public float interactionRadius = 3f;
    public string playerTag = "Player";

    [Header("Configurações")]
    [Range(0.5f, 5f)]
    public float jumpscareDuration = 2.5f;
    public bool showInteractPrompt = true;

    // --- Privados ---
    private Transform _player;
    private bool _triggered = false;
    private Image _jumpscareUI;
    private GameObject _promptUI;
    private EventInstance _soundInstance;

    void Start()
    {
        // Busca o player
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            _player = playerObj.transform;
        else
            Debug.LogWarning("[Jumpscare] Player não encontrado. Verifique a tag.");

        BuildUI();
    }

    void BuildUI()
    {
        // Canvas fullscreen
        GameObject canvasGO = new GameObject("_JumpscareCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);

        // Imagem do jumpscare (tela cheia)
        GameObject imgGO = new GameObject("JumpscareImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        _jumpscareUI = imgGO.AddComponent<Image>();
        _jumpscareUI.color = Color.white;
        _jumpscareUI.raycastTarget = false;
        RectTransform imgRT = imgGO.GetComponent<RectTransform>();
        imgRT.anchorMin = Vector2.zero;
        imgRT.anchorMax = Vector2.one;
        imgRT.offsetMin = Vector2.zero;
        imgRT.offsetMax = Vector2.zero;
        imgGO.SetActive(false);

        // Prompt "[E] Interagir"
        if (showInteractPrompt)
        {
            _promptUI = new GameObject("InteractPrompt");
            _promptUI.transform.SetParent(canvasGO.transform, false);

            // Fundo semi-transparente
            Image bg = _promptUI.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.55f);
            bg.raycastTarget = false;
            RectTransform bgRT = _promptUI.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0.5f, 0.08f);
            bgRT.anchorMax = new Vector2(0.5f, 0.08f);
            bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.sizeDelta = new Vector2(220f, 44f);

            // Texto
            GameObject textGO = new GameObject("PromptText");
            textGO.transform.SetParent(_promptUI.transform, false);
            Text txt = textGO.AddComponent<Text>();
            txt.text = "[E]  Interagir";
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 22;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.raycastTarget = false;
            RectTransform txtRT = textGO.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;

            _promptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (_triggered || _player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);
        bool inRange = dist <= interactionRadius;

        if (_promptUI != null)
            _promptUI.SetActive(inRange);

        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            _triggered = true;

            if (_promptUI != null)
                _promptUI.SetActive(false);

            StartCoroutine(PlayJumpscare());
        }
    }

    private IEnumerator PlayJumpscare()
    {
        // Ativa a imagem
        if (_jumpscareUI != null)
        {
            _jumpscareUI.sprite = jumpscareImage;
            _jumpscareUI.gameObject.SetActive(true);

            // Animação de escala: surge rápido
            float elapsed = 0f;
            float scaleTime = 0.08f;
            _jumpscareUI.transform.localScale = Vector3.one * 0.3f;

            while (elapsed < scaleTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / scaleTime);
                _jumpscareUI.transform.localScale = Vector3.Lerp(
                    Vector3.one * 0.3f,
                    Vector3.one * 1.05f,
                    t
                );
                yield return null;
            }

            _jumpscareUI.transform.localScale = Vector3.one;
        }

        // Dispara o evento FMOD
        if (!string.IsNullOrEmpty(jumpscareSoundEvent))
        {
            _soundInstance = RuntimeManager.CreateInstance(jumpscareSoundEvent);
            _soundInstance.start();
        }

        // Aguarda
        yield return new WaitForSecondsRealtime(jumpscareDuration);

        // Libera FMOD
        _soundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _soundInstance.release();

        // Fecha o jogo
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        if (_soundInstance.isValid())
        {
            _soundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _soundInstance.release();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.35f);
        Gizmos.DrawSphere(transform.position, interactionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
