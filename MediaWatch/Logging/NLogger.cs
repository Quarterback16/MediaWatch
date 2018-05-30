using NLog;
using System;

namespace MediaWatch.Logging
{
    public class NLogger : ILog
    {
        private Logger _logger;

        public NLogger()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Warning(string message)
        {
            _logger.Warn(message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Exception(Exception exception)
        {
            //_logger.DebugException( exception );
        }
    }
}