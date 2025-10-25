using BootManager.Core.ValueObjects;

namespace BootManager.Core.Interfaces;

public interface IPasswordHasher
{
    HashResult Hash(string password);
    bool Verify(string password, HashResult stored);
}