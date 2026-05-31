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

    /// <summary>
    /// Ініціалізує директорію даних та завантажує користувачів із файлу users.json.
    /// </summary>
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

    /// <summary>
    /// Зберігає поточний список зареєстрованих користувачів у файл users.json.
    /// </summary>
    private void SaveUsers()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(_users, options);
        File.WriteAllText(UsersFile, json);
    }

    /// <summary>
    /// Реєструє нового користувача в системі із перевіркою унікальності логіну.
    /// </summary>
    /// <param name="login">Унікальний логін користувача.</param>
    /// <param name="password">Пароль користувача у відкритому вигляді.</param>
    /// <param name="fullName">Повне ім'я користувача.</param>
    /// <param name="role">Початкова роль (за замовчуванням Editor).</param>
    /// <returns>Кортеж з прапорцем успішності та описом результату.</returns>
    public (bool Success, string Message) Register(string login, string password, string fullName, UserRole role = UserRole.Editor)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            return (false, "AuthLoginPasswordRequired");

        if (_users.Any(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase)))
            return (false, "AuthUserAlreadyExists");

        var user = new User
        {
            Login = login,
            FullName = fullName,
            PasswordHash = HashPassword(password),
            Role = _users.Count == 0 ? UserRole.Admin : role // First user is always Admin, others use selected role
        };

        _users.Add(user);
        SaveUsers();
        return (true, "AuthRegistrationSuccessful");
    }

    /// <summary>
    /// Відновлює сесію користувача за його логіном (для автологіну при старті).
    /// </summary>
    /// <param name="login">Логін користувача.</param>
    public void RestoreSession(string login)
    {
        var user = _users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
        if (user != null)
        {
            CurrentUser = user;
        }
    }

    /// <summary>
    /// Виконує вхід у систему за допомогою логіну та пароля.
    /// </summary>
    /// <param name="login">Логін користувача.</param>
    /// <param name="password">Пароль користувача.</param>
    /// <returns>Кортеж із результатом входу та об'єктом авторизованого користувача.</returns>
    public (bool Success, string Message, User? User) Login(string login, string password)
    {
        var user = _users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
        if (user == null)
            return (false, "AuthInvalidLoginOrPassword", null);

        if (VerifyPassword(password, user.PasswordHash))
        {
            CurrentUser = user;
            return (true, "AuthLoginSuccessful", user);
        }

        return (false, "AuthInvalidLoginOrPassword", null);
    }

    /// <summary>
    /// Завершує поточну сесію та скидає авторизаційні дані користувача.
    /// </summary>
    public void Logout()
    {
        CurrentUser = null;
    }

    /// <summary>
    /// Хешує введений пароль користувача за алгоритмом PBKDF2-SHA256 із використанням солі.
    /// </summary>
    /// <param name="password">Пароль у відкритому вигляді.</param>
    /// <returns>Закодований у Base64 рядок, що містить сіль та хеш.</returns>
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

    /// <summary>
    /// Перевіряє відповідність введеного пароля збереженому хешу.
    /// </summary>
    /// <param name="password">Введений пароль.</param>
    /// <param name="hashedPassword">Збережений у базі даних хеш пароля.</param>
    /// <returns>Значення true, якщо пароль валідний; інакше false.</returns>
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
