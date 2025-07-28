namespace FlashFit.Models
{
    public class UserData
    {
        public UserInfo User { get; set; }
        public TrainingInfo Training { get; set; }
        public FoodInfo Food { get; set; }
        public PlanoInfo Plano { get; set; }
        public CalculosInfo Calculos { get; set; }
    }

    public class UserInfo
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public int Idade { get; set; }
        public double Peso { get; set; }
        public double PesoDesejado { get; set; }
        public double Altura { get; set; }
        public string Sexo { get; set; }
        public string Objetivo { get; set; }
        public string ObjetivoSecundario { get; set; }
        public string Atividade { get; set; }
        public string Alergias { get; set; }
        public string CondicaoMedica { get; set; }
    }

    public class TrainingInfo
    {
        public string Local { get; set; }
        public string Experiencia { get; set; }
        public int Frequencia { get; set; }
        public int Duracao { get; set; }
        public string EquipamentosEmCasa { get; set; }
        public string RestricoesTreino { get; set; }
        public string LimitacoesFisicas { get; set; }
    }

    public class FoodInfo
    {
        public string RestricoesAlimentares { get; set; }
        public int FrequenciaRefeicoes { get; set; }
        public string HorarioRefeicoes { get; set; }
        public string AlimentosFavoritos { get; set; }
        public string AlimentosNaoGostar { get; set; }
        public string SuplementosUtilizados { get; set; }
    }

    public class PlanoInfo
    {
        public string Id { get; set; }
        public string MetodoPagamento { get; set; }
    }

    public class CalculosInfo
    {
        public double IMC { get; set; }
        public double TMB { get; set; }
        public double TDEE { get; set; }
        public double CaloriasParaPerda { get; set; }
        public double CaloriasParaGanho { get; set; }
    }
}
