using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;

namespace GenPhoto.Helpers
{
    internal class ScopedStopwatchLogger : IDisposable
    {
        private Stopwatch m_stopwatch;
        readonly ILogger m_logger;
        readonly LogLevel m_logLevel;
        StringBuilder m_text;
        List<object> m_parameters;

        public ScopedStopwatchLogger(ILogger logger, LogLevel logLevel, string text, params object[] parameters)
        {
            m_logLevel = logLevel;
            m_logger = logger;
            m_text = new StringBuilder();
            m_parameters = new List<object>();

            AppentText(text, parameters);

            m_stopwatch = Stopwatch.StartNew();
        }

        public ScopedStopwatchLogger AppentText(string text, params object[] parameters)
        {
            if (m_text.Length > 0)
            {
                m_text.Append(", ");
            }
            m_text.Append(text);

            m_parameters.AddRange(parameters);

            return this;
        }

        public void Dispose()
        {
            m_stopwatch.Stop();

            AppentText("Elapsed {} ms.", m_stopwatch.ElapsedMilliseconds);

            m_logger.Log(m_logLevel, m_text.ToString(), m_parameters.ToArray());
        }
    }
}
