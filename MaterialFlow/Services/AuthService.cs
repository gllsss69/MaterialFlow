using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MaterialFlow.Models;

namespace MaterialFlow.Services;

public class AuthService
{
    private const string DataFolder = "Data";
    private const string UsersFile = "Data/users.json";
    private List<User> _users = new();
    private static AuthService? _instance;

    public static AuthService Instance => _instance ??= new AuthService();

    public User? CurrentUser { get; private set; }

    private AuthService()
    {
        InitializeData();
    }

    private void InitializeData()
    {
        if (!Directory.Exists(DataFolder))
        {
            Directory.CreateDirectory(DataFolder);
        }

        if (File.Exists(UsersFile))
        {
            try
            {
                var json = File.ReadAllText(UsersFile);
                _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch
            {
                _users = new List<User>();
            }
        }
        else
        {
            _users = new List<User>();
            SaveUsers();
        }
    }

    private void SaveUsers()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(_users, options);
        File.WriteAllText(UsersFile, json);
    }

    public (bool Success, string Message) Register(string login, string password, string fullName)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            return (false, "Login and password are required.");

        if (_users.Any(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase)))
            return (false, "User already exists.");

        var user = new User
        {
            Login = login,
            FullName = fullName,
            PasswordHash = HashPassword(password),
            Role = _users.Count == 0 ? UserRole.Admin : UserRole.Editor // First user is Admin
        };

        _users.Add(user);
        SaveUsers();
        return (true, "Registration successful.");
    }

    public (bool Success, string Message, User? User) Login(string login, string password)
    {
        var user = _users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
        if (user == null)
            return (false, "Invalid login or password.", null);

        if (VerifyPassword(password, user.PasswordHash))
        {
            CurrentUser = user;
            return (true, "Login successful.", user);
        }

        return (false, "Invalid login or password.", null);
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    private string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(20);
        
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);
        
        return Convert.ToBase64String(hashBytes);
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            byte[] hashBytes = Convert.ToBase64String(Encoding.UTF8.GetBytes(hashedPassword)) == hashedPassword ? 
                               Encoding.UTF8.GetBytes(hashedPassword) : // Legacy or wrong format handling
                               Convert.FromBase64String(hashedPassword);
            
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);
            
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                    return false;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
