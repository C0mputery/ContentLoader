internal static class Logger {
    public static void Log(string message) {
        Console.WriteLine($"[ContentLoader] {message}");
    }
    
    public static void LogError(string message) {
        Console.WriteLine($"[ContentLoader] [ERROR] {message}");
    }
    
    public static void LogWarning(string message) {
        Console.WriteLine($"[ContentLoader] [WARNING] {message}");
    }
}