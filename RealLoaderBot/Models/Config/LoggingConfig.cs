using Microsoft.Extensions.Configuration;

namespace RealLoaderBot.Config {
    public class LoggingConfig {
        public string LogFile { get; set; }
        public bool WriteLogFile { get; set; }
        public bool DebugLogs { get; set; }

        public LoggingConfig(IConfiguration configuration) {
            configuration.GetSection(nameof(LoggingConfig)).Bind(this);
        }
    }
}

