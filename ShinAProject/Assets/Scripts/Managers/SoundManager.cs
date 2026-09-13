using UnityEngine;

namespace ShinA.Managers
{
    public sealed class SoundManager : MonoBehaviour
    {
        private static SoundManager instance;
        private AudioSource bgmSource;
        private AudioSource sfxSource;

        public static SoundManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<SoundManager>();
                    if (instance == null)
                    {
                        GameObject managerObject = new("SoundManager");
                        instance = managerObject.AddComponent<SoundManager>();
                    }
                }

                return instance;
            }
        }

        public AudioClip CurrentBgm => bgmSource != null ? bgmSource.clip : null;
        public bool IsBgmPlaying => bgmSource != null && bgmSource.isPlaying;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateOnStartup()
        {
            _ = Instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            CreateAudioSources();
        }

        public void PlayBgm(AudioClip clip, float volume = 1f, bool loop = true, bool restart = false)
        {
            if (clip == null)
            {
                return;
            }

            if (!restart && bgmSource.clip == clip && bgmSource.isPlaying)
            {
                return;
            }

            bgmSource.clip = clip;
            bgmSource.volume = Mathf.Clamp01(volume);
            bgmSource.loop = loop;
            bgmSource.Play();
        }

        public void StopBgm()
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        }

        public void PauseBgm()
        {
            bgmSource.Pause();
        }

        public void ResumeBgm()
        {
            bgmSource.UnPause();
        }

        public void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
            }
        }

        public void PlaySfxAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, Mathf.Clamp01(volume));
            }
        }

        private void CreateAudioSources()
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
        }
    }
}
