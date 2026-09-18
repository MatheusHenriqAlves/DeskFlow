using Xunit;
using DeskFlow.Domain.Models.Enums;
using DeskFlow.Domain.Services;

namespace DeskFlow.Tests;

public class SmartPriorityServiceTests
{
    private readonly SmartPriorityService _service = new();

    [Fact]
    public void ReportedHackerAttack_IsCriticalWithConsistentScoreAndReason()
    {
        var result = _service.Calculate("Sistema de segurança caiu", "Estamos sofrendo ataque hacker");
        Assert.Equal(TicketPriority.Critical, result.Priority);
        Assert.True(result.Score >= 10);
        Assert.Contains(result.Reasons, reason => reason.Contains("Incidente grave"));
    }

    [Theory]
    [InlineData("Incidente", "Estamos sob ataque cibernético.", TicketPriority.Critical)]
    [InlineData("Incidente", "Ransomware criptografou os arquivos.", TicketPriority.Critical)]
    [InlineData("Incidente", "Vazamento de dados confirmado.", TicketPriority.Critical)]
    [InlineData("Incidente", "Dados apagados após invasão.", TicketPriority.Critical)]
    [InlineData("Incidente", "Suspeita de ataque hacker.", TicketPriority.High)]
    [InlineData("Incidente", "Possível vazamento de dados.", TicketPriority.High)]
    [InlineData("Incidente", "Alerta de malware.", TicketPriority.High)]
    [InlineData("Proteção", "Firewall desativado.", TicketPriority.High)]
    [InlineData("Treinamento de segurança", "Treinamento para prevenir ataque hacker.", TicketPriority.Low)]
    [InlineData("Prevenção", "Como proteger contra ransomware?", TicketPriority.Low)]
    [InlineData("Verificação", "Não houve ataque hacker.", TicketPriority.Low)]
    [InlineData("Verificação", "Sem evidências de vazamento de dados.", TicketPriority.Low)]
    [InlineData("Verificação", "Suspeita de invasão descartada.", TicketPriority.Low)]
    [InlineData("Impressora não funciona", "Preciso imprimir um documento.", TicketPriority.Low)]
    [InlineData("O software da empresa travou", "O aplicativo não funciona.", TicketPriority.Medium)]
    [InlineData("Operação parada", "Toda a empresa está sem acesso ao sistema.", TicketPriority.Critical)]
    [InlineData("Setor bloqueado", "Todo o setor não consegue trabalhar no sistema.", TicketPriority.High)]
    [InlineData("Incidente", "Não houve vazamento, mas estamos sofrendo ataque hacker.", TicketPriority.Critical)]
    public void TriageDistinguishesIncidentsFromRoutineRequests(string title, string description, TicketPriority expected)
    {
        var result = _service.Calculate(title, description);
        Assert.Equal(expected, result.Priority);
        Assert.Equal(expected, result.Score switch
        {
            >= 10 => TicketPriority.Critical,
            >= 7 => TicketPriority.High,
            >= 4 => TicketPriority.Medium,
            _ => TicketPriority.Low
        });
    }

    [Fact]
    public void SimpleIndividualIssue_IsLow()
    {
        var result = _service.Calculate("Mouse com defeito", "O mouse do meu computador não funciona.");
        Assert.Equal(TicketPriority.Low, result.Priority);
    }

    [Fact]
    public void DepartmentBlocked_IsMediumOrHigher()
    {
        var result = _service.Calculate("Setor bloqueado", "Todo o setor não consegue trabalhar no sistema.");
        Assert.True(result.Priority >= TicketPriority.Medium);
    }

    [Fact]
    public void ProductionPaymentsOutage_IsCritical()
    {
        var result = _service.Calculate(
            "Sistema de pagamentos de produção fora do ar",
            "Todos os usuários estão bloqueados e o financeiro está indisponível.");
        Assert.Equal(TicketPriority.Critical, result.Priority);
        Assert.True(result.Score >= 10);
    }
}
