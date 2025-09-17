using Serilog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Winerr.NET.WebServer.Config
{
    public static class ConfigLoader
    {
        private const string ConfigFileName = "config.yml";

        public static ServerConfig LoadConfig(Serilog.ILogger logger)
        {
            if (!File.Exists(ConfigFileName))
            {
                logger.Warning("Config file '{file}' not found. Creating a default one.", ConfigFileName);

                var defaultConfigContent = @"
# =========================================
# Winerr.NET WebServer Configuration
# =========================================

server:
  # The URL on which the server will run.
  # 'localhost' - accessible only from this computer.
  # '0.0.0.0' - accessible from all network interfaces (for example, by IP in the local network).
  url: http://localhost:5000

ipFilter:
  # Enables or disables the IP filter.
  # true - enabled, false - disabled.
  enabled: false

  # Filter mode:
  # Blacklist - block IPs from the list below.
  # Whitelist - allow access ONLY to IPs from the list below.
  mode: Blacklist

  # List of IP addresses for the filter.
  ipList:
    # - 192.168.1.115 
    # - 127.0.0.1

  # HTTP response code for blocked IPs.
  blockResponseCode: 403

  # Message for blocked IPs.
  blockResponseMessage: Access denied by IP filter.

authentication:
  # Enables API key authentication.
  # If true, all requests must include a valid API key.
  enabled: false

  # The secret API key.
  # Send it via 'Authorization: Bearer <key>' header or '?api_key=<key>' query parameter.
  apiKey: change-this-secret-key-in-production
";
                var finalContent = defaultConfigContent.TrimStart();
                File.WriteAllText(ConfigFileName, finalContent);
            }

            logger.Information("Loading configuration from '{file}'...", ConfigFileName);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yamlContent = File.ReadAllText(ConfigFileName);
            var config = deserializer.Deserialize<ServerConfig>(yamlContent);

            if (config == null)
            {
                logger.Warning("Config file is empty or invalid. Using default settings.");
                return new ServerConfig();
            }

            config.Server ??= new ServerSettings();
            config.IpFilter ??= new IpFilterSettings();
            config.IpFilter.IpList ??= new List<string>();

            return config;
        }
    }
}
