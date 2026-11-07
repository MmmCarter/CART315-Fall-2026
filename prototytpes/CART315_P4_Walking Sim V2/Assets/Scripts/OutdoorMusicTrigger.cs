using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OutdoorMusicTrigger : MonoBehaviour
{
    [SerializeField] private CharacterController player;
    [SerializeField] private AudioSource musicSource;

    [SerializeField, Range(0f, 1f)]
    private float targetVolume = 0.18f;

    [SerializeField, Min(0f)]
    private float fadeDuration = 4f;

    private bool hasStarted;

    private void Awake()
    {
        if (musicSource == null)
            return;

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasStarted || player == null || musicSource == null)
            return;

        if (other.GetComponentInParent<CharacterController>() != player)
            return;

        if (musicSource.clip == null)
            return;

        hasStarted = true;
        StartCoroutine(FadeInMusic());
    }

    private IEnumerator FadeInMusic()
    {
        musicSource.volume = 0f;
        musicSource.Play();

        if (fadeDuration <= 0f)
        {
            musicSource.volume = targetVolume;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / fadeDuration);

            musicSource.volume =
                Mathf.Lerp(0f, targetVolume, progress);

            yield return null;
        }

        musicSource.volume = targetVolume;
    }
}