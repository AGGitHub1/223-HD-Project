using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleReactionMachine
{
    public class EnhancedReactionController : IController
    {
        private IGui _gui;
        private IRandom _random;
        private IState _currentState;
        private int _ticksUntilStart;
        private int _elapsedTime;
        private int _displayTimeCounter;  // Manage display persistence after stopping
        private int _gameCount = 0;
        private List<double> _gameTimes = new List<double>(); // Store the times for each game

        public void Connect(IGui gui, IRandom rng)
        {
            _gui = gui;
            _random = rng;
            _gui.Connect(this);
            _currentState = new Idle(this);

        }

        public void Init()
        {
            ResetGame();
        }

        private void ResetGame()
        {
            _currentState = new Idle(this);
            _gui.SetDisplay("Insert coin");
            _gameTimes.Clear();
            _gameCount = 0;
        }
        public void CoinInserted() => _currentState.CoinInserted();
        public void GoStopPressed() => _currentState.GoStopPressed();
        public void Tick() => _currentState.Tick();

        private interface IState
        {
            void CoinInserted();
            void GoStopPressed();
            void Tick();
        }

        private class Idle : IState
        {
            private EnhancedReactionController _controller;
            public Idle(EnhancedReactionController controller) { _controller = controller; }
            public void CoinInserted()
            {
                _controller._currentState = new Ready(_controller);
                _controller._gui.SetDisplay("Press GO!");
            }
            public void GoStopPressed() { }
            public void Tick() { }
        }

        private class Ready : IState
        {
            private EnhancedReactionController _controller;
            public Ready(EnhancedReactionController controller) { _controller = controller; }
            public void CoinInserted() { }
            public void GoStopPressed()
            {
                _controller._currentState = new Wait(_controller, _controller._random);
                _controller._gui.SetDisplay("Wait...");
            }
            public void Tick() { }
        }

        private class Wait : IState
        {
            private EnhancedReactionController _controller;
            private IRandom _random;
            private int _ticksUntilStart;

            public Wait(EnhancedReactionController controller, IRandom random)
            {
                _controller = controller;
                _random = random;
                _ticksUntilStart = _random.GetRandom(100, 250);
            }
            public void CoinInserted() { }
            public void GoStopPressed() //if the user presses the button before the time is up, reset the game
            {
                _controller.ResetGame();
            }
            public void Tick()
            {
                if (--_ticksUntilStart <= 0) //if the Wait.. finished without the user pressing the button, go to the next state Run
                {
                    _controller._currentState = new Run(_controller);
                }
            }
        }

        private class Run : IState
        {
            private EnhancedReactionController _controller;
            private int _elapsedTime = 0;  // Initialised to start counting from zero

            public Run(EnhancedReactionController controller)
            {
                _controller = controller;
                _controller._gui.SetDisplay("0.00");  // Ensure this happens exactly when Run starts
                _elapsedTime = 0;
            }

            public void CoinInserted() { }

            public void GoStopPressed()
            {
                _controller._gameTimes.Add(_elapsedTime * 0.01); // Add current time to list of game times
                _controller._currentState = new Interim(_controller, _elapsedTime);
                _controller._gui.SetDisplay($"{_elapsedTime * 0.01:F2}");
            }

            public void Tick()
            {
                _elapsedTime++;
                _controller._gui.SetDisplay($"{_elapsedTime * 0.01:F2}");

                if (_elapsedTime > 199)
                {

                    _controller._gameTimes.Add(_elapsedTime * 0.01);
                    _controller._currentState = new Interim(_controller, _elapsedTime);
                    _elapsedTime = 0;  // Reset to prevent carry-over
                }
            }
        }

        private class Interim : IState
        {
            private EnhancedReactionController _controller;
            private int _displayTimeCounter;

            public Interim(EnhancedReactionController controller, int elapsedTime)
            {
                _controller = controller;
                _displayTimeCounter = 300;  // Ensure this starts at 299 for a full 3 seconds display
                _controller._gui.SetDisplay($"{elapsedTime * 0.01:F2}");  // Immediately display the time
            }

            public void CoinInserted()
            { }

            public void GoStopPressed()
            {
                if (_controller._gameCount < 2)  // Check if it's before the third game
                {
                    _controller._gameCount++;
                    _controller._currentState = new Wait(_controller, _controller._random);
                    _controller._gui.SetDisplay("Wait...");
                }
                else
                {
                    // Transition to FinalScore if it was the third game
                    _controller._currentState = new FinalScore(_controller);
                }
            }

            public void Tick()
            {
                if (--_displayTimeCounter <= 0)
                {
                    if (_controller._gameCount < 2)  // If currently on the first or second game (0 or 1)
                    {
                        _controller._gameCount++;
                        _controller._currentState = new Wait(_controller, _controller._random);
                        _controller._gui.SetDisplay("Wait...");
                    }
                    else
                    {
                        // Move to FinalScore state after 3 games
                        _controller._currentState = new FinalScore(_controller);
                    }
                }
            }
        }
        private class FinalScore : IState
        {
            private EnhancedReactionController _controller;
            private int _displayTimeCounter = 500;  // 5 seconds

            public FinalScore(EnhancedReactionController controller)
            {
                _controller = controller;
                double averageTime = _controller._gameTimes.Average();
                _controller._gui.SetDisplay($"{averageTime:F2}");
            }

            public void CoinInserted()
            { }

            public void GoStopPressed()
            {
                _controller.ResetGame();
            }

            public void Tick()
            {
                if (--_displayTimeCounter <= 0)
                {
                    _controller.ResetGame(); // Reset game if 5 seconds pass without any interaction
                }
            }
        }
    }
}




