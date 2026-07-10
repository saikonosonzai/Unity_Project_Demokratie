//Control game state here

namespace AdventurePuzzleKit
{
    public static class GameState
    {
        public static bool IsExamining { get; set; }
        public static bool IsInventoryOpen { get; set; }
        public static bool IsUsingSystem { get; set; }
        public static bool isGamePaused { get; set; }

        // A combined property to check if the player is "busy" with any major state
        public static bool IsPlayerBusy => IsExamining || IsInventoryOpen || isGamePaused || IsInteracting;

        public static bool IsInteracting => IsUsingSystem || IsExamining || isGamePaused;
    }
}

