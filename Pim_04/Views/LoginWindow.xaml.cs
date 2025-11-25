using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using BCrypt.Net;

namespace Pim_04.Views
{
    public partial class LoginWindow : Window
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DeskProDb"].ConnectionString;
        private TextBox? passwordTextBox;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnEntrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string usuario = txtUsuario.Text;
                string senha = txtSenha.Password;

                if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha))
                {
                    MessageBox.Show("Por favor, preencha todos os campos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!usuario.All(c => char.IsLetterOrDigit(c) || c == '_'))
                {
                    MessageBox.Show("O nome de usuário deve conter apenas letras, números ou sublinhado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT PasswordHash, Id, IsAdmin FROM Users WHERE Username = @username", connection);
                    command.Parameters.AddWithValue("@username", usuario);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string? storedHash = reader.IsDBNull(0) ? null : reader.GetString(0);
                            int userId = reader.GetInt32(1);
                            bool isAdmin = reader.GetBoolean(2);

                            if (storedHash != null && BCrypt.Net.BCrypt.Verify(senha, storedHash))
                            {
                                MessageBox.Show($"Login efetuado com sucesso!\nUsuário: {usuario}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                                new TelaPrincipal(userId, isAdmin, usuario).Show(); // Passa o usuario como userName
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Usuário ou senha incorretos.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Usuário não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro durante o login: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AbrirCadastro_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new CadastroWindow().Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir a janela de cadastro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtUsuario_KeyDown(object sender, KeyEventArgs e)
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
                btnEntrar_Click(sender, new RoutedEventArgs());
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
                    FontSize = txtSenha.FontSize,
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
                }
            }
        }

        private void ChkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            if (passwordTextBox != null)
            {
                txtSenha.Password = passwordTextBox.Text;
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
    }
}