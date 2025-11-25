using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Configuration;

namespace Pim_04.Views
{
    public partial class RelatoriosWindow : Window
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DeskProDb"].ConnectionString;
        private readonly TelaPrincipal parentWindow;
        private readonly HttpClient httpClient;

        public RelatoriosWindow(TelaPrincipal parent)
        {
            InitializeComponent();
            this.parentWindow = parent;
            LoadRelatorios();
            // Inicializa o cliente HTTP
            string apiKey = ConfigurationManager.AppSettings["GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("Chave da API não encontrada no app.config. Verifique o arquivo de configuração.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
            MessageBox.Show($"Cliente HTTP inicializado com chave: {apiKey.Substring(0, 5)}...", "Depuração", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadRelatorios()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Contar chamados abertos
                    var commandAbertos = new SqlCommand("SELECT COUNT(*) FROM Chamados WHERE Status = 'Aberto'", connection);
                    txtChamadosAbertos.Text = commandAbertos.ExecuteScalar().ToString();

                    // Contar chamados em andamento
                    var commandEmAndamento = new SqlCommand("SELECT COUNT(*) FROM Chamados WHERE Status = 'Em Andamento'", connection);
                    txtChamadosEmAndamento.Text = commandEmAndamento.ExecuteScalar().ToString();

                    // Contar chamados fechados
                    var commandFechados = new SqlCommand("SELECT COUNT(*) FROM Chamados WHERE Status = 'Fechado'", connection);
                    txtChamadosFechados.Text = commandFechados.ExecuteScalar().ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar relatórios: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       

            

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.Show();
            this.Close();
        }
    }
}