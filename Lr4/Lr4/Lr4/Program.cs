using System;
using System.Collections.Generic;

namespace SnakeGameLab4
{
    // ============================================================
    // 1. ПАТТЕРН: ДЕКОРАТОР (Decorator)
    // Динамическое расширение свойств сегментов змейки
    // ============================================================

    // Компонент (Component) — интерфейс сегмента змейки
    public interface ISnakeSegment
    {
        string Name { get; }
        int ScoreValue { get; }
        string GetDescription();
    }

    // Конкретный компонент (ConcreteComponent) — базовый сегмент
    public class BaseSegment : ISnakeSegment
    {
        private readonly string _name;
        public int ScoreValue { get; }

        public BaseSegment(string name, int scoreValue)
        {
            _name = name;
            ScoreValue = scoreValue;
        }

        public string Name => _name;

        public virtual string GetDescription() => $"{_name} (очки: {ScoreValue})";
    }

    // Базовый декоратор (Decorator)
    public abstract class SegmentDecorator : ISnakeSegment
    {
        protected readonly ISnakeSegment _segment;

        protected SegmentDecorator(ISnakeSegment segment)
        {
            _segment = segment;
        }

        public string Name => _segment.Name;
        public virtual int ScoreValue => _segment.ScoreValue;

        public abstract string GetDescription();
    }

    // Конкретный декоратор — сегмент со щитом (защищает от столкновения)
    public class ShieldDecorator : SegmentDecorator
    {
        public ShieldDecorator(ISnakeSegment segment) : base(segment) { }

        public override int ScoreValue => _segment.ScoreValue + 10;

        public override string GetDescription() =>
            $"{_segment.GetDescription()} + [ЩИТ] (защита от столкновения)";
    }

    // Конкретный декоратор — сегмент с ускорением
    public class SpeedDecorator : SegmentDecorator
    {
        public SpeedDecorator(ISnakeSegment segment) : base(segment) { }

        public override int ScoreValue => _segment.ScoreValue + 5;

        public override string GetDescription() =>
            $"{_segment.GetDescription()} + [СКОРОСТЬ] (ускорение движения)";
    }

    // Конкретный декоратор — сегмент с ядом (наносит урон при столкновении)
    public class PoisonDecorator : SegmentDecorator
    {
        public PoisonDecorator(ISnakeSegment segment) : base(segment) { }

        public override int ScoreValue => _segment.ScoreValue - 15;

        public override string GetDescription() =>
            $"{_segment.GetDescription()} + [ЯД] (урон при столкновении)";
    }

    // ============================================================
    // 2. ПАТТЕРН: КОМПОНОВЩИК (Composite)
    // Иерархия игровых объектов на поле
    // ============================================================

    // Компонент (Component) — интерфейс игрового объекта
    public interface IGameObject
    {
        string Name { get; }
        void Display(int indent);
    }

    // Лист (Leaf) — объект еды
    public class FoodObject : IGameObject
    {
        private readonly int _points;
        public string Name { get; }

        public FoodObject(string name, int points)
        {
            Name = name;
            _points = points;
        }

        public void Display(int indent)
        {
            Console.WriteLine($"{new string(' ', indent)}- [Еда] {Name} | +{_points} очков");
        }
    }

    // Лист (Leaf) — объект препятствия
    public class ObstacleObject : IGameObject
    {
        private readonly int _damage;
        public string Name { get; }

        public ObstacleObject(string name, int damage)
        {
            Name = name;
            _damage = damage;
        }

        public void Display(int indent)
        {
            Console.WriteLine($"{new string(' ', indent)}- [Препятствие] {Name} | урон: {_damage}");
        }
    }

    // Лист (Leaf) — объект бонуса
    public class BonusObject : IGameObject
    {
        private readonly string _effect;
        public string Name { get; }

        public BonusObject(string name, string effect)
        {
            Name = name;
            _effect = effect;
        }

        public void Display(int indent)
        {
            Console.WriteLine($"{new string(' ', indent)}- [Бонус] {Name} | эффект: {_effect}");
        }
    }

    // Композиция (Composite) — уровень (группа объектов)
    public class Level : IGameObject
    {
        private readonly string _name;
        private readonly List<IGameObject> _children = new List<IGameObject>();

        public string Name => _name;

        public Level(string name)
        {
            _name = name;
        }

        public void Add(IGameObject obj) => _children.Add(obj);
        public void Remove(IGameObject obj) => _children.Remove(obj);

        public void Display(int indent)
        {
            Console.WriteLine($"{new string(' ', indent)}[Уровень] {_name} (объектов: {_children.Count})");
            foreach (var child in _children)
            {
                child.Display(indent + 2);
            }
        }
    }

    // ============================================================
    // 3. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №4: Игра «Змейка» ===\n");

            // --- Демонстрация паттерна ДЕКОРАТОР ---
            Console.WriteLine("1. Демонстрация паттерна 'Декоратор':");
            Console.WriteLine("   (Динамическое расширение свойств сегментов змейки)\n");

            // Базовый сегмент
            ISnakeSegment baseSegment = new BaseSegment("Обычный сегмент", 10);
            Console.WriteLine($"   Базовый: {baseSegment.GetDescription()}");

            // Оборачиваем в декоратор — добавляем щит
            ISnakeSegment shieldSegment = new ShieldDecorator(baseSegment);
            Console.WriteLine($"   Со щитом: {shieldSegment.GetDescription()}");

            // Оборачиваем в несколько декораторов — щит + скорость + яд
            ISnakeSegment bossSegment = new PoisonDecorator(
                new SpeedDecorator(
                    new ShieldDecorator(new BaseSegment("Босс-сегмент", 50))));
            Console.WriteLine($"   Босс-сегмент: {bossSegment.GetDescription()}");
            Console.WriteLine($"   Очки босс-сегмента: {bossSegment.ScoreValue}\n");

            // --- Демонстрация паттерна КОМПОНОВЩИК ---
            Console.WriteLine("2. Демонстрация паттерна 'Компоновщик':");
            Console.WriteLine("   (Иерархия игровых объектов на поле)\n");

            // Создаём отдельные объекты (листы)
            FoodObject apple = new FoodObject("Яблоко", 10);
            FoodObject berry = new FoodObject("Ягода", 15);
            ObstacleObject rock = new ObstacleObject("Камень", 20);
            BonusObject speedBonus = new BonusObject("Ускорение", "x2 скорость на 5 сек");

            // Создаём уровень "Лесная поляна"
            Level forestLevel = new Level("Лесная поляна");
            forestLevel.Add(apple);
            forestLevel.Add(berry);
            forestLevel.Add(rock);

            // Создаём отдельные объекты для другого уровня
            FoodObject coin = new FoodObject("Монета", 25);
            ObstacleObject wall = new ObstacleObject("Стена", 30);
            BonusObject shieldBonus = new BonusObject("Щит", "Защита на 10 сек");

            // Создаём уровень "Городские джунгли"
            Level cityLevel = new Level("Городские джунгли");
            cityLevel.Add(coin);
            cityLevel.Add(wall);
            cityLevel.Add(shieldBonus);

            // Создаём главный уровень — композиция уровней и отдельных объектов
            Level mainLevel = new Level("ГЛАВНЫЙ УРОВЕНЬ ИГРЫ");
            mainLevel.Add(forestLevel);
            mainLevel.Add(cityLevel);
            mainLevel.Add(new BonusObject("Секретный бонус", "Бессмертие на 3 сек"));

            // Отображаем всю иерархию
            mainLevel.Display(0);

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}