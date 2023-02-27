
namespace OrderManagement.OrderProcessing.Sub1
{
    internal static class Class1
    {
        public static List<int> GetPrimesInRange(int num)
        {
            List<int> primes = new()
            {
                2
            };

            for (int i = 1; i <= num; i = i + 2)
            {
                if (DetermineIsPrime(i) == true && i > 1)
                {
                    primes.Add(i);
                }
            }
            return primes;
        }

        static bool DetermineIsPrime(int num)
        {
            int y;

            List<int> divisors = new();

            double x = Math.Sqrt(num);

            y = (int)Math.Ceiling(x);

            if (num == 3 || num == 2)
            {
                return true;
            }
            for (int counter = 1; counter <= y + 1; counter++)
            {
                if (num % counter == 0)
                {
                    divisors.Add(counter);

                    if (divisors.Count > 1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
