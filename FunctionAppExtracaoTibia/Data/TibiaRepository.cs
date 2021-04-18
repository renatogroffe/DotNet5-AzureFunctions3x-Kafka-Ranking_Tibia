using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using FunctionAppExtracaoTibia.Documents;

namespace FunctionAppExtracaoTibia.Data
{
    public static class TibiaRepository
    {
        private static readonly string _BaseTibia;
        private static readonly string _Collection;

        static TibiaRepository()
        {
            _BaseTibia = Environment.GetEnvironmentVariable("BaseTibia");
            _Collection = Environment.GetEnvironmentVariable("CollectionRanking");

            using var client = GetDocumentClient();

            client.CreateDatabaseIfNotExistsAsync(
                new Database { Id = _BaseTibia }).Wait();

            var collectionInfo = new DocumentCollection();
            collectionInfo.Id = _Collection;

            collectionInfo.IndexingPolicy =
                new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

            client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(_BaseTibia),
                collectionInfo,
                new RequestOptions { OfferThroughput = 400 }).Wait();
        }
        private static DocumentClient GetDocumentClient()
        {
            return new DocumentClient(
                new Uri(Environment.GetEnvironmentVariable("DBTibiaEndpointUri")),
                Environment.GetEnvironmentVariable("DBTibiaEndpointPrimaryKey"));
        }

        public static void Save(RankingTibia ranking)
        {
            var horario = DateTime.Now;
            
            ranking.id = horario.ToString("yyyyMMdd-HHmmss");
            ranking.time = horario.ToString("yyyy-MM-dd HH:mm:ss");

            using var client = GetDocumentClient();
            client.CreateDocumentAsync(
               UriFactory.CreateDocumentCollectionUri(
                   _BaseTibia, _Collection), ranking).Wait();
        }
    }
}