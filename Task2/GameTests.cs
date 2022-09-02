using FluentAssertions;
using Task1;

namespace Task2;

public class GameTests
{
    private readonly Random _rand;

    public GameTests()
    {
        _rand = new Random(1234);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    [InlineData(-1000)]
    public void GetScore_ShouldReturnZeroZeroScore_WhenOffsetIsNotPositive(int negativeOffset)
    {
        // Arrange
        var stamps = TestDataGenerator.GenerateRandomizedStamps(_rand);
        var game = new Game(stamps);

        // Act
        var score = game.getScore(negativeOffset);

        // Assert
        score.Should().Be(new Score(0, 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(1000)]
    public void GetScore_ShouldReturnFinalScore_WhenOffsetIsEqualOrGreaterThanTheLastOne(int offsetExcess)
    {
        // Arrange
        var stamps = TestDataGenerator.GenerateRandomizedStamps(_rand);
        var game = new Game(stamps);

        // Act
        var score = game.getScore(stamps.Last().offset - 1 + offsetExcess);

        // Assert
        score.Should().Be(stamps.Last().score);
    }

    [Fact]
    public void GetScore_ShouldReturnZeroZeroScore_WhenGameStampsArrayIsEmpty()
    {
        // Arrange
        var game = new Game();

        // Act
        var score = game.getScore(_rand.Next(1, 50001));

        // Assert
        score.Should().Be(new Score(0, 0));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(1000)]
    [InlineData(12500)]
    [InlineData(27632)]
    public void GetScore_ShouldReturnCorrectScore_WhenOffsetIsCorrectAndPresentedInGameStampsArray(int offset)
    {
        // Arrange
        var stamps = TestDataGenerator.GenerateStampsWithDefinedOffsets(_rand, offsets: offset);
        var game = new Game(stamps);

        // Act
        var score = game.getScore(offset);

        // Assert
        score.Should().Be(stamps.FirstOrDefault(s => s.offset == offset).score);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(1000)]
    [InlineData(12500)]
    [InlineData(27632)]
    public void GetScore_ShouldReturnCorrectScore_WhenOffsetIsCorrectButNotPresentedInGameStampsArray(int offset)
    {
        // Arrange
        var stamps = TestDataGenerator.GenerateStampsWithoutDefinedOffsets(_rand, offsets: offset);
        var game = new Game(stamps);

        // Act
        var score = game.getScore(offset);

        // Assert
        score.Should().Be(
            stamps.Where(s => s.offset < offset)
                .Last().score);
    }
}

public class TestDataGenerator
{
    public static GameStamp[] GenerateRandomizedStamps(Random rand,
        int count = 50000,
        int maxOffsetStep = 3,
        double scoreChangeProbability = 0.0001,
        double homeScoreProbability = 0.45)
    {
        if (count <= 0)
        {
            return new GameStamp[0];
        }

        var gameStamps = new GameStamp[count];
        gameStamps[0] = new GameStamp(0, 0, 0);

        for (int i = 1; i < count; i++)
        {
            gameStamps[i] = GenerateGameStamp(rand,
                gameStamps[i - 1],
                maxOffsetStep,
                scoreChangeProbability,
                homeScoreProbability);
        }

        return gameStamps;
    }

    public static GameStamp[] GenerateStampsWithDefinedOffsets(Random rand,
        int count = 50000,
        int maxOffsetStep = 3,
        double scoreChangeProbability = 0.0001,
        double homeScoreProbability = 0.45,
        params int[] offsets)
    {
        if (count <= 0)
        {
            return new GameStamp[0];
        }

        var definedOffsets = offsets.Where(off => off >= 0)
            .OrderBy(off => off)
            .ToArray();

        var gameStamps = new GameStamp[count];
        gameStamps[0] = new GameStamp(0, 0, 0);

        var definedOffsetsCurrentIndex = (definedOffsets[0] == 0) ? 1 : 0;

        for (int i = 1; i < count; i++)
        {
            gameStamps[i] = GenerateGameStamp(rand,
                gameStamps[i - 1],
                maxOffsetStep,
                scoreChangeProbability,
                homeScoreProbability);

            if (definedOffsetsCurrentIndex < definedOffsets.Length
                && definedOffsets[definedOffsetsCurrentIndex] <= gameStamps[i].offset)
            {
                gameStamps[i].offset = definedOffsets[definedOffsetsCurrentIndex];
                definedOffsetsCurrentIndex++;
            }
        }

        return gameStamps;
    }

    public static GameStamp[] GenerateStampsWithoutDefinedOffsets(Random rand,
        int count = 50000,
        int maxOffsetStep = 3,
        double scoreChangeProbability = 0.0001,
        double homeScoreProbability = 0.45,
        params int[] offsets)
    {
        if (count <= 0)
        {
            return new GameStamp[0];
        }

        var excludedOffsets = offsets.Where(off => off > 0)
            .OrderBy(off => off)
            .ToArray();

        var gameStamps = new GameStamp[count];
        gameStamps[0] = new GameStamp(0, 0, 0);

        var excludedOffsetsCurrentIndex = 0;

        for (int i = 1; i < count; i++)
        {
            gameStamps[i] = GenerateGameStamp(rand,
                gameStamps[i - 1],
                maxOffsetStep,
                scoreChangeProbability,
                homeScoreProbability);

            if (excludedOffsetsCurrentIndex < excludedOffsets.Length)
            {
                if (excludedOffsets[excludedOffsetsCurrentIndex] < gameStamps[i].offset)
                {
                    excludedOffsetsCurrentIndex++;
                }

                else if (excludedOffsets[excludedOffsetsCurrentIndex] == gameStamps[i].offset)
                {
                    if (gameStamps[i - 1].offset == gameStamps[i].offset - 1)
                    {
                        gameStamps[i].offset++;
                    }
                    else
                    {
                        gameStamps[i].offset--;
                    }
                    excludedOffsetsCurrentIndex++;
                }
            }
        }

        return gameStamps;
    }

    private static GameStamp GenerateGameStamp(Random rand,
        GameStamp previousStamp,
        int maxOffsetStep = 3,
        double scoreChangeProbability = 0.0001,
        double homeScoreProbability = 0.45)
    {
        bool scoreChanged = rand.NextDouble() > 1 - scoreChangeProbability;
        int homeScoreChange = scoreChanged && rand.NextDouble() > 1 - homeScoreProbability ? 1 : 0;
        int awayScoreChange = scoreChanged && homeScoreChange == 0 ? 1 : 0;
        int offsetChange = (int)(Math.Floor(rand.NextDouble() * maxOffsetStep)) + 1;

        return new GameStamp(
            previousStamp.offset + offsetChange,
            previousStamp.score.home + homeScoreChange,
            previousStamp.score.away + awayScoreChange
        );
    }
}