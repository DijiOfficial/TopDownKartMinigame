using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Battlekart
{
    public class MatchManager : MonoBehaviour
    {
        public enum eMatchState
        {
            Lobby = 0,
            Countdown = 1,
            Playing = 2,
            SuddenDeath = 3,
            Finished = 4
        }

        [SerializeField] private StealSystem _stealSystem;
        [SerializeField] private PlayerInputManager _playerInputManager;
        [SerializeField] private KartSpawner _kartSpawner;
        [SerializeField] private EndSequence _endSequence;
        [SerializeField] private float _matchDuration = 180.0f;
        [SerializeField] private float _countdownDuration = 3.0f;
        [SerializeField] private float _suddenDeathMusicPitch = 1.1f;
        [SerializeField] private GameObject[] _lobbyOnlyObjects = System.Array.Empty<GameObject>();

        [Header("HUD Layout")]
        [SerializeField, Range(0.0f, 0.5f)] private float _scoreboardTop = 0.01f;
        [SerializeField, Range(0.3f, 2.0f)] private float _scoreboardScale = 0.8f;
        [SerializeField, Range(0.0f, 0.5f)] private float _lobbyPanelTop = 0.1f;
        [SerializeField] private Color _panelColor = new Color(0.0f, 0.0f, 0.0f, 0.55f);
        [SerializeField] private Color _shadowColor = new Color(0.0f, 0.0f, 0.0f, 0.8f);

        [Header("Results")]
        [SerializeField] private float _revealDelay = 2.0f;
        [SerializeField] private float _revealInterval = 0.5f;
        [SerializeField] private float _slideDuration = 0.35f;
        [SerializeField] private float _promptDelay = 0.5f;
        [SerializeField, Range(0.0f, 1.0f)] private float _resultsLeft = 0.55f;
        [SerializeField, Range(0.0f, 1.0f)] private float _resultsTop = 0.15f;

        private readonly List<BalloonCarrier> _ranking = new();
        private readonly List<BalloonCarrier> _results = new();
        private float _finishedTime;
        private float _timer;
        private int _lastCountdownSecond;
        private BalloonCarrier _winner;
        private float _scoreboardBottom;

        private GUIStyle _bigStyle;
        private GUIStyle _timerStyle;
        private GUIStyle _rowStyle;
        private GUIStyle _hintStyle;
        private GUIStyle _resultStyle;

        public eMatchState State { get; private set; } = eMatchState.Lobby;

        private void Start()
        {
            EnterLobby();
        }

        private void Update()
        {
            switch (State)
            {
                case eMatchState.Lobby:
                    if (WasHostStartPressed())
                    {
                        StartCountdown();
                    }
                    break;
                case eMatchState.Countdown:
                    _timer -= Time.deltaTime;
                    PlayCountdownTick();
                    if (_timer <= 0.0f)
                    {
                        StartPlaying();
                    }
                    break;
                case eMatchState.Playing:
                    _timer -= Time.deltaTime;
                    if (_timer <= 0.0f)
                    {
                        _timer = 0.0f;
                        ResolveTimeUp();
                    }
                    break;
                case eMatchState.SuddenDeath:
                    if (TryGetSingleLeader(out BalloonCarrier leader))
                    {
                        Finish(leader);
                    }
                    break;
                case eMatchState.Finished:
                    if (IsRevealComplete() && WasHostStartPressed())
                    {
                        StartCountdown();
                    }
                    break;
            }
        }

        private void EnterLobby()
        {
            State = eMatchState.Lobby;
            _winner = null;
            _stealSystem.StealingEnabled = false;
            _playerInputManager.EnableJoining();
            KartItems.ItemsEnabled = false;
            SetLobbyObjectsActive(true);
            AudioManager.Play(eMusic.Menu);
        }

        private void StartCountdown()
        {
            State = eMatchState.Countdown;
            _timer = _countdownDuration;
            _lastCountdownSecond = Mathf.CeilToInt(_timer);
            AudioManager.Play(eMusic.Countdown);
            AudioManager.Play(eSfx.CountdownTick);
            _winner = null;
            _stealSystem.StealingEnabled = false;
            _playerInputManager.DisableJoining();
            KartItems.ItemsEnabled = false;
            SetLobbyObjectsActive(false);
            _endSequence.Stop();
            _kartSpawner.SpawnBots();
            _kartSpawner.RespawnAll();

            foreach (BalloonCarrier carrier in BalloonCarrier.All)
            {
                carrier.ResetBalloons();
            }
            ResetAllItems();
        }

        private void StartPlaying()
        {
            State = eMatchState.Playing;
            _timer = _matchDuration;
            _stealSystem.StealingEnabled = true;
            KartItems.ItemsEnabled = true;
            AudioManager.Play(eMusic.Game);
            AudioManager.Play(eSfx.CountdownGo);
        }

        private void ResolveTimeUp()
        {
            if (TryGetSingleLeader(out BalloonCarrier leader))
            {
                Finish(leader);
            }
            else
            {
                State = eMatchState.SuddenDeath;
                AudioManager.SetMusicPitch(_suddenDeathMusicPitch);
            }
        }

        private void Finish(BalloonCarrier winner)
        {
            State = eMatchState.Finished;
            _winner = winner;
            _stealSystem.StealingEnabled = false;
            KartItems.ItemsEnabled = false;
            ResetAllItems();
            _finishedTime = Time.time;

            _results.Clear();
            _results.AddRange(BalloonCarrier.All);
            _results.Sort(CompareByScore);

            _endSequence.Play(winner);
            AudioManager.Play(eMusic.Victory);
        }

        private bool IsRevealComplete()
        {
            return Time.time >= GetRevealStart() + _results.Count * _revealInterval + _promptDelay;
        }

        private float GetRevealStart()
        {
            return _finishedTime + _revealDelay;
        }

        private static int CompareByScore(BalloonCarrier left, BalloonCarrier right)
        {
            int byCount = right.Count.CompareTo(left.Count);
            return byCount != 0 ? byCount : left.PlayerIndex.CompareTo(right.PlayerIndex);
        }

        private static void ResetAllItems()
        {
            foreach (BalloonCarrier carrier in BalloonCarrier.All)
            {
                if (carrier.TryGetComponent(out KartItems items))
                {
                    items.ResetItems();
                }
            }

            foreach (ItemPickup pickup in ItemPickup.All)
            {
                pickup.ResetSpot();
            }
        }

        private void PlayCountdownTick()
        {
            int second = Mathf.CeilToInt(_timer);
            if (second < _lastCountdownSecond && second > 0)
            {
                AudioManager.Play(eSfx.CountdownTick);
            }
            _lastCountdownSecond = second;
        }

        private void SetLobbyObjectsActive(bool active)
        {
            foreach (GameObject lobbyObject in _lobbyOnlyObjects)
            {
                if (lobbyObject != null)
                {
                    lobbyObject.SetActive(active);
                }
            }
        }

        public static bool TryGetSingleLeader(out BalloonCarrier leader)
        {
            leader = null;
            int best = -1;
            bool tied = false;

            foreach (BalloonCarrier carrier in BalloonCarrier.All)
            {
                if (carrier.Count > best)
                {
                    best = carrier.Count;
                    leader = carrier;
                    tied = false;
                }
                else if (carrier.Count == best)
                {
                    tied = true;
                }
            }

            return leader is not null && tied == false;
        }

        private bool WasHostStartPressed()
        {
            if (_kartSpawner.TryGetHostPlayer(out PlayerInput host) == false)
            {
                return false;
            }

            foreach (InputDevice device in host.devices)
            {
                if (device is Gamepad gamepad && gamepad.startButton.wasPressedThisFrame)
                {
                    return true;
                }

                if (device is Keyboard keyboard && keyboard.enterKey.wasPressedThisFrame)
                {
                    return true;
                }
            }

            return false;
        }

        private string GetHostName()
        {
            if (_kartSpawner.TryGetHostPlayer(out PlayerInput host) && host.TryGetComponent(out BalloonCarrier carrier))
            {
                return carrier.PlayerName;
            }

            return "P1";
        }

        private void OnGUI()
        {
            CreateStyles();
            if (State == eMatchState.Finished)
            {
                DrawResults();
                return;
            }

            DrawScoreboard();
            DrawTimer();
            DrawLobbyPanel();
            DrawCenterMessage();
        }

        private void CreateStyles()
        {
            int unit = Mathf.Max(12, Screen.height / 30);
            int rowSize = Mathf.Max(10, Mathf.RoundToInt(unit * _scoreboardScale));
            if (_bigStyle is not null && _bigStyle.fontSize == unit * 3 && _rowStyle.fontSize == rowSize)
            {
                return;
            }

            _bigStyle = new GUIStyle(GUI.skin.label) { fontSize = unit * 3, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _timerStyle = new GUIStyle(GUI.skin.label) { fontSize = unit * 2, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperCenter };
            _rowStyle = new GUIStyle(GUI.skin.label) { fontSize = rowSize, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            _hintStyle = new GUIStyle(GUI.skin.label) { fontSize = unit, alignment = TextAnchor.MiddleCenter };
            _resultStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(unit * 1.2f), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
        }

        private void DrawShadowLabel(Rect rect, string text, GUIStyle style, Color color)
        {
            float offset = Mathf.Max(1.0f, style.fontSize * 0.06f);
            style.normal.textColor = _shadowColor;
            GUI.Label(new Rect(rect.x + offset, rect.y + offset, rect.width, rect.height), text, style);
            style.normal.textColor = color;
            GUI.Label(rect, text, style);
        }

        private void DrawScoreboard()
        {
            _ranking.Clear();
            _ranking.AddRange(BalloonCarrier.All);
            _ranking.Sort(CompareByScore);

            float rowHeight = _rowStyle.fontSize * 1.6f;
            float swatchSize = rowHeight * 0.6f;
            float gap = _rowStyle.fontSize * 1.5f;
            float y = Screen.height * _scoreboardTop;

            float totalWidth = 0.0f;
            foreach (BalloonCarrier carrier in _ranking)
            {
                totalWidth += swatchSize + _rowStyle.fontSize * 0.4f + _rowStyle.CalcSize(new GUIContent(GetScoreText(carrier))).x + gap;
            }
            totalWidth -= gap;

            float x = (Screen.width - totalWidth) * 0.5f;
            foreach (BalloonCarrier carrier in _ranking)
            {
                string text = GetScoreText(carrier);
                float textWidth = _rowStyle.CalcSize(new GUIContent(text)).x;

                Color previous = GUI.color;
                GUI.color = _shadowColor;
                GUI.DrawTexture(new Rect(x + 2.0f, y + (rowHeight - swatchSize) * 0.5f + 2.0f, swatchSize, swatchSize), Texture2D.whiteTexture);
                GUI.color = carrier.PlayerColor;
                GUI.DrawTexture(new Rect(x, y + (rowHeight - swatchSize) * 0.5f, swatchSize, swatchSize), Texture2D.whiteTexture);
                GUI.color = previous;

                x += swatchSize + _rowStyle.fontSize * 0.4f;
                DrawShadowLabel(new Rect(x, y, textWidth + 4.0f, rowHeight), text, _rowStyle, Color.white);
                x += textWidth + gap;
            }

            _scoreboardBottom = _ranking.Count > 0 ? y + rowHeight : y;
        }

        private static string GetScoreText(BalloonCarrier carrier)
        {
            return $"{carrier.PlayerName}  {carrier.Count} pts";
        }

        private void DrawTimer()
        {
            if (State != eMatchState.Playing && State != eMatchState.SuddenDeath)
            {
                return;
            }

            string text = State == eMatchState.SuddenDeath ? "SUDDEN DEATH" : FormatTime(_timer);
            DrawShadowLabel(new Rect(0.0f, _scoreboardBottom, Screen.width, _timerStyle.fontSize * 1.5f), text, _timerStyle, Color.white);
        }

        private void DrawLobbyPanel()
        {
            if (State != eMatchState.Lobby)
            {
                return;
            }

            string title = "BALLOON HEIST";
            string joinLine = "Press any button to join";
            string startLine = _kartSpawner.TryGetHostPlayer(out PlayerInput _) ? $"Only {GetHostName()} can start  -  press START / ENTER" : "Waiting for players...";

            Vector2 titleSize = _bigStyle.CalcSize(new GUIContent(title));
            Vector2 joinSize = _hintStyle.CalcSize(new GUIContent(joinLine));
            Vector2 startSize = _hintStyle.CalcSize(new GUIContent(startLine));

            float padding = _hintStyle.fontSize;
            float width = Mathf.Max(titleSize.x, Mathf.Max(joinSize.x, startSize.x)) + padding * 2.0f;
            float height = titleSize.y + joinSize.y + startSize.y + padding * 2.0f;
            float top = Mathf.Max(Screen.height * _lobbyPanelTop, _scoreboardBottom + padding * 0.5f);
            Rect panel = new Rect((Screen.width - width) * 0.5f, top, width, height);

            Color previous = GUI.color;
            GUI.color = _panelColor;
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = previous;

            float y = panel.y + padding;
            DrawShadowLabel(new Rect(panel.x, y, panel.width, titleSize.y), title, _bigStyle, Color.white);
            y += titleSize.y;
            DrawShadowLabel(new Rect(panel.x, y, panel.width, joinSize.y), joinLine, _hintStyle, Color.white);
            y += joinSize.y;
            DrawShadowLabel(new Rect(panel.x, y, panel.width, startSize.y), startLine, _hintStyle, Color.white);
        }

        private void DrawCenterMessage()
        {
            Rect center = new Rect(0.0f, Screen.height * 0.35f, Screen.width, _bigStyle.fontSize * 1.5f);

            switch (State)
            {
                case eMatchState.Countdown:
                    DrawShadowLabel(center, Mathf.CeilToInt(_timer).ToString(), _bigStyle, Color.white);
                    break;
            }
        }

        private void DrawResults()
        {
            float left = Screen.width * _resultsLeft;
            float width = Screen.width * 0.95f - left;
            float padding = _resultStyle.fontSize * 0.6f;
            float rowHeight = _resultStyle.fontSize * 1.7f;
            float titleHeight = _bigStyle.fontSize * 1.4f;
            float top = Screen.height * _resultsTop;
            float revealStart = GetRevealStart();
            float now = Time.time;
            int count = _results.Count;

            float winnerReveal = revealStart + (count - 1) * _revealInterval;
            float titleProgress = Mathf.Clamp01((now - winnerReveal) / _slideDuration);
            if (titleProgress > 0.0f && _winner is not null)
            {
                float slide = (1.0f - EaseOutCubic(titleProgress)) * (Screen.width - left);
                Color winnerColor = _winner.PlayerColor;
                DrawShadowLabel(new Rect(left + slide, top, width, titleHeight), $"{_winner.PlayerName} WINS!", _bigStyle, winnerColor);
            }

            float rowsTop = top + titleHeight + padding;
            if (now >= revealStart && count > 0)
            {
                float visibleRows = Mathf.Clamp(Mathf.Floor((now - revealStart) / _revealInterval) + 1.0f, 0.0f, count);
                float panelHeight = visibleRows * rowHeight + padding * 2.0f;
                Rect panel = new Rect(left, rowsTop + (count - visibleRows) * rowHeight, width, panelHeight);
                Color previous = GUI.color;
                GUI.color = _panelColor;
                GUI.DrawTexture(panel, Texture2D.whiteTexture);
                GUI.color = previous;
            }

            for (int rank = 0; rank < count; rank++)
            {
                float rowReveal = revealStart + (count - 1 - rank) * _revealInterval;
                float progress = Mathf.Clamp01((now - rowReveal) / _slideDuration);
                if (progress <= 0.0f)
                {
                    continue;
                }

                BalloonCarrier carrier = _results[rank];
                float slide = (1.0f - EaseOutCubic(progress)) * (Screen.width - left);
                float x = left + padding + slide;
                float y = rowsTop + padding + rank * rowHeight;
                float swatchSize = rowHeight * 0.55f;

                DrawShadowLabel(new Rect(x, y, rowHeight * 1.6f, rowHeight), $"#{rank + 1}", _resultStyle, Color.white);
                x += rowHeight * 1.6f;

                Color previous = GUI.color;
                GUI.color = _shadowColor;
                GUI.DrawTexture(new Rect(x + 2.0f, y + (rowHeight - swatchSize) * 0.5f + 2.0f, swatchSize, swatchSize), Texture2D.whiteTexture);
                GUI.color = carrier.PlayerColor;
                GUI.DrawTexture(new Rect(x, y + (rowHeight - swatchSize) * 0.5f, swatchSize, swatchSize), Texture2D.whiteTexture);
                GUI.color = previous;
                x += swatchSize + padding;

                DrawShadowLabel(new Rect(x, y, width, rowHeight), $"{carrier.PlayerName}   {carrier.Count} pts", _resultStyle, Color.white);
            }

            if (IsRevealComplete())
            {
                float promptY = rowsTop + padding * 3.0f + count * rowHeight;
                DrawShadowLabel(new Rect(left, promptY, width, _hintStyle.fontSize * 2.0f), $"Only {GetHostName()} can restart  -  press START / ENTER", _hintStyle, Color.white);
            }
        }

        private static float EaseOutCubic(float t)
        {
            float inverse = 1.0f - t;
            return 1.0f - inverse * inverse * inverse;
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.CeilToInt(seconds);
            return $"{total / 60}:{total % 60:00}";
        }
    }
}
