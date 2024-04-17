using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NeuLdapMgnt.Models;

/// <summary>Stores the SHA512 hash and salt of a user's password.</summary>
public sealed class UserPassword {
    /// <summary>The SHA512 hash of the password.</summary>
    public byte[] Hash { get; private set; } = { };

    /// <summary>The salt used for the hashing of the password.</summary>
    public byte[] Salt { get; private set; } = { };

    /// <summary>Initializes a new <see cref="UserPassword"/>.</summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="saltLength">The length of the salt to generate in bytes.</param>
    /// <param name="locked">Determines whether the user's account is locked.</param>
    public UserPassword(string password, int saltLength) {
        SetPassword(password, saltLength);
    }

    /// <summary>Initializes a new <see cref="UserPassword"/> from a LDAP password hashed with SSHA512.</summary>
    /// <param name="ldapPassword">The password string in the <c>{SSHA512}</c> format.</param>
    /// <exception cref="FormatException"><paramref name="ldapPassword"/> is in an incorrect format.</exception>
    public UserPassword(string ldapPassword) {
        if (!ldapPassword.StartsWith("{SSHA512}"))
            throw new FormatException("The password string is not in the SSHA512 format");

        byte[] bytes = Convert.FromBase64String(ldapPassword[9..]);
        Hash = bytes[..64];
        Salt = bytes[64..];
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

    /// <summary>Converts the password hash and salt into a string.</summary>
    /// <returns>The password formatted to <c>{SSHA512}</c> .</returns>
    public override string ToString() {
        return "{SSHA512}" + Convert.ToBase64String(Hash.Concat(Salt).ToArray());
    }
}
