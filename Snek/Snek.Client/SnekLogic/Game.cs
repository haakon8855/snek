using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Snek.Client.SnekLogic;

public enum Direction
{
    None,
    Down,
    Right,
    Up,
    Left
}

public enum CellsType
{
    Empty,
    SnekHead,
    Snek,
    Food
}

public class Game
{
    public int Score = 0;
    public int Step = 0;
    private static (int height, int width) Size = (20, 30);
    public CellsType[][] Grid = new CellsType[Size.height][];

    private List<(int y, int x)> _snek = new();
    private (int y, int x) _food = new();

    public Direction CurrentSnekDirection;
    public Direction CurrentFoodDirection;
    public Direction RequestedSnekDirection;
    public Direction RequestedFoodDirection;

    public Replay Replay = new();

    private bool moveFood = true;

    public Random Random = new Random();
    private readonly HttpClient _httpClient;
    public Game()
    {
        InitDemoSnake();
    }

    public Game(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ServerApi");
        Init();
    }

    public void Init(int seed = 0)
    {
        Score = 0;
        Step = 0;
        _snek.Clear();

        // Init replay
        Replay = new Replay();

        // Set random seed
        Replay.Seed = seed;
        if (Replay.Seed == 0)
        {
            Replay.Seed = Random.Next();
        }
        Random = new Random(Replay.Seed);

        for (int i = 0; i < Size.height; i++)
        {
            Grid[i] = new CellsType[Size.width];
        }

        var startCoords = GetRandomCoords();
        _snek.Add((startCoords.y, startCoords.x));
        Grid[_snek.First().y][_snek.First().x] = CellsType.SnekHead;

        var foodStartCoords = GetRandomCoords();
        while (foodStartCoords == startCoords)
        {
            foodStartCoords = GetRandomCoords();
        }

        _food = foodStartCoords;
        Grid[foodStartCoords.y][foodStartCoords.x] = CellsType.Food;

        CurrentSnekDirection = Direction.None;
        CurrentFoodDirection = Direction.None;
        RequestedSnekDirection = Direction.None;
        RequestedFoodDirection = Direction.None;
    }

    public bool Update()
    {
        Step++;
        bool isValidState = UpdateSnek();
        if (moveFood)
        {
            UpdateFood();
        }
        moveFood = !moveFood;

        return isValidState;
    }

    private bool UpdateSnek()
    {
        CurrentSnekDirection = RequestedSnekDirection;

        (int y, int x) nextCoords = (-1, -1);
        nextCoords = CurrentSnekDirection switch
        {
            Direction.Down => GetDown(_snek.First()),
            Direction.Up => GetUp(_snek.First()),
            Direction.Left => GetLeft(_snek.First()),
            Direction.Right => GetRight(_snek.First()),
            _ => _snek.First()
        };

        if (CheckWallCollision(nextCoords))
        {
            return false;
        }
        if (CheckSnekCollision(nextCoords))
        {
            return false;
        }

        bool foodCollision = CheckFoodCollision(nextCoords);

        if (foodCollision)
        {
            GenerateNewFood();
        }

        MoveSnekTo(nextCoords, foodCollision);
        return true;
    }

    private void UpdateFood()
    {
        CurrentFoodDirection = RequestedFoodDirection;

        (int y, int x) nextCoords = CurrentFoodDirection switch
        {
            Direction.Down => GetDown(_food),
            Direction.Up => GetUp(_food),
            Direction.Left => GetLeft(_food),
            Direction.Right => GetRight(_food),
            _ => _food
        };

        if (CheckWallCollision(nextCoords))
        {
            return;
        }
        if (CheckSnekCollision(nextCoords))
        {
            return;
        }

        MoveFoodTo(nextCoords);
    }

    private void GenerateNewFood()
    {
        Score++;

        var nextCoords = GetRandomCoords();

        while (Grid[nextCoords.y][nextCoords.x] != CellsType.Empty)
        {
            nextCoords = GetRandomCoords();
        }

        Grid[nextCoords.y][nextCoords.x] = CellsType.Food;
        _food = nextCoords;

        RequestedFoodDirection = Direction.None;
    }

    public void ChangeSnekDirection(Direction requestedDirection)
    {
        if (DenyChangeDirection(CurrentSnekDirection, requestedDirection))
        {
            return;
        }
        RequestedSnekDirection = requestedDirection;
        Replay.Inputs[Step] = requestedDirection;
    }

    public void ChangeFoodDirection(Direction requestedDirection)
    {
        if (DenyChangeDirection(CurrentFoodDirection, requestedDirection))
        {
            return;
        }
        RequestedFoodDirection = requestedDirection;
    }

