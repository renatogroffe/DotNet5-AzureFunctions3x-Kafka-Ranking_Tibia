using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FunctionAppExtracaoTibia.Documents;
using FunctionAppExtracaoTibia.Data;
using Confluent.Kafka;

namespace FunctionAppExtracaoTibia
{
    public static class ExtracaoTibiaTimerTrigger
    {
        [Function("ExtracaoTibiaTimerTrigger")]
        public static void Run([TimerTrigger("*/20 * * * * *")] FunctionContext context)
        {
            var logger = context.GetLogger("ExtracaoTibiaTimerTrigger");
            var httpClient = new HttpClient();

            string endpointTibia = Environment.GetEnvironmentVariable("EndpointTibia");
            var ranking = httpClient.GetFromJsonAsync<RankingTibia>(
                endpointTibia).Result;
            logger.LogInformation($"Obtidos dados do endpoint: {endpointTibia}");

            // Mantém apenas parte dos scores
            int numUsuariosExtracao = new Random().Next(3,7);
            ranking.highscores.data =
                ranking.highscores.data.Take(numUsuariosExtracao).ToList();

            TibiaRepository.Save(ranking);
            logger.LogInformation("Dados persistidos...");

            // Envia notificação para o Slack
            httpClient.PostAsJsonAsync(
                Environment.GetEnvironmentVariable("EndpointSlack"),
                new { local = Environment.MachineName, numUsuariosExtracao })
                    .Wait();
            logger.LogInformation(
                $"Notificação enviada para o Slack | No. de scores registrados: {numUsuariosExtracao}");

            string topic = Environment.GetEnvironmentVariable("TopicKafka");
            var configKafka = new ProducerConfig
            {
                BootstrapServers = Environment.GetEnvironmentVariable("BrokerKafka"),
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = Environment.GetEnvironmentVariable("UserKafka"),
                SaslPassword = Environment.GetEnvironmentVariable("PasswordKafka")
            };

            string conteudoRanking = JsonSerializer.Serialize(ranking);
            using (var producer = new ProducerBuilder<Null, string>(configKafka).Build())
            {
                var result = producer.ProduceAsync(
                    topic,
                    new Message<Null, string>
                    { Value = conteudoRanking }).Result;

                logger.LogInformation(
                    $"Apache Kafka - Envio para o tópico {topic} concluído | " +
                    $"Status: { result.Status.ToString()} | Conteúdo: {conteudoRanking}");
            }

            logger.LogInformation($"{nameof(ExtracaoTibiaTimerTrigger)} executada em: {DateTime.Now}");
        }
    }
}