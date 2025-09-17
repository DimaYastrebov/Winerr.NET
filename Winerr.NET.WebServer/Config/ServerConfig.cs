using YamlDotNet.Serialization;

namespace Winerr.NET.WebServer.Config
{
    public enum IpFilterMode { Disabled, Whitelist, Blacklist }

    public class ServerConfig
    {
        [YamlMember(Alias = "server")]
        public ServerSettings Server { get; set; } = new();

        [YamlMember(Alias = "ipFilter")]
        public IpFilterSettings IpFilter { get; set; } = new();

        [YamlMember(Alias = "authentication")]
        public AuthSettings Authentication { get; set; } = new();
    }

    public class ServerSettings
    {
        public string Url { get; set; } = "http://localhost:5000";
    }

    public class IpFilterSettings
    {
        public bool Enabled { get; set; } = false;
        public IpFilterMode Mode { get; set; } = IpFilterMode.Blacklist;
        public List<string> IpList { get; set; } = new();
        public int BlockResponseCode { get; set; } = 403;
        public string BlockResponseMessage { get; set; } = "Access denied.";
    }

    public class AuthSettings
    {
        public bool Enabled { get; set; } = false;
        public string ApiKey { get; set; } = "change-this-secret-key";
    }
}