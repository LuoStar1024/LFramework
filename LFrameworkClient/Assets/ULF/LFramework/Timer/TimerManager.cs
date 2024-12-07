namespace LFramework
{
    internal sealed class TimerManager : LFrameworkModule, ITimerManager
    {
        private readonly TimerPool _timerPool;

        public TimerManager()
        {
            _timerPool = new TimerPool();
        }
        
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            _timerPool.Update(elapseSeconds, realElapseSeconds);
        }

        internal override void Shutdown()
        {
            _timerPool.Shutdown();
        }

        public int AddTimer(float time, LFrameworkActionArgs callback, int repeatCount = 1, bool isUnscaled = false,
            params object[] args)
        {
            return _timerPool.AddTimer(time, callback, repeatCount, isUnscaled, args);
        }

        public void Stop(int timerId)
        {
            _timerPool.Stop(timerId);
        }

        public void Resume(int timerId)
        {
            _timerPool.Resume(timerId);
        }

        public bool IsRunning(int timerId)
        {
            return _timerPool.IsRunning(timerId);
        }

        public float GetLeftTime(int timerId)
        {
            return _timerPool.GetLeftTime(timerId);
        }

        public void Restart(int timerId)
        {
            _timerPool.Restart(timerId);
        }

        public void Reset(int timerId, float time, LFrameworkActionArgs callback, int repeatCount = 1, bool isUnscaled = false)
        {
            _timerPool.Reset(timerId, time, callback, repeatCount, isUnscaled);
        }

        public void Reset(int timerId, float time, int repeatCount = 1, bool isUnscaled = false)
        {
            _timerPool.Reset(timerId, time, repeatCount, isUnscaled);
        }

        public void RemoveTimer(int timerId)
        {
            _timerPool.RemoveTimer(timerId);
        }

        public void RemoveAllTimer()
        {
            _timerPool.RemoveAllTimer();
        }
    }
}


