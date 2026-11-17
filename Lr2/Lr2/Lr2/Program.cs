using System;
using System.Collections.Generic;

namespace SnakeGameLab2
{
    // ============================================================
    // 1. ПАТТЕРН: ФАБРИЧНЫЙ МЕТОД (Factory Method)
    // Создание различных типов еды для змейки
    // ============================================================

    // Продукт
    public interface IFood
    {
        string Name { get; }
        int ScoreValue { get; }
        int LengthEffect { get; } // +1 обычная, +3 бонус, -1 яд
        string GetDescription();
    }

    // Конкретные продукты
    public class RegularFood : IFood
    {
        public string Name => "Обычная еда";
        public int ScoreValue => 10;
        public int LengthEffect => 1;
        public string GetDescription() => $"{Name}: +{ScoreValue} очков, +{LengthEffect} к длине";
    }

    public class BonusFood : IFood
    {
        public string Name => "Бонусная еда";
        public int ScoreValue => 50;
        public int LengthEffect => 3;
        public string GetDescription() => $"{Name}: +{ScoreValue} очков, +{LengthEffect} к длине";
    }

    public class PoisonFood : IFood
    {
        public string Name => "Яд";
        public int ScoreValue => -20;
        public int LengthEffect => -1;
        public string GetDescription() => $"{Name}: {ScoreValue} очков, {LengthEffect} к длине";
    }

    // Создатель (Creator)
    public interface IFoodFactory
    {
        IFood CreateFood();
    }

    // Конкретные создатели
    public class RegularFoodFactory : IFoodFactory
    {
        public IFood CreateFood() => new RegularFood();
    }

    public class BonusFoodFactory : IFoodFactory
    {
        public IFood CreateFood() => new BonusFood();
    }

    public class PoisonFoodFactory : IFoodFactory
    {
        public IFood CreateFood() => new PoisonFood();
    }

    // ============================================================
    // 2. ПАТТЕРН: ПРОТОТИП (Prototype)
    // Клонирование конфигураций уровней
    // ============================================================

    public interface ILevelPrototype
    {
        ILevelPrototype Clone();
    }

    public class GameLevel : ILevelPrototype
    {
        public string LevelName { get; set; }
        public int GridWidth { get; set; }
        public int GridHeight { get; set; }
        public int TargetScore { get; set; }
        public int InitialSnakeLength { get; set; }
        public string Difficulty { get; set; }

        public GameLevel(string name, int width, int height, int target, int length, string difficulty)
        {
            LevelName = name;
            GridWidth = width;
            GridHeight = height;
            TargetScore = target;
            InitialSnakeLength = length;
            Difficulty = difficulty;
        }

        // Реализация клонирования через MemberwiseClone
        public ILevelPrototype Clone()
        {
            return (GameLevel)this.MemberwiseClone();
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"  Уровень: '{LevelName}' | Поле: {GridWidth}x{GridHeight} | " +
                              $"Цель: {TargetScore} очков | Начальная длина: {InitialSnakeLength} | " +
                              $"Сложность: {Difficulty}");
        }
    }

    // ============================================================
    // 3. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №2: Игра «Змейка» ===\n");

            // --- Демонстрация Фабричного метода ---
            Console.WriteLine("1. Демонстрация паттерна 'Фабричный метод':");
            Console.WriteLine("   (Создание различных типов еды через фабрики)\n");

            IFoodFactory regularFactory = new RegularFoodFactory();
            IFoodFactory bonusFactory = new BonusFoodFactory();
            IFoodFactory poisonFactory = new PoisonFoodFactory();

            // Создаём последовательность еды, которую «съест» змейка
            List<IFood> foods = new List<IFood>
            {
                regularFactory.CreateFood(),
                regularFactory.CreateFood(),
                bonusFactory.CreateFood(),
                poisonFactory.CreateFood()
            };

            int totalScore = 0;
            int totalLength = 0;
            foreach (var food in foods)
            {
                Console.WriteLine($"   Съедено: {food.GetDescription()}");
                totalScore += food.ScoreValue;
                totalLength += food.LengthEffect;
            }
            Console.WriteLine($"   -> ИТОГО: {totalScore} очков, изменение длины: {totalLength}\n");

            // --- Демонстрация Прототипа ---
            Console.WriteLine("2. Демонстрация паттерна 'Прототип':");
            Console.WriteLine("   (Клонирование и модификация уровней)\n");

            // Базовый уровень
            GameLevel baseLevel = new GameLevel("Базовый уровень", 20, 20, 100, 3, "Легко");
            Console.WriteLine("   Исходный уровень:");
            baseLevel.DisplayInfo();

            // Клонируем базовый уровень и создаём сложный
            GameLevel hardLevel = (GameLevel)baseLevel.Clone();
            hardLevel.LevelName = "Сложный уровень";
            hardLevel.TargetScore = 500;
            hardLevel.Difficulty = "Сложно";
            hardLevel.GridWidth = 30;
            hardLevel.GridHeight = 30;

            Console.WriteLine("\n   Клонированный и модифицированный уровень:");
            hardLevel.DisplayInfo();

            // Создаём ещё один уровень на основе базового — «Марафон»
            GameLevel marathonLevel = (GameLevel)baseLevel.Clone();
            marathonLevel.LevelName = "Марафон";
            marathonLevel.TargetScore = 1000;
            marathonLevel.Difficulty = "Экстремально";
            marathonLevel.GridWidth = 40;
            marathonLevel.GridHeight = 40;

            Console.WriteLine("\n   Ещё один клон — уровень «Марафон»:");
            marathonLevel.DisplayInfo();

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}