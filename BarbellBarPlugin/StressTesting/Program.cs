using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using BarbellBarPlugin.Kompas;
using BarbellBarPlugin.Model;
using Microsoft.VisualBasic.Devices;

namespace StressTesting
{
    internal static class Program
    {
        /// <summary>
        /// Точка входа в приложение нагрузочного тестирования.
        /// Запускает бесконечный цикл построения модели и логирует время построения,
        /// загрузку ОЗУ системы, загрузку CPU процесса и Working Set процесса.
        /// </summary>
        /// <param name="args">Аргументы командной строки (не используются).</param>
        private static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var parameters = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1250,
                separatorDiameter: 40,
                sleeveLength: 350);

            var wrapper = new Wrapper();
            var builder = new BarBuilder(wrapper);

            var stopWatch = new Stopwatch();
            var computerInfo = new ComputerInfo();
            var currentProcess = Process.GetCurrentProcess();

            const double bytesToGigabytes = 1.0 / 1073741824.0;
            const double bytesToMegabytes = 1.0 / (1024.0 * 1024.0);

            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

            using var streamWriter = new StreamWriter(logPath, append: false, encoding: Encoding.UTF8)
            {
                AutoFlush = true
            };

            streamWriter.WriteLine("Iteration\tBuildTimeMs\tUsedRamGb\tCpuProcessPercent\tProcessWorkingSetMb");

            int count = 0;

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                Environment.Exit(0);
            };

            while (count<2000)
            {
                count++;

                currentProcess.Refresh();
                TimeSpan cpuStart = currentProcess.TotalProcessorTime;

                stopWatch.Restart();
                builder.Build(parameters, closeDocumentAfterBuild: true);
                stopWatch.Stop();

                currentProcess.Refresh();
                TimeSpan cpuEnd = currentProcess.TotalProcessorTime;

                long elapsedMs = stopWatch.ElapsedMilliseconds;
                if (elapsedMs <= 0)
                    elapsedMs = 1;

                TimeSpan cpuDelta = cpuEnd - cpuStart;

                double cpuPercent =
                    cpuDelta.TotalMilliseconds / (elapsedMs * Environment.ProcessorCount) * 100.0;

                double usedRamGb =
                    (computerInfo.TotalPhysicalMemory - computerInfo.AvailablePhysicalMemory) * bytesToGigabytes;

                double workingSetMb =
                    currentProcess.WorkingSet64 * bytesToMegabytes;

                streamWriter.WriteLine(
                    $"{count}\t{elapsedMs}\t{usedRamGb:F6}\t{cpuPercent:F2}\t{workingSetMb:F2}");
            }
        }
    }
}
