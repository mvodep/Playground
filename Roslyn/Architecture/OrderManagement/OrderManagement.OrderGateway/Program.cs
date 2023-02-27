using OrderManagement.OrderProcessing;

namespace OrderManagement.OrderGateway
{
    internal class Program
    {
        static void Main(string[] args)
        {
            OrderProcessor.Generate();
        }
    }
}