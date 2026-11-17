using System;
using System.Collections.Generic;

namespace SnakeGameLab5
{
    // ============================================================
    // 1. ПАТТЕРН: ПРИСПОСОБЛЕНЕЦ (Flyweight)
    // Оптимизация памяти за счёт разделения внутреннего состояния
    // ============================================================

    // Интерфейс приспособленца
    public interface ISegmentType
    {
        string Name { get; }
        string Color { get; }
        string Symbol { get; }
        void DisplayDetails();
    }

    // Конкретный приспособленец (внутреннее состояние)
    public class SegmentType : ISegmentType
    {
        public string Name { get; }
        public string Color { get; }
        public string Symbol { get; }

        public SegmentType(string name, string color, string symbol)
        {
            Name = name;
            Color = color;
            Symbol = symbol;
        }

        public void DisplayDetails()
        {
            Console.WriteLine($"  [{Symbol}] {Name} (Цвет: {Color})");
        }
    }

    // Фабрика приспособленцев (кэширует объекты)
    public class SegmentTypeFactory
    {
        private readonly Dictionary<string, ISegmentType> _types =
            new Dictionary<string, ISegmentType>();

        public ISegmentType GetSegmentType(string name)
        {
            if (!_types.ContainsKey(name))
            {
                Console.WriteLine($"  [Factory] Создание нового типа сегмента: '{name}'");
                string color, symbol;
                switch (name)
                {
                    case "Голова":
                        color = "Зелёный"; symbol = "O";
                        break;
                    case "Тело":
                        color = "Светло-зелёный"; symbol = "o";
                        break;
                    case "Хвост":
                        color = "Тёмно-зелёный"; symbol = "·";
                        break;
                    default:
                        color = "Серый"; symbol = "?";
                        break;
                }
                _types[name] = new SegmentType(name, color, symbol);
            }
            else
            {
                Console.WriteLine($"  [Factory] Использование существующего типа: '{name}'");
            }
            return _types[name];
        }
    }

    // Контекст (внешнее состояние)
    public class SnakeSegment
    {
        public int X { get; }
        public int Y { get; }
        public ISegmentType Type { get; }

        public SnakeSegment(ISegmentType type, int x, int y)
        {
            Type = type;
            X = x;
            Y = y;
        }

        public void Display()
        {
            Console.WriteLine($"  Сегмент ({X},{Y}): {Type.Symbol} [{Type.Name}]");
        }
    }

    // ============================================================
    // 2. ПАТТЕРН: ФАСАД (Facade)
    // Упрощение интерфейса сложной подсистемы
    // ============================================================

    // Подсистема 1: Игровой движок
    public class GameEngine
    {
        private bool _isRunning = false;

        public void Start()
        {
            _isRunning = true;
            Console.WriteLine("  [Engine] Игра запущена.");
        }

        public void Move(string direction)
        {
            if (_isRunning)
            {
                Console.WriteLine($"  [Engine] Змейка движется в направлении: {direction}");
            }
            else
            {
                Console.WriteLine("  [Engine] Игра не запущена!");
            }
        }

        public bool IsRunning() => _isRunning;
    }

    // Подсистема 2: Трекер очков
    public class ScoreTracker
    {
        private int _score = 0;

        public void AddPoints(int points)
        {
            _score += points;
            Console.WriteLine($"  [Score] +{points} очков. Всего: {_score}");
        }

        public int GetScore() => _score;
    }

    // Подсистема 3: Менеджер уровней
    public class LevelManager
    {
        private int _currentLevel = 1;

        public int GetCurrentLevel() => _currentLevel;

        public void NextLevel()
        {
            _currentLevel++;
            Console.WriteLine($"  [Level] Переход на уровень {_currentLevel}!");
        }
    }

    // Фасад
    public class GameFacade
    {
        private readonly GameEngine _engine;
        private readonly ScoreTracker _score;
        private readonly LevelManager _levels;
        private readonly SegmentTypeFactory _segmentFactory;

        public GameFacade()
        {
            _engine = new GameEngine();
            _score = new ScoreTracker();
            _levels = new LevelManager();
            _segmentFactory = new SegmentTypeFactory();
        }

        public void StartGame()
        {
            Console.WriteLine("\n--- Запуск игры через Фасад ---");
            _engine.Start();
            _score.AddPoints(0);
            Console.WriteLine($"  [Facade] Текущий уровень: {_levels.GetCurrentLevel()}");
        }

        public void MoveSnake(string direction)
        {
            Console.WriteLine($"\n--- Движение змейки ({direction}) ---");
            _engine.Move(direction);
            _score.AddPoints(10);

            // Каждые 50 очков — переход на новый уровень
            if (_score.GetScore() > 0 && _score.GetScore() % 50 == 0)
            {
                _levels.NextLevel();
            }
        }

        public void CreateLongSnake(int length)
        {
            Console.WriteLine($"\n--- Создание змейки длиной {length} сегментов ---");
            Console.WriteLine("  (Демонстрация паттерна Приспособленец)");

            // Получаем общие типы сегментов из фабрики
            ISegmentType headType = _segmentFactory.GetSegmentType("Голова");
            ISegmentType bodyType = _segmentFactory.GetSegmentType("Тело");
            ISegmentType tailType = _segmentFactory.GetSegmentType("Хвост");

            // Создаём много сегментов, но типы переиспользуются
            List<SnakeSegment> snake = new List<SnakeSegment>();

            // Голова
            snake.Add(new SnakeSegment(headType, 5, 5));
            // Тело
            for (int i = 1; i < length - 1; i++)
            {
                snake.Add(new SnakeSegment(bodyType, 5 - i, 5));
            }
            // Хвост
            if (length > 1)
            {
                snake.Add(new SnakeSegment(tailType, 5 - length + 1, 5));
            }

            Console.WriteLine($"  [Facade] Создано {snake.Count} сегментов змейки.");
            Console.WriteLine("  Первые 3 сегмента:");
            for (int i = 0; i < Math.Min(3, snake.Count); i++)
            {
                snake[i].Display();
            }
        }

        public string GetStatus()
        {
            return $"Уровень: {_levels.GetCurrentLevel()} | Очки: {_score.GetScore()} | " +
                   $"Движок активен: {_engine.IsRunning()}";
        }
    }

    // ============================================================
    // 3. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №5: Игра «Змейка» ===\n");

            // Создаём фасад (клиенту не нужно знать о подсистемах)
            GameFacade facade = new GameFacade();

            // Запуск игры
            facade.StartGame();

            // Движение змейки
            facade.MoveSnake("Вправо");
            facade.MoveSnake("Вправо");
            facade.MoveSnake("Вниз");
            facade.MoveSnake("Вниз");
            facade.MoveSnake("Влево");

            // Демонстрация Приспособленца — создаём длинную змейку
            facade.CreateLongSnake(10);

            // Создание ещё одной змейки — типы сегментов переиспользуются
            facade.CreateLongSnake(15);

            // Получение статуса игры
            Console.WriteLine("\n--- Статус игры ---");
            Console.WriteLine($"  {facade.GetStatus()}");

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}