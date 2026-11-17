using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeLabs
{
    // Общие модели игры. Код намеренно оставлен простым и учебным — по стилю ЛР №3.
    public enum Direction { Up, Down, Left, Right }
    public enum GameStatus { Menu, Playing, Paused, GameOver }
    public struct Point { public int X, Y; public Point(int x, int y) { X = x; Y = y; } public override string ToString() { return "(" + X + "," + Y + ")"; } }

    public class SnakeGame
    {
        public List<Point> Body { get; private set; }
        public Point Food { get; set; }
        public Direction CurrentDirection { get; set; }
        public int Score { get; set; }
        public GameStatus Status { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public SnakeGame() { Width = 12; Height = 8; Body = new List<Point> { new Point(4, 3), new Point(3, 3), new Point(2, 3) }; Food = new Point(8, 3); CurrentDirection = Direction.Right; Status = GameStatus.Playing; }
        public void Turn(Direction direction) { if (!((CurrentDirection == Direction.Up && direction == Direction.Down) || (CurrentDirection == Direction.Down && direction == Direction.Up) || (CurrentDirection == Direction.Left && direction == Direction.Right) || (CurrentDirection == Direction.Right && direction == Direction.Left))) CurrentDirection = direction; }
        public void MoveOneCell() { Point h = Body[0]; switch (CurrentDirection) { case Direction.Up: h.Y--; break; case Direction.Down: h.Y++; break; case Direction.Left: h.X--; break; default: h.X++; break; } Body.Insert(0, h); if (h.X == Food.X && h.Y == Food.Y) { Score += 10; Food = new Point(8, 5); } else Body.RemoveAt(Body.Count - 1); }
        public void Restore(List<Point> body, Point food, Direction direction, int score, GameStatus status) { Body = new List<Point>(body); Food = food; CurrentDirection = direction; Score = score; Status = status; }
    }

    // ЛР 6. Заместитель (Proxy): контролирует доступ к настройкам игры.
    public class GameSettings { public int TickMilliseconds = 180; public bool SoundEnabled = true; }
    public interface ISettingsStore { GameSettings Load(); void Save(GameSettings settings); }
    public class SettingsStore : ISettingsStore { private GameSettings value = new GameSettings(); public GameSettings Load() { return value; } public void Save(GameSettings settings) { value = settings; } }
    public class SettingsProxy : ISettingsStore
    {
        private readonly ISettingsStore realStore; private readonly bool canWrite;
        public SettingsProxy(ISettingsStore store, bool writeAccess) { realStore = store; canWrite = writeAccess; }
        public GameSettings Load() { Console.WriteLine("   Proxy: проверен доступ на чтение."); return realStore.Load(); }
        public void Save(GameSettings settings) { if (!canWrite) throw new InvalidOperationException("Запись настроек запрещена."); if (settings.TickMilliseconds < 50 || settings.TickMilliseconds > 1000) throw new ArgumentException("Интервал должен быть от 50 до 1000 мс."); realStore.Save(settings); Console.WriteLine("   Proxy: настройки сохранены."); }
    }

    // ЛР 7. Команда (Command) и Итератор (Iterator).
    public interface IGameCommand { void Execute(SnakeGame game); }
    public class TurnCommand : IGameCommand { private Direction direction; public TurnCommand(Direction d) { direction = d; } public void Execute(SnakeGame game) { game.Turn(direction); Console.WriteLine("   Команда: направление изменено на " + direction); } }
    public class MoveCommand : IGameCommand { public void Execute(SnakeGame game) { game.MoveOneCell(); Console.WriteLine("   Команда: выполнен шаг змейки."); } }
    public class CommandQueue { private Queue<IGameCommand> queue = new Queue<IGameCommand>(); public void Add(IGameCommand command) { queue.Enqueue(command); } public void ExecuteAll(SnakeGame game) { while (queue.Count > 0) queue.Dequeue().Execute(game); } }
    public class SnakeIterator { private readonly IList<Point> body; private int index; public SnakeIterator(IList<Point> snakeBody) { body = snakeBody; index = 0; } public bool HasNext { get { return index < body.Count; } } public Point Next() { if (!HasNext) throw new InvalidOperationException("Элементы закончились."); return body[index++]; } }

    // ЛР 8. Цепочка обязанностей: последовательная проверка столкновений.
    public abstract class CollisionHandler { protected CollisionHandler next; public CollisionHandler SetNext(CollisionHandler handler) { next = handler; return handler; } public bool Handle(SnakeGame game, Point point) { bool? result = Check(game, point); if (result.HasValue) return result.Value; return next != null && next.Handle(game, point); } protected abstract bool? Check(SnakeGame game, Point point); }
    public class WallCollisionHandler : CollisionHandler { protected override bool? Check(SnakeGame game, Point p) { if (p.X <= 0 || p.X >= game.Width - 1 || p.Y <= 0 || p.Y >= game.Height - 1) { Console.WriteLine("   Обработчик стен: обнаружено столкновение."); return true; } return null; } }
    public class BodyCollisionHandler : CollisionHandler { protected override bool? Check(SnakeGame game, Point p) { if (game.Body.Take(game.Body.Count - 1).Any(x => x.X == p.X && x.Y == p.Y)) { Console.WriteLine("   Обработчик тела: обнаружено столкновение."); return true; } Console.WriteLine("   Проверки завершены: столкновения нет."); return false; } }

    // ЛР 9. Посредник (Mediator) и Снимок (Memento).
    public interface IGameComponent { void OnScoreChanged(int score); }
    public class ScoreLabel : IGameComponent { public void OnScoreChanged(int score) { Console.WriteLine("   Компонент табло получил новый счёт: " + score); } }
    public class SoundNotifier : IGameComponent { public void OnScoreChanged(int score) { Console.WriteLine("   Компонент звука: событие изменения счёта " + score); } }
    public class GameMediator { private List<IGameComponent> components = new List<IGameComponent>(); public void Register(IGameComponent component) { components.Add(component); } public void NotifyScoreChanged(int score) { foreach (IGameComponent c in components) c.OnScoreChanged(score); } }
    public class GameMemento { public List<Point> Body; public Point Food; public Direction Direction; public int Score; public GameStatus Status; }
    public class GameSnapshot { public static GameMemento Save(SnakeGame game) { return new GameMemento { Body = new List<Point>(game.Body), Food = game.Food, Direction = game.CurrentDirection, Score = game.Score, Status = game.Status }; } public static void Restore(SnakeGame game, GameMemento m) { game.Restore(m.Body, m.Food, m.Direction, m.Score, m.Status); } }

    // ЛР 10. Наблюдатель (Observer) и Состояние (State).
    public interface IGameObserver { void Update(string message); }
    public class ConsoleObserver : IGameObserver { private string name; public ConsoleObserver(string n) { name = n; } public void Update(string message) { Console.WriteLine("   " + name + " получил уведомление: " + message); } }
    public class GameEventSource { private List<IGameObserver> observers = new List<IGameObserver>(); public void Subscribe(IGameObserver o) { observers.Add(o); } public void Notify(string message) { foreach (IGameObserver o in observers) o.Update(message); } }
    public interface IGameState { void Handle(SnakeGame game); string Name { get; } }
    public class PlayingState : IGameState { public string Name { get { return "Игра идёт"; } } public void Handle(SnakeGame game) { game.MoveOneCell(); Console.WriteLine("   PlayingState: змейка сделала ход."); } }
    public class PausedState : IGameState { public string Name { get { return "Пауза"; } } public void Handle(SnakeGame game) { Console.WriteLine("   PausedState: ход не выполняется."); } }
    public class GameOverState : IGameState { public string Name { get { return "Игра окончена"; } } public void Handle(SnakeGame game) { Console.WriteLine("   GameOverState: игра завершена."); } }
    public class GameStateContext { private IGameState state; public void SetState(IGameState s) { state = s; } public void Request(SnakeGame game) { state.Handle(game); } public string CurrentState { get { return state.Name; } } }

    // ЛР 11. Стратегия (Strategy) и Шаблонный метод (Template Method).
    public interface IMovementStrategy { Point Next(Point head, Direction direction, int width, int height); }
    public class BoundedMovement : IMovementStrategy { public Point Next(Point p, Direction d, int w, int h) { switch (d) { case Direction.Up: p.Y--; break; case Direction.Down: p.Y++; break; case Direction.Left: p.X--; break; default: p.X++; break; } return p; } }
    public class WrapMovement : IMovementStrategy { public Point Next(Point p, Direction d, int w, int h) { p = new BoundedMovement().Next(p, d, w, h); if (p.X <= 0) p.X = w - 2; if (p.X >= w - 1) p.X = 1; if (p.Y <= 0) p.Y = h - 2; if (p.Y >= h - 1) p.Y = 1; return p; } }
    public abstract class GameLoopTemplate { public void RunOneFrame() { ReadInput(); Update(); Render(); } protected abstract void ReadInput(); protected abstract void Update(); protected abstract void Render(); }
    public class DemoGameLoop : GameLoopTemplate { protected override void ReadInput() { Console.WriteLine("   Шаг 1: чтение ввода."); } protected override void Update() { Console.WriteLine("   Шаг 2: обновление состояния."); } protected override void Render() { Console.WriteLine("   Шаг 3: отрисовка поля."); } }

    // ЛР 12. Архитектурный паттерн MVC.
    public class SnakeModel { public SnakeGame Game { get; private set; } public SnakeModel(SnakeGame game) { Game = game; } public void Move(Direction d) { Game.Turn(d); Game.MoveOneCell(); } }
    public class SnakeView { public void Render(SnakeModel model) { Console.WriteLine("   [View] Счёт: " + model.Game.Score + "; голова: " + model.Game.Body[0]); } }
    public class SnakeController { private SnakeModel model; private SnakeView view; public SnakeController(SnakeModel m, SnakeView v) { model = m; view = v; } public void HandleInput(Direction d) { model.Move(d); view.Render(model); } }

    // ЛР 13. CQRS: команды изменяют счёт, запросы только читают его.
    public class AddScoreCommand { public int Points; public AddScoreCommand(int points) { Points = points; } }
    public class ScoreCommandHandler { private int score; public void Handle(AddScoreCommand command) { if (command.Points < 0) throw new ArgumentException("Количество очков не может быть отрицательным."); score += command.Points; Console.WriteLine("   Команда обработана. Новый счёт: " + score); } public int CurrentScore { get { return score; } } }
    public class ScoreQueryService { private ScoreCommandHandler source; public ScoreQueryService(ScoreCommandHandler handler) { source = handler; } public int GetCurrentScore() { return source.CurrentScore; } }

    // ЛР 14. Repository: отделяем работу с рекордами от игровой логики.
    public class PlayerRecord { public string PlayerName; public int BestScore; }
    public interface IRecordRepository { void Save(PlayerRecord record); PlayerRecord Find(string playerName); }
    public class InMemoryRecordRepository : IRecordRepository { private Dictionary<string, PlayerRecord> records = new Dictionary<string, PlayerRecord>(); public void Save(PlayerRecord r) { if (!records.ContainsKey(r.PlayerName) || records[r.PlayerName].BestScore < r.BestScore) records[r.PlayerName] = r; } public PlayerRecord Find(string name) { return records.ContainsKey(name) ? records[name] : null; } }

    // ЛР 15. Producer-Consumer: отдельный поток обрабатывает фоновые задачи.
    public class BackgroundTaskQueue : IDisposable { private readonly System.Collections.Concurrent.BlockingCollection<Action> tasks = new System.Collections.Concurrent.BlockingCollection<Action>(); private readonly Task worker; private bool stopped; public BackgroundTaskQueue() { worker = Task.Factory.StartNew(() => { foreach (Action action in tasks.GetConsumingEnumerable()) action(); }, TaskCreationOptions.LongRunning); } public void Produce(Action action) { tasks.Add(action); } public void Stop() { if (stopped) return; tasks.CompleteAdding(); worker.Wait(); stopped = true; } public void Dispose() { Stop(); tasks.Dispose(); } }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Лабораторные работы № 6–15. Вариант 10 — Игра «Змейка». C#\n");
            Demo("ЛР 6 — Proxy", () => { ISettingsStore real = new SettingsStore(); ISettingsStore proxy = new SettingsProxy(real, true); GameSettings s = proxy.Load(); s.TickMilliseconds = 150; proxy.Save(s); Console.WriteLine("   Интервал игры: " + proxy.Load().TickMilliseconds + " мс"); });
            Demo("ЛР 7 — Command + Iterator", () => { SnakeGame g = new SnakeGame(); CommandQueue q = new CommandQueue(); q.Add(new TurnCommand(Direction.Up)); q.Add(new MoveCommand()); q.ExecuteAll(g); SnakeIterator it = new SnakeIterator(g.Body); while (it.HasNext) Console.WriteLine("   Сегмент: " + it.Next()); });
            Demo("ЛР 8 — Chain of Responsibility", () => { SnakeGame g = new SnakeGame(); CollisionHandler wall = new WallCollisionHandler(); wall.SetNext(new BodyCollisionHandler()); Console.WriteLine("   Столкновение: " + wall.Handle(g, new Point(1, 1))); });
            Demo("ЛР 9 — Mediator + Memento", () => { SnakeGame g = new SnakeGame(); GameMemento saved = GameSnapshot.Save(g); GameMediator mediator = new GameMediator(); mediator.Register(new ScoreLabel()); mediator.Register(new SoundNotifier()); mediator.NotifyScoreChanged(20); g.Score = 99; GameSnapshot.Restore(g, saved); Console.WriteLine("   Состояние восстановлено, счёт: " + g.Score); });
            Demo("ЛР 10 — Observer + State", () => { GameEventSource events = new GameEventSource(); events.Subscribe(new ConsoleObserver("Табло")); events.Subscribe(new ConsoleObserver("Журнал")); events.Notify("Змейка съела яблоко"); GameStateContext state = new GameStateContext(); SnakeGame g = new SnakeGame(); state.SetState(new PlayingState()); state.Request(g); state.SetState(new PausedState()); state.Request(g); Console.WriteLine("   Текущее состояние: " + state.CurrentState); });
            Demo("ЛР 11 — Strategy + Template Method", () => { IMovementStrategy strategy = new WrapMovement(); Point next = strategy.Next(new Point(10, 3), Direction.Right, 12, 8); Console.WriteLine("   Стратегия перехода через границу: " + next); new DemoGameLoop().RunOneFrame(); });
            Demo("ЛР 12 — MVC", () => { SnakeGame g = new SnakeGame(); SnakeModel model = new SnakeModel(g); SnakeView view = new SnakeView(); SnakeController controller = new SnakeController(model, view); controller.HandleInput(Direction.Up); });
            Demo("ЛР 13 — CQRS", () => { ScoreCommandHandler handler = new ScoreCommandHandler(); handler.Handle(new AddScoreCommand(10)); handler.Handle(new AddScoreCommand(5)); ScoreQueryService query = new ScoreQueryService(handler); Console.WriteLine("   Запрос текущего счёта: " + query.GetCurrentScore()); });
            Demo("ЛР 14 — Repository", () => { IRecordRepository repository = new InMemoryRecordRepository(); repository.Save(new PlayerRecord { PlayerName = "Игрок", BestScore = 30 }); PlayerRecord record = repository.Find("Игрок"); Console.WriteLine("   Рекорд игрока: " + record.BestScore); });
            Demo("ЛР 15 — Producer-Consumer", () => { using (BackgroundTaskQueue queue = new BackgroundTaskQueue()) { queue.Produce(() => Console.WriteLine("   Фоновая задача: сохранение рекорда выполнено.")); queue.Produce(() => Console.WriteLine("   Фоновая задача: запись события выполнена.")); queue.Stop(); } });
            Console.WriteLine("\nДемонстрация завершена.");
        }
        static void Demo(string title, Action action) { Console.WriteLine("\n=== " + title + " ==="); action(); }
    }
}
