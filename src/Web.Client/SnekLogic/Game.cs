﻿using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Web.Client.SnekLogic;

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
    public int Score;
    public int Step;
    
    public const int TickLength = 75; // Length of one game tick (ms)
    public static readonly (int Height, int Width) Size = (20, 30);
        
    public CellsType[][] Grid = new CellsType[Size.Height][];

    private readonly List<(int y, int x)> _snek = [];
    private (int y, int x) _food;

    public Direction CurrentSnekDirection;
    public Direction CurrentFoodDirection;
    private Direction _requestedSnekDirection;
    private Direction _requestedFoodDirection;

    private Replay _replay = new();
    private bool _moveFood = true;

    private Random _random = new();
    private readonly HttpClient? _httpClient;

    public Game()
    {
        InitDemoSnake();
    }

    private Game(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ServerApi");
    }

    public static async Task<Game> CreateAsync(IHttpClientFactory httpClientFactory)
    {
        var instance = new Game(httpClientFactory);
        await instance.Init();
        return instance;
    }

    public async Task Init(int seed = 0)
    {
        Score = 0;
        Step = 0;
        _snek.Clear();

        // Init replay
        _replay = new Replay
        {
            Seed = seed
        };

        if (_replay.Seed == 0)
        {
            // Get random seed from server
            if (_httpClient is null)
                return;
            var response = await _httpClient.PostAsync($"api/game/start", new StringContent(""));
            if (response.StatusCode != HttpStatusCode.OK)
                return;
            var json = await response.Content.ReadAsStringAsync();
            var seedDto = JsonSerializer.Deserialize<SeedDTO>(json);
            if (seedDto is null)
                return;
            _replay.Seed = seedDto.Seed;
        }

        _random = new Random(_replay.Seed);

        for (var i = 0; i < Size.Height; i++)
        {
            Grid[i] = new CellsType[Size.Width];
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
        _requestedSnekDirection = Direction.None;
        _requestedFoodDirection = Direction.None;
    }

    public bool Update()
    {
        Step++;
        var isValidState = UpdateSnek();
        if (_moveFood)
        {
            UpdateFood();
        }

        _moveFood = !_moveFood;

        return isValidState;
    }

    private bool UpdateSnek()
    {
        CurrentSnekDirection = _requestedSnekDirection;

        var nextCoords = CurrentSnekDirection switch
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

        var foodCollision = CheckFoodCollision(nextCoords);

        if (foodCollision)
        {
            GenerateNewFood();
        }

        MoveSnekTo(nextCoords, foodCollision);
        return true;
    }

    private void UpdateFood()
    {
        CurrentFoodDirection = _requestedFoodDirection;

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

        _requestedFoodDirection = Direction.None;
    }

    public void ChangeSnekDirection(Direction requestedDirection)
    {
        if (DenyChangeDirection(CurrentSnekDirection, requestedDirection))
        {
            return;
        }

        _requestedSnekDirection = requestedDirection;
        _replay.Inputs[Step] = requestedDirection;
    }

    public void ChangeFoodDirection(Direction requestedDirection)
    {
        if (DenyChangeDirection(CurrentFoodDirection, requestedDirection))
        {
            return;
        }

        _requestedFoodDirection = requestedDirection;
    }

    private bool DenyChangeDirection(Direction currentDirection, Direction requestedDirection)
    {
        return ((currentDirection == Direction.Down && requestedDirection == Direction.Up) ||
                (currentDirection == Direction.Up && requestedDirection == Direction.Down) ||
                (currentDirection == Direction.Left && requestedDirection == Direction.Right) ||
                (currentDirection == Direction.Right && requestedDirection == Direction.Left) ||
                (currentDirection == requestedDirection));
    }

    private void MoveSnekTo((int y, int x) nextCoords, bool increaseSnekLength)
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

    private void MoveFoodTo((int y, int x) nextCoords)
    {
        Grid[_food.y][_food.x] = CellsType.Empty;
        Grid[nextCoords.y][nextCoords.x] = CellsType.Food;
        _food = nextCoords;
    }

    private static bool CheckWallCollision((int y, int x) coords)
    {
        var yValid = coords.y >= 0 && coords.y < Size.Height;
        var xValid = coords.x >= 0 && coords.x < Size.Width;
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

    private static (int y, int x) GetUp((int y, int x) coords)
    {
        return (coords.y - 1, coords.x);
    }

    private static (int y, int x) GetDown((int y, int x) coords)
    {
        return (coords.y + 1, coords.x);
    }

    private static (int y, int x) GetLeft((int y, int x) coords)
    {
        return (coords.y, coords.x - 1);
    }

    private static (int y, int x) GetRight((int y, int x) coords)
    {
        return (coords.y, coords.x + 1);
    }

    private (int y, int x) GetRandomCoords()
    {
        var y = _random.Next(0, Size.Height);
        var x = _random.Next(0, Size.Width);
        return (y, x);
    }

    public async Task<HttpStatusCode> GameOver()
    {
        _replay.Score = Score;
        var httpContent = Jsonify(_replay);
        if (_httpClient is null)
            return HttpStatusCode.InternalServerError;
        var response = await _httpClient.PostAsync($"api/score", httpContent);
        return response.StatusCode;
    }

    public static HttpContent Jsonify(Replay replay)
    {
        var sha256 = SHA256.Create();
        var serializedReplay = JsonSerializer.Serialize(replay);
        var dataBytes = Encoding.UTF8.GetBytes(serializedReplay + "Nei, dette går ikke!");
        var hashedBytes = FoodLogic.ComputeHash(sha256, dataBytes);
        var hashedString = Convert.ToBase64String(hashedBytes);

        return JsonContent.Create(new HighScoreDTO { Replay = replay, Checksum = hashedString });
    }

    public static async Task<bool> ValidateReplay(Replay replay)
    {
        var game = new Game();
        await game.Init(replay.Seed);

        var validState = true;

        var step = 0;
        while (validState)
        {
            if (replay.Inputs.TryGetValue(step, out var input))
            {
                game.ChangeSnekDirection(input);
            }

            validState = game.Update();
            step++;
        }

        return game.Score == replay.Score;
    }

    private void InitDemoSnake()
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
        Grid = new CellsType[Size.Height][];
        for (var i = 0; i < Size.Height; i++)
        {
            Grid[i] = new CellsType[Size.Width];
        }

        Grid[_snek.First().y][_snek.First().x] = CellsType.SnekHead;
        foreach (var coord in _snek.Skip(1))
        {
            Grid[coord.y][coord.x] = CellsType.Snek;
        }

        Grid[12][11] = CellsType.Food;
    }
}