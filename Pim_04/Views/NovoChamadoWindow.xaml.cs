using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using Pim_04.Views;

namespace Pim_04.Views
{
    public partial class NovoChamadoWindow : Window
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DeskProDb"].ConnectionString;
        private readonly TelaPrincipal parentWindow;
        private readonly int userId;
        private readonly string userName; // Novo campo para o nome do usuário

        public NovoChamadoWindow(TelaPrincipal parent, int userId, string userName)
        {
            InitializeComponent();
            this.parentWindow = parent;
            this.userId = userId;
            this.userName = userName; // Recebe o nome do usuário do TelaPrincipal
            cbSituacao.SelectedIndex = 0;

            // Debug: Verifique o userId e userName (remova após testar)
            if (userId <= 0)
            {
                MessageBox.Show($"UserId inválido: {userId}. Verifique o login.", "Erro de Debug", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show($"UserName inválido ou ausente: {userName}.", "Erro de Debug", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSalvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string titulo = txtTitulo.Text?.Trim() ?? "";
                string descricao = txtDescricao.Text?.Trim() ?? "";
                string status = (cbSituacao.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Aberto";

                if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descricao))
                {
                    MessageBox.Show("Preencha Título e Descrição.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (userId <= 0)
                {
                    MessageBox.Show("UserId inválido. Não é possível salvar.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(userName))
                {
                    MessageBox.Show("Nome de usuário inválido. Não é possível salvar.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "INSERT INTO Chamados (UsuarioId, Titulo, Descricao, Status, DataCriacao, NomeUsuario) " +
                        "VALUES (@usuarioId, @titulo, @descricao, @status, @dataCriacao, @nomeUsuario)", connection);
                    command.Parameters.AddWithValue("@usuarioId", userId);
                    command.Parameters.AddWithValue("@titulo", titulo);
                    command.Parameters.AddWithValue("@descricao", descricao);
                    command.Parameters.AddWithValue("@status", status);
                    command.Parameters.AddWithValue("@dataCriacao", DateTime.Now); // Adiciona a data atual
                    command.Parameters.AddWithValue("@nomeUsuario", userName); // Adiciona o nome do usuário
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        MessageBox.Show("Nenhum registro inserido. Verifique o schema ou constraints.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                MessageBox.Show("Chamado salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                parentWindow.RefreshChamados(); // Usando RefreshChamados em vez de LoadChamados
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar chamado: {ex.Message}\nStackTrace: {ex.StackTrace}", "Erro Detalhado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}