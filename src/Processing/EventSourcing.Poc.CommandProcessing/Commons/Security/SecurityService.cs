using EventSourcing.Poc.Processing.Options;
using Microsoft.Extensions.Options;

namespace EventSourcing.Poc.Processing.Commons.Security {
    public class SecurityService : ISecurityService {
        private readonly string _key;
        private readonly bool _enable;

        public SecurityService(IOptions<SecurityServiceOptions> options) {
            _enable = options.Value.Encryption;
            _key = options.Value.Key;
        }

        public bool Enable => _enable;
        public string CreateIv() {
            return CryptographyExtensions.GenerateString(16);
        }

        public string Encrypt(string content, string iv) {
            return content.Encrypt(_key, iv);
        }

        public string Decrypt(string content, string iv) {
            return content.Decrypt(_key, iv);
        }
    }

    public interface ISecurityService {
        bool Enable { get; }
        string CreateIv();
        string Encrypt(string content, string iv);
        string Decrypt(string content, string iv);
    }
}