using System;
using System.Collections.Generic;

namespace SnakeGameLab1
{
    // ============================================================
    // 1. ПАТТЕРН ОДИНОЧКА (Singleton)
    // Потокобезопасная реализация через Lazy<T>
    // ============================================================
    public sealed class GameSettings
    {
        private static readonly Lazy<GameSettings> _lazy =
            new Lazy<GameSettings>(() => new GameSettings());

        public static GameSettings Instance => _lazy.Value;

        public int GridWidth { get; set; } = 20;
        public int GridHeight { get; set; } = 20;
        public int TickRateMs { get; set; } = 200;
        public int HighScore { get; set; } = 0;

        // Приватный конструктор запрещает создание экземпляров извне
        private GameSettings() { }
    }

    // ============================================================
    // 2. ПРОДУКТ — класс Змейки
    // ============================================================
    public class Snake
    {
        public List<(int X, int Y)> Body { get; set; } = new List<(int, int)>();
        public string Direction { get; set; } = "Right";
        public string Color { get; set; } = "Green";

        public void Display()
        {
            Console.WriteLine($"   Змейка: Цвет={Color}, Направление={Direction}, Длина={Body.Count}");
            Console.WriteLine("   Сегменты тела: " + string.Join(" -> ", Body));
        }
    }

    // ============================================================
    // 3. ПАТТЕРН СТРОИТЕЛЬ (Builder)
    // Fluent interface — каждый метод возвращает this
    // ============================================================
    public class SnakeBuilder
    {
        private readonly Snake _snake = new Snake();

        public SnakeBuilder SetLength(int length)
        {
            _snake.Body.Clear();
            for (int i = 0; i < length; i++)
            {
                // Начальная горизонтальная линия змейки
                _snake.Body.Add((i, 0));
            }
            return this;
        }

        public SnakeBuilder SetDirection(string direction)
        {
            _snake.Direction = direction;
            return this;
        }

        public SnakeBuilder SetColor(string color)
        {
            _snake.Color = color;
            return this;
        }

        public Snake Build()
        {
            return _snake;
        }
    }

    // ============================================================
    // 4. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №1: Игра «Змейка» ===\n");

            // --- Демонстрация паттерна Одиночка ---
            Console.WriteLine("1. Проверка паттерна 'Одиночка' (GameSettings):");
            var settings1 = GameSettings.Instance;
            var settings2 = GameSettings.Instance;

            Console.WriteLine($"   settings1 == settings2 (ReferenceEquals): {ReferenceEquals(settings1, settings2)}");
            Console.WriteLine($"   Размер поля по умолчанию: {settings1.GridWidth}x{settings1.GridHeight}");

            settings1.HighScore = 1500;
            Console.WriteLine($"   Рекорд установлен через settings1: {settings2.HighScore} (виден глобально)\n");

            // --- Демонстрация паттерна Строитель ---
            Console.WriteLine("2. Проверка паттерна 'Строитель' (SnakeBuilder):");
            var builder = new SnakeBuilder();

            // Создание стандартной змейки
            var defaultSnake = builder
                .SetLength(3)
                .SetDirection("Right")
                .SetColor("Зеленый")
                .Build();

            Console.WriteLine("   Стандартная змейка:");
            defaultSnake.Display();

            // Создание змейки-босса с другими параметрами тем же билдером
            var bossSnake = builder
                .SetLength(10)
                .SetDirection("Up")
                .SetColor("Красный")
                .Build();

            Console.WriteLine("\n   Босс-змейка (построена тем же билдером):");
            bossSnake.Display();

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}