using UnityEngine;

namespace ShinA.Managers
{
    public enum SoundPlaybackType
    {
        Sfx,
        Bgm
    }

    public sealed class SoundSetter : MonoBehaviour
    {
        [SerializeField] private AudioClip soundClip;
        [SerializeField] private SoundPlaybackType playbackType = SoundPlaybackType.Sfx;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField] private bool playOnStart;
        [SerializeField] private bool loopBgm = true;
        [SerializeField] private bool spatialSfx;

        public AudioClip SoundClip
        {
            get => soundClip;
            set => soundClip = value;
        }

        public SoundPlaybackType PlaybackType
        {
            get => playbackType;
            set => playbackType = value;
        }

        public float Volume
        {
            get => volume;
            set => volume = Mathf.Clamp01(value);
        }

        private void Start()
        {
            if (playOnStart)
            {
                Play();
            }
        }

        public void Play()
        {
            if (soundClip == null)
            {
                return;
            }

            if (playbackType == SoundPlaybackType.Bgm)
            {
                SoundManager.Instance.PlayBgm(soundClip, volume, loopBgm);
            }
            else if (spatialSfx)
            {
                SoundManager.Instance.PlaySfxAtPosition(soundClip, transform.position, volume);
            }
            else
            {
                SoundManager.Instance.PlaySfx(soundClip, volume);
            }
        }

        public void StopBgm()
        {
            if (playbackType == SoundPlaybackType.Bgm && SoundManager.Instance.CurrentBgm == soundClip)
            {
                SoundManager.Instance.StopBgm();
            }
        }
    }
}
