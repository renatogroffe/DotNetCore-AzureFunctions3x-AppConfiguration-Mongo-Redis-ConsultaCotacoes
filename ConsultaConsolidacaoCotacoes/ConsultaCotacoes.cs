using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ConsultaConsolidacaoCotacoes.Documents;
using ConsultaConsolidacaoCotacoes.Data;

namespace ConsultaConsolidacaoCotacoes
{
    public class ConsultaCotacoes
    {
        private CotacoesRepository _repository;

        public ConsultaCotacoes(CotacoesRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("ValorAtual")]
        public IActionResult RunValorAtual(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string codigo = req.Query["codigo"];
            if (String.IsNullOrWhiteSpace(codigo))
            {
                log.LogError(
                    $"ValorAtual HTTP trigger - Codigo de Moeda nao informado");
                return new BadRequestObjectResult(new
                {
                    Sucesso = false,
                    Mensagem = "Código de Moeda não informado"
                });
            }

            log.LogInformation($"ValorAtual HTTP trigger - codigo da Moeda: {codigo}");
            CotacaoMoedaDocument cotacao = null;
            if (!String.IsNullOrWhiteSpace(codigo))
                cotacao = _repository.Get(codigo.ToUpper());

            if (cotacao != null)
            {
                log.LogInformation(
                    $"ValorAtual HTTP trigger - Moeda: {codigo} | Valor atual: {cotacao.Valor} | Ultima atualizacao: {cotacao.DataReferencia}");
                return new OkObjectResult(cotacao);
            }
            else
            {
                log.LogError(
                    $"ValorAtual HTTP trigger - Codigo de Moeda nao encontrado: {codigo}");
                return new NotFoundObjectResult(new
                {
                    Sucesso = false,
                    Mensagem = $"Código de Moeda não encontrado: {codigo}"
                });
            }
        }

        [FunctionName("HistoricoDolar")]
        public IActionResult RunHistorico(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var historico = _repository.ListHistoricoDolar();
            log.LogInformation(
                $"HistoricoDolar HTTP trigger - Qtde. Registros Historico: {historico.Count}");

            return new OkObjectResult(historico);
        }
    }
}