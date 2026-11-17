using System;
using System.Collections.Generic;

namespace SnakeGameLab3
{
    // ============================================================
    // 1. ПАТТЕРН: МОСТ (Bridge)
    // Разделяем абстракцию (змейка) и реализацию (отрисовка)
    // ============================================================

    // Реализация (Implementor) — интерфейс для способов отрисовки
    public interface IRenderer
    {
        void RenderSnake(List<string> body);
        void ClearScreen();
    }

    // Конкретная реализация — отрисовка в консоль
    public class ConsoleRenderer : IRenderer
    {
        public void RenderSnake(List<string> body)
        {
            Console.WriteLine("   [CONSOLE] Отрисовка змейки: " + string.Join(" -> ", body));
        }

        public void ClearScreen()
        {
            Console.WriteLine("   [CONSOLE] Очистка экрана.");
        }
    }

    // Конкретная реализация — отрисовка в HTML
    public class HtmlRenderer : IRenderer
    {
        public void RenderSnake(List<string> body)
        {
            Console.WriteLine("   [HTML] <div class='snake'>" + string.Join(", ", body) + "</div>");
        }

        public void ClearScreen()
        {
            Console.WriteLine("   [HTML] document.body.innerHTML = '';");
        }
    }

    // Абстракция (Abstraction) — базовый класс змейки
    public abstract class Snake
    {
        protected IRenderer _renderer;

        protected Snake(IRenderer renderer)
        {
            _renderer = renderer;
        }

        public abstract void Move();
        public abstract void Draw();
    }

    // Уточненная абстракция — классическая змейка
    public class ClassicSnake : Snake
    {
        private List<string> _body = new List<string> { "(0,0)", "(1,0)", "(2,0)" };

        public ClassicSnake(IRenderer renderer) : base(renderer) { }

        public override void Move()
        {
            Console.WriteLine("   [ClassicSnake] Движение по сетке на 1 клетку.");
            _body.RemoveAt(0);
            _body.Add("(3,0)");
        }

        public override void Draw()
        {
            _renderer.RenderSnake(_body);
        }
    }

    // Уточненная абстракция — скоростная змейка
    public class SpeedSnake : Snake
    {
        private List<string> _body = new List<string> { "(0,0)", "(1,0)", "(2,0)", "(3,0)" };

        public SpeedSnake(IRenderer renderer) : base(renderer) { }

        public override void Move()
        {
            Console.WriteLine("   [SpeedSnake] Быстрое движение на 2 клетки!");
            _body.RemoveAt(0);
            _body.RemoveAt(0);
            _body.Add("(4,0)");
            _body.Add("(5,0)");
        }

        public override void Draw()
        {
            _renderer.RenderSnake(_body);
        }
    }

    // ============================================================
    // 2. ПАТТЕРН: АДАПТЕР (Adapter)
    // Интеграция сторонней звуковой библиотеки
    // ============================================================

    // Целевой интерфейс (Target), который ожидает игра
    public interface IGameAudio
    {
        void PlayEatSound();
        void PlayGameOverSound();
    }

    // Адаптируемый класс (Adaptee) — сторонняя библиотека с несовместимым интерфейсом
    public class ThirdPartySoundPlayer
    {
        public void PlayWav(string path)
        {
            Console.WriteLine($"   [ThirdParty] Воспроизведение файла: {path}");
        }

        public void StopWav()
        {
            Console.WriteLine("   [ThirdParty] Остановка воспроизведения.");
        }
    }

    // Адаптер (Adapter) — преобразует интерфейс библиотеки к интерфейсу игры
    public class SoundPlayerAdapter : IGameAudio
    {
        private readonly ThirdPartySoundPlayer _adaptee;

        public SoundPlayerAdapter(ThirdPartySoundPlayer player)
        {
            _adaptee = player;
        }

        public void PlayEatSound()
        {
            // Преобразуем вызов: игра просит PlayEatSound(), адаптер вызывает PlayWav с нужным путём
            _adaptee.PlayWav("sounds/eat.wav");
        }

        public void PlayGameOverSound()
        {
            _adaptee.PlayWav("sounds/gameover.wav");
        }
    }

    // ============================================================
    // 3. КЛИЕНТСКИЙ КОД — демонстрация работы
    // ============================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Лабораторная работа №3: Игра «Змейка» ===\n");

            // --- Демонстрация паттерна МОСТ ---
            Console.WriteLine("1. Демонстрация паттерна 'Мост':");
            Console.WriteLine("   (Абстракция змейки отделена от способа отрисовки)\n");

            // Классическая змейка с консольной отрисовкой
            Snake classicConsole = new ClassicSnake(new ConsoleRenderer());
            Console.WriteLine("   ClassicSnake + ConsoleRenderer:");
            classicConsole.Move();
            classicConsole.Draw();

            Console.WriteLine();

            // Та же классическая змейка, но с HTML-отрисовкой
            Snake classicHtml = new ClassicSnake(new HtmlRenderer());
            Console.WriteLine("   ClassicSnake + HtmlRenderer:");
            classicHtml.Move();
            classicHtml.Draw();

            Console.WriteLine();

            // Скоростная змейка с консольной отрисовкой
            Snake speedConsole = new SpeedSnake(new ConsoleRenderer());
            Console.WriteLine("   SpeedSnake + ConsoleRenderer:");
            speedConsole.Move();
            speedConsole.Draw();

            Console.WriteLine();

            // --- Демонстрация паттерна АДАПТЕР ---
            Console.WriteLine("2. Демонстрация паттерна 'Адаптер':");
            Console.WriteLine("   (Интеграция сторонней звуковой библиотеки)\n");

            // Сторонняя библиотека с несовместимым интерфейсом
            ThirdPartySoundPlayer library = new ThirdPartySoundPlayer();
            Console.WriteLine("   Прямой вызов методов библиотеки:");
            library.PlayWav("custom.wav");

            Console.WriteLine();

            // Используем адаптер, чтобы работать с ней через интерфейс игры
            IGameAudio gameAudio = new SoundPlayerAdapter(library);
            Console.WriteLine("   Вызов через адаптер (целевой интерфейс IGameAudio):");
            gameAudio.PlayEatSound();
            gameAudio.PlayGameOverSound();

            Console.WriteLine("\n=== Программа успешно завершила работу ===");
        }
    }
}