namespace Api.Logger
{
    public static class FileLoggerExtensions
    {
        /// <summary>
        /// Adds a file logger.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="fileName">log file name.</param>
        /// <param name="append">if true new log entries are appended to the existing file.</param>	 
        public static ILoggingBuilder AddFile(this ILoggingBuilder factory, string fileName, bool append = true)
        {
            factory.AddProvider(new FileLoggerProvider(fileName, append));
            return factory;
        }
    }
}
