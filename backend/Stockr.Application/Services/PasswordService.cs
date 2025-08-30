using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Konscious.Security.Cryptography;

namespace Stockr.Application.Services;

public interface IPasswordService
{
    /// <summary>
    /// Gera hash seguro da senha usando Argon2id
    /// </summary>
    /// <param name="password">Senha em texto plano</param>
    /// <returns>Hash da senha no formato Argon2id</returns>
    Task<string> HashPassword(string password);
    
    /// <summary>
    /// Verifica se a senha em texto plano corresponde ao hash
    /// </summary>
    /// <param name="password">Senha em texto plano</param>
    /// <param name="hashedPassword">Hash da senha armazenada</param>
    /// <returns>True se a senha corresponde ao hash</returns>
    Task<bool> VerifyPassword(string password, string hashedPassword);
    
    /// <summary>
    /// Valida se a senha atende aos critérios de segurança
    /// </summary>
    /// <param name="password">Senha em texto plano</param>
    /// <returns>True se a senha é válida</returns>
    bool IsValidPassword(string password);
}

public class PasswordService : IPasswordService
{
    // Configurações recomendadas para Argon2id
    private const int SaltSize = 32;
    private const int KeySize = 32;
    private const int Iterations = 4;
    private const int MemorySize = 65536; // 64 MB
    private const int DegreeOfParallelism = 1;
    
    public async Task<string> HashPassword(string password)
    {
        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            Iterations = Iterations,
            MemorySize = MemorySize
        };

        var hash = await argon2.GetBytesAsync(KeySize);

        var result = new byte[SaltSize + KeySize];
        Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, result, SaltSize, KeySize);

        return Convert.ToBase64String(result);
    }

    public async Task<bool> VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);
            
            if (hashBytes.Length != SaltSize + KeySize)
                return false;

            var salt = new byte[SaltSize];
            var storedHash = new byte[KeySize];
            
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(hashBytes, SaltSize, storedHash, 0, KeySize);

            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = DegreeOfParallelism,
                Iterations = Iterations,
                MemorySize = MemorySize
            };

            var computedHash = await argon2.GetBytesAsync(KeySize);

            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
        catch
        {
            return false;
        }
    }
    
    public bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;
            
        // Critérios de validação:
        // - Mínimo 8 caracteres
        // - Pelo menos uma letra minúscula
        // - Pelo menos uma letra maiúscula
        // - Pelo menos um número
        // - Pelo menos um caractere especial
        
        if (password.Length < 8)
            return false;
            
        var hasLowercase = Regex.IsMatch(password, @"[a-z]");
        var hasUppercase = Regex.IsMatch(password, @"[A-Z]");
        var hasNumber = Regex.IsMatch(password, @"\d");
        var hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");
        
        return hasLowercase && hasUppercase && hasNumber && hasSpecialChar;
    }
}