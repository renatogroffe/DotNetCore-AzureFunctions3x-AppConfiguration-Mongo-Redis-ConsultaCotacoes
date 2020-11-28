using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using StackExchange.Redis;
using ConsultaConsolidacaoCotacoes.Documents;

namespace ConsultaConsolidacaoCotacoes.Data
{
    public class CotacoesRepository
    {
        private readonly ConnectionMultiplexer _conexaoRedis;
        private readonly string _prefixoCotacaoRedis;
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<CotacaoMoedaMongoDocument> _mongoCollection;

        public CotacoesRepository(IConfiguration configuration)
        {
            _conexaoRedis = ConnectionMultiplexer
                .Connect(configuration["RedisConnectionString"]);

            _prefixoCotacaoRedis = configuration["PrefixoCotacaoRedis"];

            _mongoClient = new MongoClient(configuration["MongoConnection"]);
            _mongoDatabase = _mongoClient.GetDatabase(
                configuration["MongoDatabase"]);
            _mongoCollection = _mongoDatabase
                .GetCollection<CotacaoMoedaMongoDocument>(
                    configuration["MongoCollection"]);
        }

        public CotacaoMoedaDocument Get(string codigo)
        {
            string strDadosAcao =
                _conexaoRedis.GetDatabase().StringGet(
                    $"{_prefixoCotacaoRedis}-{codigo}");
            if (!String.IsNullOrWhiteSpace(strDadosAcao))
                return JsonSerializer.Deserialize<CotacaoMoedaDocument>(
                    strDadosAcao,
                    new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });
            else
                return null;
        }        

        public List<CotacaoMoedaMongoDocument> ListHistoricoDolar()
        {
            return _mongoCollection.Find(all => true).ToEnumerable()
                .OrderByDescending(d => d.DataReferencia).ToList();
        }
    }
}