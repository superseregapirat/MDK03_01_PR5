using System;
using System.Collections.Generic;
using System.Threading;

namespace TurnstileBlueGreenMigrationAdvanced
{
    // Класс, имитирующий отдельный независимый контур инфраструктуры
    public class ProductionEnvironment
    {
        public string Name { get; }
        public string Version { get; set; }
        public List<string> DatabaseLogs { get; } = new List<string>();

        public ProductionEnvironment(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public void ProcessAccess(string cardId)
        {
            // Если контур Green — данные обработаны по старой структуре, если Blue — сохранены в новую
            string status = (Name == "Green") ? "обработана." : "сохранена в новую структуру.";
            Console.WriteLine($"[{Name} (Production {Version})] Запись прохода: Карта {cardId} {status}");
            DatabaseLogs.Add(cardId);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("=== СИМУЛЯЦИЯ СЦЕНАРИЯ BLUE-GREEN МИГРАЦИИ (РАЗДЕЛЬНЫЕ КОНТУРЫ) ===");

            // Физически создаем два разных контура
            var greenEnvironment = new ProductionEnvironment("Green", "v1.0");
            var blueEnvironment = new ProductionEnvironment("Blue", "v2.0");

            // Указываем балансировщику (роутеру) на текущий активный контур
            ProductionEnvironment activeRouterTarget = greenEnvironment;

            // 1. Работает старый контур Green
            activeRouterTarget.ProcessAccess("RFID-TAG-100");
            activeRouterTarget.ProcessAccess("RFID-TAG-101");
            activeRouterTarget.ProcessAccess("RFID-TAG-102");
            Console.WriteLine();

            // 2. В это время параллельно Blue обновляется (имитация работы CI/CD конвейера)
            Console.WriteLine("[CI/CD] Запущено фоновое обновление изолированного контура Blue...");
            Console.WriteLine($"[{blueEnvironment.Name} (Stage {blueEnvironment.Version})] Начало миграции схемы БД. Создание индексов...");

            // Трафик всё еще идет на Green во время миграции
            activeRouterTarget.ProcessAccess("RFID-TAG-103");
            activeRouterTarget.ProcessAccess("RFID-TAG-104");

            Console.WriteLine($"[{blueEnvironment.Name} (Stage {blueEnvironment.Version})] Миграция схемы успешно завершена. Новая структура готова.");
            Console.WriteLine("[CI/CD] Верификация тестов в контуре Blue: СТАТУС 200 OK.");
            Console.WriteLine();

            // 3. Мгновенное переключение балансировщика (API Gateway) на Blue
            Console.WriteLine("[Router] Внимание! Переключение основного трафика СКУД на контур Blue...");
            activeRouterTarget = blueEnvironment;
            Console.WriteLine();

            // 4. Теперь трафик обрабатывается новым контуром Blue
            activeRouterTarget.ProcessAccess("RFID-TAG-105");
            activeRouterTarget.ProcessAccess("RFID-TAG-106");
            activeRouterTarget.ProcessAccess("RFID-TAG-107");
            Console.WriteLine();

            // 5. Итоги миграции из разных баз данных
            Console.WriteLine("=== ИТОГ МИГРАЦИИ ===");
            Console.WriteLine($"Логов обработано в старой БД (Green): {greenEnvironment.DatabaseLogs.Count}");
            Console.WriteLine($"Логов обработано в новой БД (Blue): {blueEnvironment.DatabaseLogs.Count}");
            Console.WriteLine("Вывод: Обновление ПО и структуры БД произведено без остановки обслуживания.");

            Console.ReadLine();
        }
    }
}
