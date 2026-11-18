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
        [SerializeField] private float _matchDuration = 180.0f;
        [SerializeField] private float _countdownDuration = 3.0f;

        private readonly List<BalloonCarrier> _ranking = new();
        private float _timer;
        private BalloonCarrier _winner;

        private GUIStyle _bigStyle;
        private GUIStyle _timerStyle;
        private GUIStyle _rowStyle;
        private GUIStyle _hintStyle;

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
                    if (WasStartPressed() && BalloonCarrier.All.Count > 0)
                    {
                        StartCountdown();
                    }
                    break;
                case eMatchState.Countdown:
                    _timer -= Time.deltaTime;
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
                    if (WasStartPressed())
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
        }

        private void StartCountdown()
        {
            State = eMatchState.Countdown;
            _timer = _countdownDuration;
            _winner = null;
            _stealSystem.StealingEnabled = false;
            _playerInputManager.DisableJoining();

            foreach (BalloonCarrier carrier in BalloonCarrier.All)
            {
                carrier.ResetBalloons();
            }
        }

        private void StartPlaying()
        {
            State = eMatchState.Playing;
            _timer = _matchDuration;
            _stealSystem.StealingEnabled = true;
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
            }
        }

        private void Finish(BalloonCarrier winner)
        {
            State = eMatchState.Finished;
            _winner = winner;
            _stealSystem.StealingEnabled = false;
        }

        private bool TryGetSingleLeader(out BalloonCarrier leader)
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

        private static bool WasStartPressed()
        {
            foreach (Gamepad gamepad in Gamepad.all)
            {
                if (gamepad.startButton.wasPressedThisFrame)
                {
                    return true;
                }
            }

            Keyboard keyboard = Keyboard.current;
            return keyboard is not null && keyboard.enterKey.wasPressedThisFrame;
        }

        private void OnGUI()
        {
            CreateStyles();
            DrawScoreboard();
            DrawTimer();
            DrawCenterMessage();
        }

        private void CreateStyles()
        {
            int unit = Mathf.Max(12, Screen.height / 30);
            if (_bigStyle is not null && _bigStyle.fontSize == unit * 3)
            {
                return;
            }

            _bigStyle = new GUIStyle(GUI.skin.label) { fontSize = unit * 3, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _timerStyle = new GUIStyle(GUI.skin.label) { fontSize = unit * 2, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperCenter };
            _rowStyle = new GUIStyle(GUI.skin.label) { fontSize = unit, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            _hintStyle = new GUIStyle(GUI.skin.label) { fontSize = unit, alignment = TextAnchor.MiddleCenter };
            _bigStyle.normal.textColor = Color.white;
            _timerStyle.normal.textColor = Color.white;
            _rowStyle.normal.textColor = Color.white;
            _hintStyle.normal.textColor = Color.white;
        }

        private void DrawScoreboard()
        {
            _ranking.Clear();
            _ranking.AddRange(BalloonCarrier.All);
            _ranking.Sort((left, right) => right.Count.CompareTo(left.Count));

            float rowHeight = _rowStyle.fontSize * 1.6f;
            float x = 20.0f;
            float y = 20.0f;

            foreach (BalloonCarrier carrier in _ranking)
            {
                Color previous = GUI.color;
                GUI.color = carrier.PlayerColor;
                GUI.DrawTexture(new Rect(x, y + rowHeight * 0.15f, rowHeight * 0.7f, rowHeight * 0.7f), Texture2D.whiteTexture);
                GUI.color = previous;

                GUI.Label(new Rect(x + rowHeight, y, 400.0f, rowHeight), $"{carrier.PlayerName}   {carrier.Count}", _rowStyle);
                y += rowHeight;
            }
        }

        private void DrawTimer()
        {
            if (State != eMatchState.Playing && State != eMatchState.SuddenDeath)
            {
                return;
            }

            string text = State == eMatchState.SuddenDeath ? "SUDDEN DEATH" : FormatTime(_timer);
            GUI.Label(new Rect(0.0f, 10.0f, Screen.width, _timerStyle.fontSize * 1.5f), text, _timerStyle);
        }

        private void DrawCenterMessage()
        {
            Rect center = new Rect(0.0f, Screen.height * 0.35f, Screen.width, _bigStyle.fontSize * 1.5f);
            Rect hint = new Rect(0.0f, center.yMax, Screen.width, _hintStyle.fontSize * 2.0f);

            switch (State)
            {
                case eMatchState.Lobby:
                    GUI.Label(center, "BALLOON HEIST", _bigStyle);
                    GUI.Label(hint, "Press any button to join   -   START / ENTER to begin", _hintStyle);
                    break;
                case eMatchState.Countdown:
                    GUI.Label(center, Mathf.CeilToInt(_timer).ToString(), _bigStyle);
                    break;
                case eMatchState.Finished:
                    Color previous = _bigStyle.normal.textColor;
                    _bigStyle.normal.textColor = _winner is not null ? _winner.PlayerColor : Color.white;
                    GUI.Label(center, _winner is not null ? $"{_winner.PlayerName} WINS!" : "DRAW", _bigStyle);
                    _bigStyle.normal.textColor = previous;
                    GUI.Label(hint, "START / ENTER for a rematch", _hintStyle);
                    break;
            }
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.CeilToInt(seconds);
            return $"{total / 60}:{total % 60:00}";
        }
    }
}
