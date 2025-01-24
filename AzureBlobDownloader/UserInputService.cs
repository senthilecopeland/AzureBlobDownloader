using Microsoft.Extensions.Configuration;

namespace AzureBlobDownloader
{
    public interface IUserInputService
    {
        void GetUserInput();
    }

    public class UserInputService : IUserInputService
    {
        private readonly IConfiguration _configuration;

        public UserInputService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void GetUserInput()
        {
            Console.Write("Enter Blob filter words (use comma for mulitiple): ");
            _configuration["Filer"] = Console.ReadLine();

            Console.Write("Enter the search string: ");
            _configuration["Search"] = Console.ReadLine();

            Console.Write("Enter Search From Date (yyyy-MM-dd): ");
            _configuration["FromDate"] = Console.ReadLine();

            Console.Write("Enter Search To Date (yyyy-MM-dd): ");
            _configuration["ToDate"] = Console.ReadLine();
        }
    }
}
