using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using NessusClient;

namespace NessusPowerShell
{
    /// <summary>
    /// <para type="synopsis">In memory profile</para>
    /// </summary>
    public class NessusProfile
    {
        /// <summary>
        /// <para type="description">Nessus server</para>
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// <para type="description">Nessus Port. By default 8834</para>
        /// </summary>
        public int Port { get; set; } = 8834;

        /// <summary>
        /// <para type="description">User Name</para>
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// <para type="description">Password as SecureString</para>
        /// </summary>
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
                var formatter = new BinaryFormatter();
                var profileData = new ProfileData
                {
                    Server = Server,
                    Port = Port,
                    UserName = UserName,
                    SecureString = Password.ToBytes()
                };
                formatter.Serialize(ms, profileData);

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
                var formatter = new BinaryFormatter();
                profileData = (ProfileData) formatter.Deserialize(ms);                                
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
