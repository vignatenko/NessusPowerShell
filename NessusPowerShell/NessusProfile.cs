using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NessusClient;

namespace NessusPowerShell
{
    
    public class NessusProfile
    {
        
        public string Server { get; set; }
        
        public int Port { get; set; } = 8834;
        
        public string UserName { get; set; }

        
        public SecureString Password { get; set; }

        public string ToProtectedString()
        {
            var entropy = new byte[new Random().Next(500, 1000)];
            using (var gen = RandomNumberGenerator.Create())
            {
                gen.GetNonZeroBytes(entropy);
            }
            
            var buf = new List<byte>(entropy.Length + 4);
            buf.AddRange(BitConverter.GetBytes(entropy.Length));
            buf.AddRange(entropy);
            
            using (var ms = new MemoryStream())
            {
                

                var f = new BinaryFormatter();
                var profileData = new ProfileData
                {
                    Server = Server,
                    Port = Port,
                    UserName = UserName,
                    SecureString = Password.ToBytes()
                };
                f.Serialize(ms, profileData);

                Array.Clear(profileData.SecureString, 0, profileData.SecureString.Length);

                var binaryProfile = ms.ToArray();
                buf.AddRange(ProtectedData.Protect(binaryProfile, entropy, DataProtectionScope.CurrentUser));
                
            }
            return Convert.ToBase64String(buf.ToArray());
        }

        public static NessusProfile FromProtectedString(string base64)
        {
            var data = Convert.FromBase64String(base64);
            var entropyLen = BitConverter.ToInt32(data.Take(4).ToArray(), 0);
            var entropy = data.Skip(4).Take(entropyLen).ToArray();
            var cipher = data.Skip(4 + entropyLen).ToArray();
            ProfileData profileData;
            using (var ms = new MemoryStream(ProtectedData.Unprotect(cipher, entropy, DataProtectionScope.CurrentUser)))
            {                
                var f = new BinaryFormatter();
                profileData = (ProfileData) f.Deserialize(ms);                                
            }
            return new NessusProfile
            {
                Server = profileData.Server,
                Port = profileData.Port,
                UserName = profileData.UserName,
                Password = profileData.SecureString.ToSecureString()
            };
        }
        [Serializable]
        private class ProfileData
        {
            public string Server;
            public int Port;
            public string UserName;
            public byte[] SecureString;

        }



    }
}
