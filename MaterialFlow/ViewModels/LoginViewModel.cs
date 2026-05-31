using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaterialFlow.Models;
using MaterialFlow.Services;

namespace MaterialFlow.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private string _login = string.Empty;
    private string _password = string.Empty;
    private string _fullName = string.Empty;
    private string _message = string.Empty;
    private bool _isRegisterMode;
    private UserRole _selectedRole = UserRole.Editor;

    public string Login
    {
        get => _login;
        set { _login = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    public string FullName
    {
        get => _fullName;
        set { _fullName = value; OnPropertyChanged(); }
    }

    public string Message
    {
        get => _message;
        set { _message = value; OnPropertyChanged(); }
    }

    public UserRole SelectedRole
    {
        get => _selectedRole;
        set { _selectedRole = value; OnPropertyChanged(); }
    }

    public Array AvailableRoles => Enum.GetValues(typeof(UserRole));

    public bool IsRegisterMode
    {
        get => _isRegisterMode;
        set 
        { 
            _isRegisterMode = value; 
            OnPropertyChanged(); 
            OnPropertyChanged(nameof(IsLoginMode));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(ButtonText));
            OnPropertyChanged(nameof(SwitchText));
        }
    }

    public bool IsLoginMode => !IsRegisterMode;
    public string Title => LocalizationService.Get(IsRegisterMode ? "LoginTitleRegister" : "LoginTitleLogin", IsRegisterMode ? "Create Account" : "Login");
    public string ButtonText => LocalizationService.Get(IsRegisterMode ? "LoginBtnRegister" : "LoginBtnLogin", IsRegisterMode ? "Register" : "Login");
    public string SwitchText => LocalizationService.Get(IsRegisterMode ? "LoginSwitchToLogin" : "LoginSwitchToRegister",
        IsRegisterMode ? "Already have an account? Login" : "Don't have an account? Register");

    /// <summary>
    /// Виконує реєстрацію нового акаунта або вхід в існуючий профіль через AuthService.
    /// </summary>
    /// <returns>Значення true у разі успішного входу; false при помилці або завершенні реєстрації.</returns>
    public bool Authenticate()
    {
        if (IsRegisterMode)
        {
            var result = AuthService.Instance.Register(Login, Password, FullName, SelectedRole);
            Message = LocalizationService.Get(result.Message, result.Message);
            if (result.Success)
            {
                IsRegisterMode = false; // Switch to login after registration
            }
            return false; // Don't close window yet
        }
        else
        {
            var result = AuthService.Instance.Login(Login, Password);
            Message = LocalizationService.Get(result.Message, result.Message);
            return result.Success;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
