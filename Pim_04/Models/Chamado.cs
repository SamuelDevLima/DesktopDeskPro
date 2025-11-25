using System.ComponentModel;

namespace Pim_04.Views
{
    public class Chamado : INotifyPropertyChanged
    {
        private string? _status;

        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Descricao { get; set; }
        public string? Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        public DateTime DataCriacao { get; set; }
        public int UsuarioId { get; set; }
        private string? _nomeUsuario;
        public string? NomeUsuario
        {
            get => _nomeUsuario;
            set
            {
                _nomeUsuario = value;
                OnPropertyChanged(nameof(NomeUsuario));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}