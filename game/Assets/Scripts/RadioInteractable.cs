using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using TMPro; // opcional, para UI de "Pressione E"

public class RadioInteractable : MonoBehaviour
{
    [Header("FMOD")]
    [SerializeField] private EventReference fmodEvent;

    [Header("Interação")]
    public float interactionDistance = 2f;
    public string playerTag = "Player";
    public KeyCode interactionKey = KeyCode.E;

    [Header("UI (opcional)")]
    public GameObject promptUI; // objeto com texto "Pressione E para desligar"

    private EventInstance eventInstance;
    private bool isPlaying = false;
    private Transform player;

    void Awake()
    {
        eventInstance = RuntimeManager.CreateInstance(fmodEvent);
        RuntimeManager.AttachInstanceToGameObject(eventInstance, transform);

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    void Update()
    {
        if (!isPlaying) return;

        // Encontra o player se ainda não tiver referência
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag(playerTag);
            if (p != null) player = p.transform;
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        bool playerPerto = distance <= interactionDistance;

        // Mostra/esconde o prompt de interação
        if (promptUI != null)
            promptUI.SetActive(playerPerto);

        // Desliga ao pressionar E perto do rádio
        if (playerPerto && Input.GetKeyDown(interactionKey))
        {
            DesligarRadio();
        }
    }

    public void LigarRadio()
    {
        if (isPlaying) return;

        eventInstance.start();
        isPlaying = true;
    }

    public void DesligarRadio()
    {
        eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        isPlaying = false;

        if (promptUI != null)
            promptUI.SetActive(false);

        Debug.Log("Rádio desligado!");
    }

    void OnDestroy()
    {
        eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        eventInstance.release();
    }

    // Desenha o raio de interação no editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}