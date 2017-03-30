using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace NessusPowerShell.Test
{
    [TestFixture]
    public class NessusProfileTest
    {

        [Test]
        public void FromUnprotecedString_ShouldReadProtectedStringCorrectly()
        {
            var pwd = new SecureString();
            foreach (var c in "2")
            {
                pwd.AppendChar(c);
            }
            var p = new NessusProfile {Server = "srv1", Password = pwd, UserName = "user1", Port = 990};
            var ps = p.ToProtectedString();
            var np = NessusProfile.FromProtectedString(ps);
        }
    }
}
