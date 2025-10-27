using UnityEngine;

public class DoorBellTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource doorBellAudioSource; // Assign in inspector (optional)
    [SerializeField] private OutsideGhoulSpawner outsideGhoulSpawner; // Assign in inspector

    [Header("Settings")]
    [Tooltip("The tag used for customer objects.")]
    [SerializeField] private string customerTag = "Customer";
    [Tooltip("The tag used for the player object.")]
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(customerTag))
        {
            if (doorBellAudioSource != null)
            {
                doorBellAudioSource.Play();
            }
        }
        else if (other.CompareTag(playerTag))
        {
            if (outsideGhoulSpawner != null)
            {
                outsideGhoulSpawner.SpawnKillers();
            }
        }
    }
}