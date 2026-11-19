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
        BombExplode = 7,
        PlatePress = 8,
        BoostPad = 9
    }

    public class AudioManager : SingletonBehaviour<AudioManager>
    {
        [Header("Master")]
        [SerializeField, Range(0.0f, 1.0f)] private float _musicVolume = 0.5f;
        [SerializeField, Range(0.0f, 1.0f)] private float _sfxVolume = 0.8f;
        [SerializeField] private float _musicFadeTime = 0.5f;

        [Header("Kart Loops")]
        [SerializeField, Range(0.0f, 1.0f)] private float _engineVolume = 0.08f;
        [SerializeField, Range(0.0f, 1.0f)] private float _screechVolume = 0.25f;

        [Header("Music")]
        [SerializeField] private SoundSetting _menuMusic = new SoundSetting();
        [SerializeField] private SoundSetting _countdownMusic = new SoundSetting();
        [SerializeField] private SoundSetting _gameMusic = new SoundSetting();
        [SerializeField] private SoundSetting _victoryMusic = new SoundSetting();

        [Header("Sound Effects")]
        [SerializeField] private SoundSetting _countdownTick = new SoundSetting();
        [SerializeField] private SoundSetting _countdownGo = new SoundSetting();
        [SerializeField] private SoundSetting _steal = new SoundSetting();
        [SerializeField] private SoundSetting _pickup = new SoundSetting();
        [SerializeField] private SoundSetting _boost = new SoundSetting();
        [SerializeField] private SoundSetting _grapple = new SoundSetting();
        [SerializeField] private SoundSetting _bombTick = new SoundSetting();
        [SerializeField] private SoundSetting _bombExplode = new SoundSetting();
        [SerializeField] private SoundSetting _platePress = new SoundSetting();
        [SerializeField] private SoundSetting _boostPad = new SoundSetting();

        private AudioSource _musicA;
        private AudioSource _musicB;
        private AudioSource _activeMusic;
        private AudioSource _sfxSource;
        private eMusic _currentMusic = eMusic.None;

        public static float EngineVolume => Instance != null ? Instance._sfxVolume * Instance._engineVolume : 0.0f;
        public static float ScreechVolume => Instance != null ? Instance._sfxVolume * Instance._screechVolume : 0.0f;

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
            SoundSetting setting = GetSetting(sfx);
            if (setting is not null && setting.Clip != null)
            {
                _sfxSource.PlayOneShot(setting.Clip, _sfxVolume * setting.Volume);
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
            SoundSetting setting = GetSetting(music);
            AudioClip clip = setting is not null ? setting.Clip : null;
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
            SoundSetting setting = GetSetting(_currentMusic);
            float targetVolume = setting is not null ? _musicVolume * setting.Volume : 0.0f;
            float step = _musicFadeTime > 0.0f ? Time.unscaledDeltaTime / _musicFadeTime : 1.0f;
            AudioSource inactive = _activeMusic == _musicA ? _musicB : _musicA;

            _activeMusic.volume = Mathf.MoveTowards(_activeMusic.volume, targetVolume, step);
            inactive.volume = Mathf.MoveTowards(inactive.volume, 0.0f, step);
            if (inactive.isPlaying && inactive.volume <= 0.0f)
            {
                inactive.Stop();
            }
        }

        private SoundSetting GetSetting(eMusic music)
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

        private SoundSetting GetSetting(eSfx sfx)
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
                case eSfx.PlatePress:
                    return _platePress;
                case eSfx.BoostPad:
                    return _boostPad;
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
