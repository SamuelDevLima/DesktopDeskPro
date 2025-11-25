using Microsoft.Data.SqlClient;
using Pim_04.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Pim_04.Views
{
    public partial class TelaPrincipal : Window
    {
        private readonly int userId;
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DeskProDb"].ConnectionString;
        private readonly bool isAdmin;
        private readonly string userName; // Novo campo para o nome do usuário
        private ObservableCollection<Chamado> meusChamados = new ObservableCollection<Chamado>();
        private ObservableCollection<Chamado> todosChamados = new ObservableCollection<Chamado>();

        public TelaPrincipal(int userId, bool isAdmin, string userName)
        {
            InitializeComponent();
            this.userId = userId;
            this.isAdmin = isAdmin;
            this.userName = userName;
            dgChamados.ItemsSource = meusChamados; // Inicialmente carrega Meus Chamados
            LoadChamados("MeusChamados"); // Carrega "Meus Chamados" como padrão
            ToggleAdminFeatures();
            UpdateDataGridColumns(); // Ajusta as colunas do DataGrid com base no estado
            dgChamados.MouseDoubleClick += DgChamados_MouseDoubleClick; // Adiciona duplo clique para abrir edição
        }

        public int UserId { get { return userId; } }
        public string UserName { get { return userName; } }
        public ObservableCollection<Chamado> MeusChamados { get { return meusChamados; } } // Propriedade pública
        public ObservableCollection<Chamado> TodosChamados { get { return todosChamados; } } // Propriedade pública

        public void RefreshChamados(string viewType = "MeusChamados")
        {
            LoadChamados(viewType);
            UpdateDataGridColumns(); // Atualiza as colunas após recarregar os dados
        }

        public void LoadChamados(string viewType = "MeusChamados")
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand();
                    command.Connection = connection;

                    if (viewType == "MeusChamados")
                    {
                        command.CommandText = "SELECT Id, Titulo, Descricao, Status, DataCriacao, UsuarioId, NomeUsuario FROM Chamados WHERE UsuarioId = @userId";
                        command.Parameters.AddWithValue("@userId", userId);
                        dgChamados.ItemsSource = MeusChamados;
                        MeusChamados.Clear();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MeusChamados.Add(new Chamado
                                {
                                    Id = reader.GetInt32(0),
                                    Titulo = reader.GetString(1),
                                    Descricao = reader.GetString(2),
                                    Status = reader.GetString(3),
                                    DataCriacao = reader.GetDateTime(4),
                                    UsuarioId = reader.GetInt32(5),
                                    NomeUsuario = reader.IsDBNull(6) ? "Não informado" : reader.GetString(6)
                                });
                            }
                        }
                    }
                    else if (viewType == "TodosChamados" && isAdmin)
                    {
                        command.CommandText = "SELECT Id, Titulo, Descricao, Status, DataCriacao, UsuarioId, NomeUsuario FROM Chamados";
                        dgChamados.ItemsSource = TodosChamados;
                        TodosChamados.Clear();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TodosChamados.Add(new Chamado
                                {
                                    Id = reader.GetInt32(0),
                                    Titulo = reader.GetString(1),
                                    Descricao = reader.GetString(2),
                                    Status = reader.GetString(3),
                                    DataCriacao = reader.GetDateTime(4),
                                    UsuarioId = reader.GetInt32(5),
                                    NomeUsuario = reader.IsDBNull(6) ? "Não informado" : reader.GetString(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar chamados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleAdminFeatures()
        {
            if (isAdmin)
            {
                dgChamados.IsReadOnly = false;
                dgChamados.BeginningEdit += DgChamados_BeginningEdit;
                dgChamados.CellEditEnding += DgChamados_CellEditEnding;
                btnTodosChamados.Visibility = Visibility.Visible;
            }
            else
            {
                dgChamados.IsReadOnly = true;
                btnTodosChamados.Visibility = Visibility.Collapsed;
            }
            UpdateDataGridColumns(); // Ajusta as colunas ao alternar entre admin e comum
        }

        private void UpdateDataGridColumns()
        {
            dgChamados.Columns.Clear();
            dgChamados.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("Id"), Width = 50, IsReadOnly = true });
            dgChamados.Columns.Add(new DataGridTextColumn { Header = "Título", Binding = new Binding("Titulo"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgChamados.Columns.Add(new DataGridTextColumn { Header = "Descrição", Binding = new Binding("Descricao"), Width = new DataGridLength(2, DataGridLengthUnitType.Star) });
            dgChamados.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new Binding("Status"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgChamados.Columns.Add(new DataGridTextColumn { Header = "Data Criação", Binding = new Binding("DataCriacao") { StringFormat = "{0:dd/MM/yyyy HH:mm}" }, Width = new DataGridLength(1, DataGridLengthUnitType.Star), IsReadOnly = true });
            dgChamados.Columns.Add(new DataGridTextColumn { Header = "Usuário ID", Binding = new Binding("UsuarioId"), Width = new DataGridLength(1, DataGridLengthUnitType.Star), IsReadOnly = true });

            // Adiciona a coluna NomeUsuario apenas para administradores na visualização "Todos os Chamados"
            if (isAdmin && dgChamados.ItemsSource == TodosChamados)
            {
                dgChamados.Columns.Add(new DataGridTextColumn { Header = "Nome Usuário", Binding = new Binding("NomeUsuario"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            }
        }

        private void DgChamados_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!isAdmin || dgChamados.ItemsSource != TodosChamados) e.Cancel = true; // Impede edição para usuários comuns ou fora de "Todos os Chamados"
        }

        private void DgChamados_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (isAdmin && e.EditAction == DataGridEditAction.Commit && dgChamados.ItemsSource == TodosChamados)
            {
                var chamado = e.Row.Item as Chamado;
                if (chamado != null)
                {
                    try
                    {
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            var command = new SqlCommand("UPDATE Chamados SET Status = @status WHERE Id = @id", connection);
                            command.Parameters.AddWithValue("@status", chamado.Status);
                            command.Parameters.AddWithValue("@id", chamado.Id);
                            command.ExecuteNonQuery();
                            RefreshChamados("TodosChamados"); // Recarrega a view atual
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao atualizar status: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DgChamados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (isAdmin && dgChamados.SelectedItem != null)
            {
                var chamado = dgChamados.SelectedItem as Chamado;
                if (chamado != null)
                {
                    var editarChamadoWindow = new EditarChamadoWindow(this, chamado);
                    editarChamadoWindow.ShowDialog();
                    RefreshChamados(dgChamados.ItemsSource == MeusChamados ? "MeusChamados" : "TodosChamados");
                }
            }
        }

        private void BtnNovoChamado_Click(object sender, RoutedEventArgs e)
        {
            NovoChamadoWindow novoChamadoWindow = new NovoChamadoWindow(this, userId, UserName);
            novoChamadoWindow.ShowDialog();
            RefreshChamados("MeusChamados"); // Recarrega "Meus Chamados" após criar um novo
        }

        private void BtnMeusChamados_Click(object sender, RoutedEventArgs e)
        {
            RefreshChamados("MeusChamados"); // Recarrega "Meus Chamados"
        }

        private void BtnTodosChamados_Click(object sender, RoutedEventArgs e)
        {
            if (isAdmin)
            {
                RefreshChamados("TodosChamados"); // Carrega "Todos os Chamados" apenas para admin
            }
        }

        private void BtnRelatorios_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Tentando abrir Relatórios...", "Depuração", MessageBoxButton.OK, MessageBoxImage.Information); // Depuração
                new RelatoriosWindow(this).Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir Relatórios: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnConfiguracoes_Click(object sender, RoutedEventArgs e)
        {
            ConfiguracoesWindow configuracoesWindow = new ConfiguracoesWindow(this, userId, isAdmin);
            configuracoesWindow.ShowDialog();
            RefreshChamados(dgChamados.ItemsSource == MeusChamados ? "MeusChamados" : "TodosChamados"); // Recarrega a view atual
        }

        private void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnBuscar_Click(sender, new RoutedEventArgs());
            }
        }

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string searchTerm = txtBuscar.Text?.Trim() ?? "";
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT Id, Titulo, Descricao, Status, DataCriacao, UsuarioId, NomeUsuario FROM Chamados WHERE UsuarioId = @userId AND (Titulo LIKE @term OR Descricao LIKE @term OR Status LIKE @term)", connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@term", $"%{searchTerm}%");
                    MeusChamados.Clear();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MeusChamados.Add(new Chamado
                            {
                                Id = reader.GetInt32(0),
                                Titulo = reader.GetString(1),
                                Descricao = reader.GetString(2),
                                Status = reader.GetString(3),
                                DataCriacao = reader.GetDateTime(4),
                                UsuarioId = reader.GetInt32(5),
                                NomeUsuario = reader.IsDBNull(6) ? "Não informado" : reader.GetString(6)
                            });
                        }
                    }
                }
                RefreshChamados("MeusChamados"); // Recarrega para atualizar a visualização
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar chamados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       

        private void BtnSair_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}