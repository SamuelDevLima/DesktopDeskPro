using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using BCrypt.Net;
using System.Windows.Media;

namespace Pim_04.Views
{
    public partial class CadastroWindow : Window
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DeskProDb"].ConnectionString;
        private TextBox? passwordTextBox; // Para o campo Senha
        private TextBox? confirmPasswordTextBox; // Para o campo Confirmar Senha

        public CadastroWindow()
        {
            InitializeComponent();
        }

        private void BtnCadastrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string usuario = txtNomeUsuario.Text?.Trim() ?? "";
                string organizacao = txtOrganizacao.Text?.Trim() ?? "";
                string email = txtEmail.Text?.Trim() ?? "";
                string senha = txtSenha.Password;
                string confirmarSenha = txtConfirmarSenha.Password;

                if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(organizacao) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(confirmarSenha))
                {
                    MessageBox.Show("Preencha todos os campos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!usuario.All(c => char.IsLetterOrDigit(c) || c == '_'))
                {
                    MessageBox.Show("O nome de usuário deve conter apenas letras, números ou sublinhado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!email.Contains("@") || !email.Contains("."))
                {
                    MessageBox.Show("Digite um e-mail válido.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (senha != confirmarSenha)
                {
                    MessageBox.Show("As senhas não coincidem.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("INSERT INTO Users (Username, Organization, PasswordHash, Email, IsAdmin) VALUES (@username, @organization, @passwordHash, @email, 0)", connection);
                    command.Parameters.AddWithValue("@username", usuario);
                    command.Parameters.AddWithValue("@organization", organizacao);
                    command.Parameters.AddWithValue("@passwordHash", senhaHash);
                    command.Parameters.AddWithValue("@email", email);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Usuário cadastrado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        new LoginWindow().Show(); // Abre a tela de login
                        this.Close(); // Fecha a tela de cadastro
                    }
                    else
                    {
                        MessageBox.Show("Falha ao cadastrar usuário.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro durante o cadastro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void TxtNomeUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtOrganizacao.Focus();
            }
        }

        private void TxtOrganizacao_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtEmail.Focus();
            }
        }

        private void TxtEmail_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtSenha.Focus();
            }
        }

        private void TxtSenha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtConfirmarSenha.Focus();
            }
        }

        private void TxtConfirmarSenha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnCadastrar_Click(sender, new RoutedEventArgs());
            }
        }

        private void ChkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            if (txtSenha != null)
            {
                passwordTextBox = new TextBox
                {
                    Text = txtSenha.Password,
                    Height = 35,
                    Margin = txtSenha.Margin,
                    Foreground = txtSenha.Foreground,
                    Background = txtSenha.Background,
                    IsHitTestVisible = true
                };
                passwordTextBox.TextChanged += (s, args) => txtSenha.Password = passwordTextBox.Text;

                var parent = txtSenha.Parent as StackPanel;
                if (parent != null)
                {
                    int index = parent.Children.IndexOf(txtSenha);
                    parent.Children.Remove(txtSenha);
                    parent.Children.Insert(index, passwordTextBox);
                    txtSenha = null; // Reset reference to avoid issues
                }
            }
        }

        private void ChkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            if (passwordTextBox != null)
            {
                txtSenha = new PasswordBox
                {
                    Password = passwordTextBox.Text,
                    Height = 35,
                    Margin = passwordTextBox.Margin,
                    Foreground = passwordTextBox.Foreground,
                    Background = passwordTextBox.Background
                };
                var parent = passwordTextBox.Parent as StackPanel;
                if (parent != null)
                {
                    int index = parent.Children.IndexOf(passwordTextBox);
                    parent.Children.Remove(passwordTextBox);
                    parent.Children.Insert(index, txtSenha);
                }
                passwordTextBox = null;
            }
        }

        private void ChkShowConfirmPassword_Checked(object sender, RoutedEventArgs e)
        {
            if (txtConfirmarSenha != null)
            {
                confirmPasswordTextBox = new TextBox
                {
                    Text = txtConfirmarSenha.Password,
                    Height = 35,
                    Margin = txtConfirmarSenha.Margin,
                    Foreground = txtConfirmarSenha.Foreground,
                    Background = txtConfirmarSenha.Background,
                    IsHitTestVisible = true
                };
                confirmPasswordTextBox.TextChanged += (s, args) => txtConfirmarSenha.Password = confirmPasswordTextBox.Text;

                var parent = txtConfirmarSenha.Parent as StackPanel;
                if (parent != null)
                {
                    int index = parent.Children.IndexOf(txtConfirmarSenha);
                    parent.Children.Remove(txtConfirmarSenha);
                    parent.Children.Insert(index, confirmPasswordTextBox);
                    txtConfirmarSenha = null; // Reset reference to avoid issues
                }
            }
        }

        private void ChkShowConfirmPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            if (confirmPasswordTextBox != null)
            {
                txtConfirmarSenha = new PasswordBox
                {
                    Password = confirmPasswordTextBox.Text,
                    Height = 35,
                    Margin = confirmPasswordTextBox.Margin,
                    Foreground = confirmPasswordTextBox.Foreground,
                    Background = confirmPasswordTextBox.Background
                };
                var parent = confirmPasswordTextBox.Parent as StackPanel;
                if (parent != null)
                {
                    int index = parent.Children.IndexOf(confirmPasswordTextBox);
                    parent.Children.Remove(confirmPasswordTextBox);
                    parent.Children.Insert(index, txtConfirmarSenha);
                }
                confirmPasswordTextBox = null;
            }
        }
    }
}