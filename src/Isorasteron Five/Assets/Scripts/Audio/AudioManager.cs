
using FMODUnity;
using UnityEngine;

namespace Assets.Scripts.Audio
{
    public static class AudioManager
    {
        public static void PlayOneShot(EventReference reference, string source, Vector3? position = null)
        {
            if (reference.IsNull)
            {
                Debug.LogWarning($"[AudioManager] Event Reference '{source}' is null");
            }

            if (Application.isPlaying && RuntimeManager.IsInitialized)
            {
                RuntimeManager.PlayOneShot(reference, position ?? Vector3.zero);
            }
        }
    }
}
