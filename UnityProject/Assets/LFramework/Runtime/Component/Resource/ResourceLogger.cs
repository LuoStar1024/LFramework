namespace LFramework
{
    internal class ResourceLogger : YooAsset.ILogger
    {
        public void Log(string message)
        {
            LFramework.Log.Info(message);
        }

        public void Warning(string message)
        {
            LFramework.Log.Warning(message);
        }

        public void Error(string message)
        {
            LFramework.Log.Error(message);
        }

        public void Exception(System.Exception exception)
        {
            LFramework.Log.Fatal(exception.Message);
        }
    }
}