using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Semaphore.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(".NET 9 usando semaphore!");

            // Exemplo 1: Controle de concorrência com SemaphoreSlim
            var exemplo1 = new Exemplo1();
            exemplo1.Executar().GetAwaiter().GetResult();

            // Exemplo 2: WorkerService com SemaphoreSlim
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<Exemplo2>();
                })
                .Build();

            host.Start();

            Console.WriteLine("\n\nFim de execução\n\n");
        }
    }

    internal class Exemplo1
    {
        private const int initialCount = 1; // Disponibilidade de tarefa do semáforo
        private const int maxCount = 2; // Maximo de tarefas permitida na fila do semáforo

        private readonly SemaphoreSlim semaphore = new(initialCount, maxCount);

        public async Task Executar()
        {
            Console.WriteLine("Executando exemplo 1");

            await semaphore.WaitAsync(); // Aguardar uma disponibilidade do semáforo

            try
            {
                Console.WriteLine($"Início: {DateTime.Now}");

                // Simula uma tarefa em processamento ou integrgação com API externa
                await Task.Delay(1000);

                Console.WriteLine($"Fim: {DateTime.Now}\n\n");
            }
            finally
            {
                semaphore.Release(); // Libera o semáforo 
            }
        }
    }

    internal class Exemplo2 : BackgroundService
    {
        private const int initialCount = 1; // Disponibilidade de tarefa do semáforo

        private readonly SemaphoreSlim semaphore = new(initialCount);

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            Console.WriteLine("Executando exemplo 2 - WorkerService");

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessarTarefaComControleDeConcorrencia(stoppingToken);

                await Task.Delay(
                    TimeSpan.FromSeconds(5), stoppingToken); // Delay entre execuções
            }
        }

        private async Task ProcessarTarefaComControleDeConcorrencia(
            CancellationToken cancellationToken)
        {

            if (!await semaphore.WaitAsync(0, cancellationToken)) // tenta pegar a vaga, mas não espera
            {
                Console.WriteLine("Já tem uma tarefa em execução, ignorando execução concorrente...");
                return;
            }

            try
            {
                Console.WriteLine($"Início: {DateTime.Now}");

                // Simula uma tarefa em processamento ou integrgação com API externa
                await Task.Delay(1000);

                Console.WriteLine($"Fim: {DateTime.Now}\n\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
