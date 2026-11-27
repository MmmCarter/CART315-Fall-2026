
using UnityEngine;
using Cart315.FirstPerson;

[RequireComponent(typeof(FPController))]
public class PlayerReset : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnPoint;

    private FPController fpController;
    private FPInputManager inputManager;
    private FPJumping fpJumping;

    public bool HasControl =>
        inputManager != null && inputManager.HasControl;

    private void Awake()
    {
        fpController = GetComponent<FPController>();
        inputManager = GetComponent<FPInputManager>();
        fpJumping = GetComponent<FPJumping>();
    }

    public void ResetPlayer()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("SpawnPoint is not assigned!");
            return;
        }

        // Reset the jump buffer
        if (fpJumping != null && fpJumping.enabled)
        {
            fpJumping.enabled = false;
            fpJumping.enabled = true;
        }

        // Reset the first-person controller
        fpController.ResetPlayerState(
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}
