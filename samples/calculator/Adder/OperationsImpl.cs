namespace ComputerWorld.wit.exports.example.calculator;

public class ComputerWorldImpl : IComputerWorld
{
    public static int Add(int left, int right)
    {
        return left + right;
    }

    public static string ToUpper(string input)
    {
        return input.ToUpperInvariant();
    }
}
