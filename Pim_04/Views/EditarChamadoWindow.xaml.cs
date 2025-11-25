using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;

namespace Pim_04.Views
{
    public partial class EditarChamadoWindow : Window
    {
        private readonly TelaPrincipal parentWindow;
        private readonly Chamado chamado;
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DeskProDb"].ConnectionString;

        public EditarChamadoWindow(TelaPrincipal parent, Chamado chamado)
        {
            InitializeComponent();
            this.parentWindow = parent;
            this.chamado = chamado;

            // Preenche os campos com os dados atuais do chamado
            txtTitulo.Text = chamado.Titulo;
            txtDescricao.Text = chamado.Descricao;
            txtStatus.Text = chamado.Status;
            txtNomeUsuario.Text = chamado.NomeUsuario;
        }

        private void BtnAlterar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "UPDATE Chamados SET Titulo = @titulo, Descricao = @descricao, Status = @status, NomeUsuario = @nomeUsuario WHERE Id = @id",
                        connection);
                    command.Parameters.AddWithValue("@titulo", txtTitulo.Text);
                    command.Parameters.AddWithValue("@descricao", txtDescricao.Text);
                    command.Parameters.AddWithValue("@status", txtStatus.Text);
                    command.Parameters.AddWithValue("@nomeUsuario", string.IsNullOrWhiteSpace(txtNomeUsuario.Text) ? (object)DBNull.Value : txtNomeUsuario.Text);
                    command.Parameters.AddWithValue("@id", chamado.Id);
                    command.ExecuteNonQuery();
                    MessageBox.Show("Chamado atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    parentWindow.RefreshChamados(parentWindow.dgChamados.ItemsSource == parentWindow.MeusChamados ? "MeusChamados" : "TodosChamados");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao atualizar chamado: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TxtTitulo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnAlterar_Click(sender, e);
            }
        }

        private void TxtDescricao_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnAlterar_Click(sender, e);
            }
        }

        private void TxtStatus_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnAlterar_Click(sender, e);
            }
        }

        private void TxtNomeUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnAlterar_Click(sender, e);
            }
        }
    }
}