namespace Task1
{
    class App
    {
        static void Main(string[] args)
        {
            Game.task1();
        }
    }
    public struct Score
    {
        public int home;
        public int away;

        public Score(int home, int away)
        {
            this.home = home;
            this.away = away;
        }
    }

    public struct GameStamp
    {
        public int offset;
        public Score score;
        public GameStamp(int offset, int home, int away)
        {
            this.offset = offset;
            this.score = new Score(home, away);
        }
    }

    public class Game
    {
        const int TIMESTAMPS_COUNT = 50000;

        const double PROBABILITY_SCORE_CHANGED = 0.0001;

        const double PROBABILITY_HOME_SCORE = 0.45;

        const int OFFSET_MAX_STEP = 3;

        GameStamp[] gameStamps;

        public Game()
        {
            this.gameStamps = new GameStamp[] { };
        }

        public Game(GameStamp[] gameStamps)
        {
            this.gameStamps = gameStamps;
        }

        GameStamp generateGameStamp(GameStamp previousValue)
        {
            Random rand = new Random();

            bool scoreChanged = rand.NextDouble() > 1 - PROBABILITY_SCORE_CHANGED;
            int homeScoreChange = scoreChanged && rand.NextDouble() > 1 - PROBABILITY_HOME_SCORE ? 1 : 0;
            int awayScoreChange = scoreChanged && homeScoreChange == 0 ? 1 : 0;
            int offsetChange = (int)(Math.Floor(rand.NextDouble() * OFFSET_MAX_STEP)) + 1;

            return new GameStamp(
                previousValue.offset + offsetChange,
                previousValue.score.home + homeScoreChange,
                previousValue.score.away + awayScoreChange
                );
        }

        static Game generateGame()
        {
            Game game = new Game();
            game.gameStamps = new GameStamp[TIMESTAMPS_COUNT];

            GameStamp currentStamp = new GameStamp(0, 0, 0);
            for (int i = 0; i < TIMESTAMPS_COUNT; i++)
            {
                game.gameStamps[i] = currentStamp;
                currentStamp = game.generateGameStamp(currentStamp);
            }

            return game;
        }

        public static void task1()
        {
            Game game = generateGame();
            game.printGameStamps();
        }

        void printGameStamps()
        {
            foreach (GameStamp stamp in this.gameStamps)
            {
                Console.WriteLine($"{stamp.offset}: {stamp.score.home}-{stamp.score.away}");
            }
        }

        public Score getScore(int offset)
        {
            if (offset < 0)
            {
                return new Score(0, 0);
            }

            /* Бинарный левосторонний включающий поиск:
            ищем индекс элемента в массиве this.gameStamps,
            offset которого меньше или равен требуемому offset*/
            int leftIndex = 0;
            int rightIndex = this.gameStamps.Length - 1;
            int middleIndex = 0;
            int middleOffset = 0;
            int binarySearchResultIndex = -1;

            while (leftIndex <= rightIndex)
            {
                /*Вычисляем (leftIndex + rightIndex + 1) / 2 способом,
                позволяющим избежать переполнения*/
                middleIndex = leftIndex + ((rightIndex - leftIndex + 1) / 2);
                middleOffset = this.gameStamps[middleIndex].offset;

                if (middleOffset < offset)
                {
                    leftIndex = middleIndex + 1;
                    binarySearchResultIndex = middleIndex;
                }
                else if (middleOffset > offset)
                {
                    rightIndex = middleIndex - 1;
                }
                else
                {
                    binarySearchResultIndex = middleIndex;
                    break;
                }
            }

            /*Все элементы в массиве this.gameStamps имеют
            offset больше, чем требуемый. Значит, счёт игры 0-0*/
            if (binarySearchResultIndex == -1)
            {
                return new Score(0, 0);
            }

            return this.gameStamps[binarySearchResultIndex].score;
        }
    }
}