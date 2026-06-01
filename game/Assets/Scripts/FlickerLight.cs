using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// FlickerLight — Unity 6 (Horror Edition)
/// Funciona tanto em Play Mode quanto diretamente no Editor (Edit Mode).
/// Attach em qualquer GameObject com componente Light.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Light))]
public class FlickerLight : MonoBehaviour
{
    [Header("Intensidade")]
    [Tooltip("Intensidade máxima da luz quando ligada.")]
    [SerializeField] private float onIntensity = 1f;

    [Tooltip("Varia a intensidade aleatoriamente enquanto a luz está ligada, dando sensação de voltagem instável.")]
    [SerializeField] private bool randomizeIntensity = true;

    [Tooltip("Intensidade mínima durante a variação (só usado se Randomize Intensity estiver ativo).")]
    [SerializeField, Range(0f, 1f)] private float minIntensityRatio = 0.4f;

    [Header("Surto rápido (faísca)")]
    [Tooltip("Quantidade mínima de piscadas rápidas em sequência.")]
    [SerializeField] private int minBurstCount = 2;

    [Tooltip("Quantidade máxima de piscadas rápidas em sequência.")]
    [SerializeField] private int maxBurstCount = 8;

    [Tooltip("Duração mínima de cada piscada dentro do surto (segundos).")]
    [SerializeField] private float minBurstFlicker = 0.03f;

    [Tooltip("Duração máxima de cada piscada dentro do surto (segundos).")]
    [SerializeField] private float maxBurstFlicker = 0.12f;

    [Header("Pausa entre surtos")]
    [Tooltip("Tempo mínimo que a luz fica estável (ligada ou apagada) entre surtos.")]
    [SerializeField] private float minIdleTime = 0.2f;

    [Tooltip("Tempo máximo que a luz fica estável entre surtos. Valores altos criam longos apagões.")]
    [SerializeField] private float maxIdleTime = 3.5f;

    [Tooltip("Chance (0 a 1) de a luz ficar APAGADA durante a pausa, em vez de ligada.")]
    [SerializeField, Range(0f, 1f)] private float blackoutChance = 0.35f;

    [Header("Geral")]
    [Tooltip("Inicia o flicker automaticamente ao entrar em cena.")]
    [SerializeField] private bool playOnAwake = true;

    // ── referências internas ──────────────────────────────────────────────
    private Light _light;
    private Coroutine _flickerCoroutine;

    // Controle de tempo para o Editor (substitui WaitForSeconds fora do Play Mode)
    private double _editorNextStepTime;
    private int    _editorBurstRemaining;
    private bool   _editorInBurst;
    private bool   _editorRunning;

    // ─────────────────────────────────────────────────────────────────────
    private void OnEnable()
    {
        _light = GetComponent<Light>();
        _light.intensity = onIntensity;

        if (playOnAwake)
            StartFlicker();
    }

    private void OnDisable()
    {
        StopFlicker();

#if UNITY_EDITOR
        EditorApplication.update -= EditorTick;
#endif
    }

    // ── API pública ───────────────────────────────────────────────────────

    /// <summary>Inicia (ou reinicia) o efeito de flicker.</summary>
    public void StartFlicker()
    {
        StopFlicker(leaveOn: true);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            _editorRunning = true;
            _editorInBurst = true;
            _editorBurstRemaining = Random.Range(minBurstCount, maxBurstCount + 1);
            _editorNextStepTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += EditorTick;
            return;
        }
#endif
        _flickerCoroutine = StartCoroutine(HorrorFlickerRoutine());
    }

    /// <summary>Para o efeito e deixa a luz no estado desejado.</summary>
    public void StopFlicker(bool leaveOn = true)
    {
#if UNITY_EDITOR
        if (_editorRunning)
        {
            _editorRunning = false;
            EditorApplication.update -= EditorTick;
        }
#endif
        if (_flickerCoroutine != null)
        {
            StopCoroutine(_flickerCoroutine);
            _flickerCoroutine = null;
        }

        if (_light == null) return;
        _light.enabled = leaveOn;
        if (leaveOn) _light.intensity = onIntensity;
    }

    // ── Tick do Editor (substitui a Coroutine fora do Play Mode) ─────────
#if UNITY_EDITOR
    private void EditorTick()
    {
        if (!_editorRunning || _light == null) return;
        if (EditorApplication.timeSinceStartup < _editorNextStepTime) return;

        if (_editorInBurst)
        {
            // Alterna a luz e agenda o próximo passo do surto
            _light.enabled = !_light.enabled;

            if (_light.enabled)
                _light.intensity = randomizeIntensity
                    ? onIntensity * Random.Range(minIntensityRatio, 1f)
                    : onIntensity;

            _editorNextStepTime = EditorApplication.timeSinceStartup
                                  + Random.Range(minBurstFlicker, maxBurstFlicker);

            _editorBurstRemaining--;

            if (_editorBurstRemaining <= 0)
            {
                // Surto terminou — entra na pausa
                _light.enabled    = true;
                _light.intensity  = onIntensity;
                _editorInBurst    = false;

                bool goBlackout  = Random.value < blackoutChance;
                _light.enabled   = !goBlackout;

                _editorNextStepTime = EditorApplication.timeSinceStartup
                                      + Random.Range(minIdleTime, maxIdleTime);
            }
        }
        else
        {
            // Pausa terminou — inicia novo surto
            _editorInBurst        = true;
            _editorBurstRemaining = Random.Range(minBurstCount, maxBurstCount + 1);
            _editorNextStepTime   = EditorApplication.timeSinceStartup;
        }

        // Força o Editor a redesenhar a Scene View
        SceneView.RepaintAll();
    }
#endif

    // ── Coroutine principal (Play Mode) ───────────────────────────────────
    private IEnumerator HorrorFlickerRoutine()
    {
        while (true)
        {
            // 1. Surto: série de piscadas rápidas
            int burstCount = Random.Range(minBurstCount, maxBurstCount + 1);

            for (int i = 0; i < burstCount; i++)
            {
                _light.enabled = !_light.enabled;

                if (_light.enabled)
                    _light.intensity = randomizeIntensity
                        ? onIntensity * Random.Range(minIntensityRatio, 1f)
                        : onIntensity;

                yield return new WaitForSeconds(Random.Range(minBurstFlicker, maxBurstFlicker));
            }

            // 2. Pausa: luz estável ou apagão
            _light.enabled   = true;
            _light.intensity = onIntensity;

            bool goBlackout  = Random.value < blackoutChance;
            _light.enabled   = !goBlackout;

            yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));
        }
    }

    // ── Validação no editor ───────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnValidate()
    {
        minBurstCount   = Mathf.Max(1, minBurstCount);
        maxBurstCount   = Mathf.Max(minBurstCount, maxBurstCount);
        minBurstFlicker = Mathf.Max(0.01f, minBurstFlicker);
        maxBurstFlicker = Mathf.Max(minBurstFlicker, maxBurstFlicker);
        minIdleTime     = Mathf.Max(0.05f, minIdleTime);
        maxIdleTime     = Mathf.Max(minIdleTime, maxIdleTime);
        onIntensity     = Mathf.Max(0f, onIntensity);
    }
#endif
}