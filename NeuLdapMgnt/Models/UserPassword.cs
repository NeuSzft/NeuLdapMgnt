using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NeuLdapMgnt.Models;

/// <summary>Stores the SHA-512 hash and salt of a user's password and provides methods for converting it to and from Modular Crypt Format.</summary>
/// <remarks>The format used is "<c>$6${encodedPasswordSalt}${encodedPasswordHash}</c>".</remarks>
public sealed partial class UserPassword {
    /// <summary>The SHA-512 hash of the password.</summary>
    public byte[] Hash { get; private set; } = { };

    /// <summary>The salt used for the hashing of the password.</summary>
    public byte[] Salt { get; private set; } = { };

    /// <summary>Determines whether the user's account is locked.</summary>
    public bool Locked { get; set; }

    /// <summary>Initializes a new <see cref="UserPassword"/>.</summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="saltLength">The length of the salt to generate in bytes.</param>
    /// <param name="locked">Determines whether the user's account is locked.</param>
    public UserPassword(string password, int saltLength, bool locked = false) {
        SetPassword(password, saltLength);
        Locked = locked;
    }

    /// <summary>Initializes a new <see cref="UserPassword"/> from a string in the Modular Crypt Format.</summary>
    /// <param name="mcf">The password string in the "<c>$6${encodedPasswordSalt}${encodedPasswordHash}</c>" format.</param>
    /// <exception cref="FormatException"><paramref name="mcf"/> is in an incorrect format.</exception>
    /// <exception cref="ArgumentException">The hashing algorithm specified in <see cref="mcf"/> is not SHA-512.</exception>
    public UserPassword(string mcf) {
        string[] splitMcf = mcf.Split('$');

        if (splitMcf.Length != 4)
            throw new FormatException("The string is in an incorrect format");
        if (splitMcf[1] != "6")
            throw new ArgumentException("The hashing algorithm must be SHA-512");

        Locked = splitMcf[0].Contains('!');
        Salt   = DecodeBytes(splitMcf[2]);
        Hash   = DecodeBytes(splitMcf[3]);
    }

    /// <summary>Sets the password hash and salt.</summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="saltLength">The length of the salt to generate in bytes.</param>
    public void SetPassword(string password, int saltLength = 16) {
        Salt = RandomNumberGenerator.GetBytes(saltLength);
        Hash = SHA512.HashData(Encoding.UTF8.GetBytes(password).Concat(Salt).ToArray());
    }

    /// <summary>Checks if the passwords match.</summary>
    /// <param name="password">The password to check.</param>
    /// <returns><c>true</c> if the passwords match and <c>false</c> if they do not.</returns>
    public bool CheckPassword(string password) {
        byte[] hash = SHA512.HashData(Encoding.UTF8.GetBytes(password).Concat(Salt).ToArray());
        return hash.SequenceEqual(Hash);
    }

    /// <summary>Returns the password in the Modular Crypt Format.</summary>
    /// <returns>The password formatted as "<c>$6${encodedPasswordSalt}${encodedPasswordHash}</c>".</returns>
    public override string ToString() {
        return $"{(Locked ? "!" : string.Empty)}$6${EncodeBytes(Salt)}${EncodeBytes(Hash)}";
    }
}
