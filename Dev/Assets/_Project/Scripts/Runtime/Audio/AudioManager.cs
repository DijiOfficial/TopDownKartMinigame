using UnityEngine;

namespace Battlekart
{
    public enum eMusic
    {
        None = 0,
        Menu = 1,
        Countdown = 2,
        Game = 3,
        Victory = 4
    }

    public enum eSfx
    {
        CountdownTick = 0,
        CountdownGo = 1,
        Steal = 2,
        Pickup = 3,
        Boost = 4,
        Grapple = 5,
        BombTick = 6,
        BombExplode = 7
    }

    public class AudioManager : SingletonBehaviour<AudioManager>
    {
        [Header("Music")]
        [SerializeField] private AudioClip _menuMusic;
        [SerializeField] private AudioClip _countdownMusic;
        [SerializeField] private AudioClip _gameMusic;
        [SerializeField] private AudioClip _victoryMusic;
        [SerializeField, Range(0.0f, 1.0f)] private float _musicVolume = 0.5f;
        [SerializeField] private float _musicFadeTime = 0.5f;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip _countdownTick;
        [SerializeField] private AudioClip _countdownGo;
        [SerializeField] private AudioClip _steal;
        [SerializeField] private AudioClip _pickup;
        [SerializeField] private AudioClip _boost;
        [SerializeField] private AudioClip _grapple;
        [SerializeField] private AudioClip _bombTick;
        [SerializeField] private AudioClip _bombExplode;
        [SerializeField, Range(0.0f, 1.0f)] private float _sfxVolume = 0.8f;

        private AudioSource _musicA;
        private AudioSource _musicB;
        private AudioSource _activeMusic;
        private AudioSource _sfxSource;
        private eMusic _currentMusic = eMusic.None;

        protected override void Awake()
        {
            base.Awake();
            _musicA = CreateSource("Music A");
            _musicB = CreateSource("Music B");
            _sfxSource = CreateSource("Sfx");
            _activeMusic = _musicA;
        }

        public static void Play(eSfx sfx)
        {
            if (Instance != null)
            {
                Instance.PlaySfx(sfx);
            }
        }

        public static void Play(eMusic music)
        {
            if (Instance != null)
            {
                Instance.PlayMusic(music);
            }
        }

        public static void SetMusicPitch(float pitch)
        {
            if (Instance != null)
            {
                Instance._musicA.pitch = pitch;
                Instance._musicB.pitch = pitch;
            }
        }

        private void PlaySfx(eSfx sfx)
        {
            AudioClip clip = GetClip(sfx);
            if (clip != null)
            {
                _sfxSource.PlayOneShot(clip, _sfxVolume);
            }
        }

        private void PlayMusic(eMusic music)
        {
            if (music == _currentMusic)
            {
                return;
            }

            _currentMusic = music;
            SetMusicPitch(1.0f);

            AudioSource next = _activeMusic == _musicA ? _musicB : _musicA;
            AudioClip clip = GetClip(music);
            next.Stop();
            next.clip = clip;
            next.loop = music == eMusic.Menu || music == eMusic.Game;
            next.volume = 0.0f;
            if (clip != null)
            {
                next.Play();
            }

            _activeMusic = next;
        }

        private void Update()
        {
            float step = _musicFadeTime > 0.0f ? Time.unscaledDeltaTime / _musicFadeTime * _musicVolume : _musicVolume;
            AudioSource inactive = _activeMusic == _musicA ? _musicB : _musicA;

            _activeMusic.volume = Mathf.MoveTowards(_activeMusic.volume, _musicVolume, step);
            inactive.volume = Mathf.MoveTowards(inactive.volume, 0.0f, step);
            if (inactive.isPlaying && inactive.volume <= 0.0f)
            {
                inactive.Stop();
            }
        }

        private AudioClip GetClip(eMusic music)
        {
            switch (music)
            {
                case eMusic.Menu:
                    return _menuMusic;
                case eMusic.Countdown:
                    return _countdownMusic;
                case eMusic.Game:
                    return _gameMusic;
                case eMusic.Victory:
                    return _victoryMusic;
                default:
                    return null;
            }
        }

        private AudioClip GetClip(eSfx sfx)
        {
            switch (sfx)
            {
                case eSfx.CountdownTick:
                    return _countdownTick;
                case eSfx.CountdownGo:
                    return _countdownGo;
                case eSfx.Steal:
                    return _steal;
                case eSfx.Pickup:
                    return _pickup;
                case eSfx.Boost:
                    return _boost;
                case eSfx.Grapple:
                    return _grapple;
                case eSfx.BombTick:
                    return _bombTick;
                case eSfx.BombExplode:
                    return _bombExplode;
                default:
                    return null;
            }
        }

        private AudioSource CreateSource(string sourceName)
        {
            GameObject sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform, false);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0.0f;
            return source;
        }
    }
}
