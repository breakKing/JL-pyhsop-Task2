using FluentAssertions;
using Task1;

namespace Task2;

public class GameTests
{
    private readonly Random _rand;

    public GameTests()
    {
        _rand = new Random(420);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-50)]
    [InlineData(-1000)]
    public void GetScore_ShouldReturnZeroZeroScore_WhenOffsetIsNegative(int negativeOffset)
    {
        // Arrange
        var stamps = TestDataGenerator.GenerateRandomizedStamps(_rand);
        Game game = new Game(stamps);

        // Act
        Score score = game.getScore(negativeOffset);

        // Assert
        score.Should().Be(new Score(0, 0));
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