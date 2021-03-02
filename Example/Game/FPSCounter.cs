using System;
using Apos.Input;

namespace GameProject {
    class FPSCounter {
        public int FramesPerSecond { get; private set; } = 0;
        public int UpdatePerSecond { get; private set; } = 0;
        public double TimePerFrame { get; private set; } = 0;
        public double TimePerUpdate { get; private set; } = 0;
        public int DroppedFrames { get; set; } = 0;

        public void Update(long elapsedTickTime) {
            _updateCounter++;

            _totalTime += elapsedTickTime;
            timer += elapsedTickTime;
            if (timer <= TimeSpan.TicksPerSecond) {
                return;
            } else if (timer > TimeSpan.TicksPerSecond * 60) {
                // This fixes a case where the game stops being updated for a long time.
                // For example when the computer is in sleep mode.
                timer = TimeSpan.TicksPerSecond * 60;
                return;
            }

            UpdatePerSecond = _updateCounter;
            FramesPerSecond = _framesCounter;
            _updateCounter = 0;
            _framesCounter = 0;
            timer -= TimeSpan.TicksPerSecond;

            TimePerUpdate = Math.Truncate(1000d / UpdatePerSecond * 10000) / 10000;
            if (FramesPerSecond > 0) {
                TimePerFrame = Math.Truncate(1000d / FramesPerSecond * 10000) / 10000;
            }

            if (FramesPerSecond < 60 && _totalTime > 3000 && InputHelper.IsActive) {
                DroppedFrames += 60 - FramesPerSecond;
            }
        }
        public void Draw() {
            _framesCounter++;
        }

        private long timer = 0;
        private int _framesCounter = 0;
        private int _updateCounter = 0;
        private long _totalTime = 0;
    }
}
