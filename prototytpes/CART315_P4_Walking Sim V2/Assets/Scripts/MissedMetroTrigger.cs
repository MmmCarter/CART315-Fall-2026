using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MissedMetroTrigger : MonoBehaviour
{
    [SerializeField] private CharacterController player;
    [SerializeField] private AudioSource metroSound;

    [SerializeField] private float duration = 6f;
    [SerializeField] private float travelDistance = 30f;

    private bool hasPlayed;

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed || player == null || metroSound == null)
            return;

        if (other.GetComponentInParent<CharacterController>() != player)
            return;

        hasPlayed = true;
        StartCoroutine(Depart());
    }

    private IEnumerator Depart()
    {
        Vector3 startPosition = metroSound.transform.position;
        float startVolume = metroSound.volume;
        float length = Mathf.Max(0.1f, duration);

        metroSound.loop = false;
        metroSound.Play();

        float elapsed = 0f;

        while (elapsed < length)
        {
            float progress = elapsed / length;

            metroSound.transform.position =
                startPosition + Vector3.forward * travelDistance * progress;

            metroSound.volume =
                startVolume * (1f - progress);

            elapsed += Time.deltaTime;
            yield return null;
        }

        metroSound.Stop();
        metroSound.volume = startVolume;
        metroSound.transform.position = startPosition;
    }
}