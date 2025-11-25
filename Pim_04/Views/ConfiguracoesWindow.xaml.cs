using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;

namespace Pim_04.Views
{
    public partial class ConfiguracoesWindow : Window
    {
        private readonly int userId;
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DeskProDb"].ConnectionString;
        private readonly bool isAdmin;
        private ObservableCollection<User> usuarios = new ObservableCollection<User>();
        private TelaPrincipal parentWindow;

        public ConfiguracoesWindow(TelaPrincipal parent, int userId, bool isAdmin)
        {
            InitializeComponent();
            this.parentWindow = parent;
            this.userId = userId;
            this.isAdmin = isAdmin;
            dgUsuarios.ItemsSource = usuarios;
            LoadUserInfo();
            ToggleAdminFeatures();
        }

        private void LoadUserInfo()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT Username, Email FROM Users WHERE Id = @userId", connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtNome.Text = reader.GetString(0);
                            txtEmail.Text = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar informações do usuário: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleAdminFeatures()
        {
            if (isAdmin)
            {
                txtUsuariosTitle.Visibility = Visibility.Visible;
                dgUsuarios.Visibility = Visibility.Visible;
                btnSalvar.Visibility = Visibility.Visible;
                LoadUsuarios();
            }
            else
            {
                txtUsuariosTitle.Visibility = Visibility.Collapsed;
                dgUsuarios.Visibility = Visibility.Collapsed;
                btnSalvar.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadUsuarios()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT Id, Username, Email, IsAdmin FROM Users", connection);
                    using (var reader = command.ExecuteReader())
                    {
                        usuarios.Clear();
                        while (reader.Read())
                        {
                            usuarios.Add(new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                IsAdmin = reader.GetBoolean(3)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar usuários: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgUsuarios_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            // Permitir edição apenas para admin (controlado por IsReadOnly = False)
        }

        private void DgUsuarios_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (isAdmin && e.EditAction == DataGridEditAction.Commit)
            {
                var user = e.Row.Item as User;
                if (user != null)
                {
                    try
                    {
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            var command = new SqlCommand("UPDATE Users SET IsAdmin = @isAdmin WHERE Id = @id", connection);
                            command.Parameters.AddWithValue("@isAdmin", user.IsAdmin);
                            command.Parameters.AddWithValue("@id", user.Id);
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao atualizar status de administrador: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("UPDATE Users SET Username = @username, Email = @email WHERE Id = @userId", connection);
                    command.Parameters.AddWithValue("@username", txtNome.Text.Trim());
                    command.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                    command.Parameters.AddWithValue("@userId", userId);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Informações atualizadas com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsuarios(); // Recarrega a lista de usuários para refletir mudanças
                    }
                    else
                    {
                        MessageBox.Show("Nenhuma alteração foi salva.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar alterações: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
    }
}