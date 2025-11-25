using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pim_04.Views
{
    public partial class ChatbotWindow : Window
    {
        private readonly TelaPrincipal parentWindow;
        private readonly HttpClient httpClient;
        private readonly string apiKey;

        public ChatbotWindow(TelaPrincipal parent)
        {
            InitializeComponent();
            this.parentWindow = parent;
            // Inicializa o cliente com a chave da API
            apiKey = ConfigurationManager.AppSettings["GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("Chave da API não encontrada no app.config. Verifique o arquivo de configuração.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
            MessageBox.Show($"Chatbot inicializado com chave: {apiKey.Substring(0, 5)}... Tente perguntar algo.", "Depuração", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            string pergunta = txtInput.Text?.Trim();
            if (string.IsNullOrWhiteSpace(pergunta) || (txtInput.Text == "Digite sua pergunta..." && !txtInput.IsKeyboardFocused))
            {
                MessageBox.Show("Digite uma pergunta ou descrição do problema.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Adiciona a mensagem do usuário imediatamente
            AddMessage("Você: " + pergunta, true);
            txtInput.Text = "";

            try
            {
                MessageBox.Show("Enviando requisição à API...", "Depuração", MessageBoxButton.OK, MessageBoxImage.Information);
                var requestBody = new
                {
                    contents = new[] { new { parts = new[] { new { text = pergunta } } } },
                    generationConfig = new { temperature = 0.7, topK = 1, topP = 1, maxOutputTokens = 2048 }
                };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.0-pro:generateContent?key={apiKey}";
                MessageBox.Show($"URL usada: {url}", "Depuração", MessageBoxButton.OK, MessageBoxImage.Information);
                var response = await httpClient.PostAsync(url, content);

                MessageBox.Show($"Status da resposta: {response.StatusCode} - {response.ReasonPhrase}", "Depuração", MessageBoxButton.OK, MessageBoxImage.Information);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Erro na API: {response.StatusCode} - {response.ReasonPhrase}\nDetalhes: {errorContent}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    AddMessage("IA: Erro ao processar sua solicitação. Verifique a chave da API ou o endpoint.", false);
                    return;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Resposta bruta: {jsonResponse}", "Depuração", MessageBoxButton.OK, MessageBoxImage.Information);
                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                string respostaIA = responseObject.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "Sem resposta disponível.";

                // Identificar nível 1 e propor solução
                string respostaFinal = ProcessResponseForLevel1(pergunta, respostaIA);
                AddMessage("IA: " + respostaFinal, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao integrar com a IA: {ex.Message}\nDetalhes: {ex.StackTrace}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                AddMessage("IA: Erro interno. Tente novamente.", false);
            }
        }

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSend_Click(sender, e);
            }
        }

        private void AddMessage(string message, bool isUser)
        {
            TextBlock tb = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = isUser ? FontWeights.Bold : FontWeights.Normal
            };
            ConversationPanel.Children.Add(tb);
            ConversationScrollViewer.ScrollToEnd();
        }

        private string ProcessResponseForLevel1(string pergunta, string respostaIA)
        {
            string[] level1Keywords = { "senha", "login", "configuração", "conexão", "email", "acesso" };
            bool isLevel1 = Array.Exists(level1Keywords, keyword => pergunta.ToLower().Contains(keyword));

            if (isLevel1)
            {
                if (pergunta.ToLower().Contains("senha"))
                    return $"{respostaIA}\nSolução sugerida: Tente redefinir sua senha no portal de autoatendimento ou contate o suporte.";
                if (pergunta.ToLower().Contains("login"))
                    return $"{respostaIA}\nSolução sugerida: Verifique suas credenciais ou limpe o cache do navegador.";
                if (pergunta.ToLower().Contains("configuração"))
                    return $"{respostaIA}\nSolução sugerida: Reinstale o software ou verifique as configurações padrão.";
                if (pergunta.ToLower().Contains("conexão"))
                    return $"{respostaIA}\nSolução sugerida: Verifique sua conexão com a internet ou reinicie o roteador.";
                if (pergunta.ToLower().Contains("email"))
                    return $"{respostaIA}\nSolução sugerida: Confirme o endereço de email ou reenvie a verificação.";
                if (pergunta.ToLower().Contains("acesso"))
                    return $"{respostaIA}\nSolução sugerida: Solicite acesso ao administrador do sistema.";
            }
            return respostaIA;
        }

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.Show();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            httpClient.Dispose();
            base.OnClosed(e);
        }
    }
}