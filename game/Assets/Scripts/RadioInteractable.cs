using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;
using FMOD.Studio;

public class RadioInteractable : MonoBehaviour
{
    [Header("FMOD")]
    [SerializeField] private EventReference fmodEvent;

    [Header("Interação")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private string playerTag = "Player";

    [Header("UI (Opcional)")]
    [SerializeField] private GameObject promptUI;

    private EventInstance eventInstance;
    private bool isPlaying;
    private Transform player;

    private void Awake()
    {
        eventInstance = RuntimeManager.CreateInstance(fmodEvent);
        RuntimeManager.AttachInstanceToGameObject(eventInstance, gameObject);

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag(playerTag);

            if (playerObject != null)
                player = playerObject.transform;

            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        bool playerPerto = distance <= interactionDistance;

        if (promptUI != null)
            promptUI.SetActive(playerPerto);

        if (playerPerto &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            DesligarRadio();
        }
    }

    public void LigarRadio()
    {
        if (isPlaying)
            return;

        PLAYBACK_STATE playbackState;
        eventInstance.getPlaybackState(out playbackState);

        if (playbackState != PLAYBACK_STATE.PLAYING)
        {
            eventInstance.start();
        }

        isPlaying = true;
    }

    public void DesligarRadio()
    {
        if (!isPlaying)
            return;

        eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        isPlaying = false;

        if (promptUI != null)
            promptUI.SetActive(false);

        Debug.Log("Rádio desligado.");
    }

    private void OnDestroy()
    {
        if (eventInstance.isValid())
        {
       eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            eventInstance.release();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            interactionDistance
        );
    }
}