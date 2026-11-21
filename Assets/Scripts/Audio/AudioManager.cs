using UnityEngine;

namespace SlotGame.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip spinClip;
        [SerializeField] private AudioClip stopClip;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip bigWinClip;
        [SerializeField] private AudioClip bonusClip;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlaySpin() => Play(spinClip);
        public void PlayStop() => Play(stopClip);
        public void PlayWin() => Play(winClip);
        public void PlayBigWin() => Play(bigWinClip);
        public void PlayBonus() => Play(bonusClip);

        private void Play(AudioClip clip)
        {
            if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
        }

        public void SetMusicVolume(float v) { if (musicSource) musicSource.volume = Mathf.Clamp01(v); }
        public void SetSfxVolume(float v) { if (sfxSource) sfxSource.volume = Mathf.Clamp01(v); }
    }
}