    private bool DenyChangeDirection(Direction currentDirection, Direction requestedDirection)
    {
        return ((currentDirection == Direction.Down && requestedDirection == Direction.Up) ||
            (currentDirection == Direction.Up && requestedDirection == Direction.Down) ||
            (currentDirection == Direction.Left && requestedDirection == Direction.Right) ||
            (currentDirection == Direction.Right && requestedDirection == Direction.Left) ||
            (currentDirection == requestedDirection));
    }

    public void MoveSnekTo((int y, int x) nextCoords, bool increaseSnekLength)
    {
        if (!increaseSnekLength)
        {
            Grid[_snek.Last().y][_snek.Last().x] = CellsType.Empty;
            _snek.Remove(_snek.Last());
        }
        if (_snek.Count >= 1)
        {
            Grid[_snek.First().y][_snek.First().x] = CellsType.Snek;
        }

        Grid[nextCoords.y][nextCoords.x] = CellsType.SnekHead;
        _snek.Insert(0, nextCoords);
    }
    public void MoveFoodTo((int y, int x) nextCoords)
    {
        Grid[_food.y][_food.x] = CellsType.Empty;
        Grid[nextCoords.y][nextCoords.x] = CellsType.Food;
        _food = nextCoords;
    }

    public bool CheckWallCollision((int y, int x) coords)
    {
        bool yValid = coords.y >= 0 && coords.y < Size.height;
        bool xValid = coords.x >= 0 && coords.x < Size.width;
        return !yValid || !xValid;
    }

    private bool CheckSnekCollision((int y, int x) coords)
    {
        return Grid[coords.y][coords.x] == CellsType.Snek;
    }

    private bool CheckFoodCollision((int y, int x) coords)
    {
        return Grid[coords.y][coords.x] == CellsType.Food;
    }

    public (int y, int x) GetUp((int y, int x) coords)
    {
        return (coords.y - 1, coords.x);
    }

    public (int y, int x) GetDown((int y, int x) coords)
    {
        return (coords.y + 1, coords.x);
    }

    public (int y, int x) GetLeft((int y, int x) coords)
    {
        return (coords.y, coords.x - 1);
    }

    public (int y, int x) GetRight((int y, int x) coords)
    {
        return (coords.y, coords.x + 1);
    }


    public (int y, int x) GetRandomCoords()
    {
        int y = Random.Next(0, Size.height);
        int x = Random.Next(0, Size.width);
        return (y, x);
    }

    public async Task<HttpStatusCode> GameOver()
    {
        Replay.Score = Score;
        var httpContent = Jsonify(Replay);
        var response = await _httpClient.PostAsync($"api/game/score", httpContent);
        return response.StatusCode;
    }

    public static HttpContent Jsonify(Replay replay)
    {
        var sha256 = SHA256.Create();
        var serializedReplay = JsonSerializer.Serialize(replay);
        byte[] dataBytes = Encoding.UTF8.GetBytes(serializedReplay + "Nei, dette går ikke!");
        byte[] hashedBytes = FoodLogic.ComputeHash(sha256, dataBytes);
        string hashedString = Convert.ToBase64String(hashedBytes);

        return JsonContent.Create(new HighScoreDTO { Replay = replay, Checksum = hashedString });
    }

    public static bool VerifyReplay(Replay replay)
    {
        Game game = new Game();
        game.Init(replay.Seed);

        bool validState = true;

        int step = 0;
        while (validState)
        {
            if (replay.Inputs.ContainsKey(step))
            {
                game.ChangeSnekDirection(replay.Inputs[step]);
            }
            validState = game.Update();
            step++;
        }

        return game.Score == replay.Score;
    }

    public void InitDemoSnake()
    {
        _snek.Clear();
        _snek.Add((9, 11));
        _snek.Add((8, 11));
        _snek.Add((7, 11));
        _snek.Add((6, 11));
        _snek.Add((6, 12));
        _snek.Add((6, 13));
        _snek.Add((6, 14));
        _snek.Add((6, 15));


        // Clear grid
        Grid = new CellsType[Size.height][];
        for (int i = 0; i < Size.height; i++)
        {
            Grid[i] = new CellsType[Size.width];
        }

        Grid[_snek.First().y][_snek.First().x] = CellsType.SnekHead;
        foreach (var coord in _snek.Skip(1))
        {
            Grid[coord.y][coord.x] = CellsType.Snek;
        }

        Grid[12][11] = CellsType.Food;
    }
}
