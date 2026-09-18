using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DeskFlow.Domain.Models.Enums;

namespace DeskFlow.Domain.Services;

public record PriorityAssessment(TicketPriority Priority, int Score, IReadOnlyList<string> Reasons);

public class SmartPriorityService
{
    public PriorityAssessment Calculate(string title, string description)
    {
        var text = Normalize($"{title}. {description}");
        var reasons = new List<string>();
        var impact = HighestMatch(text, reasons,
            (5, "Impacto em toda a organização", ["todos os usuarios", "todos os funcionarios", "empresa inteira", "toda a empresa"]),
            (3, "Impacto em um setor inteiro", ["setor inteiro", "todo o setor", "departamento inteiro"]),
            (2, "Impacto em vários usuários", ["varios usuarios", "varios funcionarios"]),
            (1, "Impacto individual", ["meu computador", "um usuario", "um funcionario"]));
        var urgency = HighestMatch(text, reasons,
            (3, "Serviço/operação indisponível", ["fora do ar", "sistema indisponivel", "servico indisponivel", "operacao parada", "nao inicia", "parou de funcionar", "caiu"]),
            (2, "Usuário ou operação bloqueada", ["bloqueado", "bloqueada", "nao consegue trabalhar", "sem acesso", "nao funciona", "travou"]),
            (1, "Solicitação marcada como urgente", ["urgente", "urgencia"]));
        var context = HighestMatch(text, reasons,
            (4, "Ambiente de produção afetado", ["producao", "ambiente produtivo"]),
            (2, "Infraestrutura corporativa afetada", ["servidor", "banco de dados", "database", "sistema corporativo", "software da empresa", "sistema da empresa"]),
            (1, "Software/sistema afetado", ["software", "aplicacao", "sistema"]));

        var securityRisk = SecurityRisk(text);
        var financialRisk = ContainsAny(text, ["pagamento", "pagamentos", "financeiro", "faturamento", "cobranca"]) ? 3 : 0;
        var risk = Math.Max(securityRisk.Score, financialRisk);
        if (securityRisk.Score > 0 && securityRisk.Score >= financialRisk)
            reasons.Add($"{securityRisk.Reason} (+{risk})");
        else if (financialRisk > 0)
            reasons.Add($"Risco financeiro (+{risk})");

        var score = impact + urgency + context + risk;
        if (impact == 5 && urgency >= 2 && score < 10)
        {
            reasons.Add($"Operação de toda a organização bloqueada: mínimo Critical (+{10 - score}).");
            score = 10;
        }
        else if (impact >= 3 && urgency >= 2 && score < 7)
        {
            reasons.Add($"Operação de um setor bloqueada: mínimo High (+{7 - score}).");
            score = 7;
        }
        var priority = score switch
        {
            >= 10 => TicketPriority.Critical,
            >= 7 => TicketPriority.High,
            >= 4 => TicketPriority.Medium,
            _ => TicketPriority.Low
        };
        if (reasons.Count == 0)
            reasons.Add("Nenhum fator de criticidade elevado foi detectado.");
        return new PriorityAssessment(priority, score, reasons);
    }

    private static (int Score, string Reason) SecurityRisk(string text)
    {
        (int Score, string Reason) result = (0, "");
        // Clause-level exclusions avoid treating a training request as an attack,
        // without masking an actual incident in a different clause. This remains a heuristic.
        foreach (var clause in Regex.Split(text, @"[.!?;\r\n]+|\b(?:mas|porem|entretanto)\b"))
        {
            var critical = ContainsAny(clause,
                ["ataque hacker", "ataque cibernetico", "ataque em andamento", "sob ataque", "sofrendo ataque",
                 "ransomware", "invasao", "invadido", "invadida", "vazamento", "dados vazando",
                 "dados expostos", "dados roubados", "exfiltracao", "perda de dados", "dados perdidos", "dados apagados"]);
            var threat = critical || ContainsAny(clause, ["malware", "virus", "phishing", "credenciais comprometidas"]);
            var protectionOutage = ContainsAny(clause, ["firewall", "antivirus", "sistema de seguranca"])
                && ContainsAny(clause, ["caiu", "fora do ar", "desativado", "desativada", "nao funciona", "indisponivel"]);
            if (!threat && !protectionOutage) continue;
            if (Regex.IsMatch(clause, @"\b(nao (?:ha|houve|ocorreu|estamos sofrendo|foi (?:detectado|confirmado))|sem (?:sinais|indicios|evidencias|ocorrencia|risco)|falso positivo|descartad[oa])\b"))
                continue;
            if (ContainsAny(clause, ["treinamento", "simulacao", "simulado", "prevencao", "prevenir", "evitar", "protecao contra", "teste de", "testes de", "duvida sobre", "como proteger"]))
                continue;
            var suspected = ContainsAny(clause, ["suspeita", "suspeito", "possivel", "possibilidade", "risco de", "tentativa", "alerta de"]);
            var score = critical && !suspected ? 10 : 7;
            if (score > result.Score)
                result = (score, score == 10
                    ? "Incidente grave de segurança ou perda de dados relatado: mínimo Critical"
                    : "Suspeita de incidente ou proteção comprometida: mínimo High");
        }
        return result;
    }

    private static int HighestMatch(string text, List<string> reasons, params (int Score, string Reason, string[] Keywords)[] rules)
    {
        foreach (var rule in rules)
        {
            if (!ContainsAny(text, rule.Keywords)) continue;
            reasons.Add($"{rule.Reason} (+{rule.Score})");
            return rule.Score;
        }
        return 0;
    }

    private static bool ContainsAny(string text, string[] keywords) => keywords.Any(keyword =>
        Regex.IsMatch(text, $@"\b{Regex.Escape(keyword)}\b"));

    private static string Normalize(string text)
    {
        var normalized = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var c in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        return Regex.Replace(builder.ToString().Normalize(NormalizationForm.FormC), @"[^\S\r\n]+", " ");
    }
}
