using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("- Nhập email tại file emails.txt và email cần sắp xếp tại sortdomain.txt \n- Các email khác sẽ tự xử lý theo đuôi!! \n- Share Free By ZyrM (t.me/dckzyr) ( MSquad )\n> Nhấn phím bất kì để bắt đầu");
        Console.ReadLine();
        string directory = Directory.GetCurrentDirectory();
        string inputFile = Path.Combine(directory, "emails.txt");
        string popularEmailsDirectory = Path.Combine(directory, "PopularEmail");
        string subDomainEmailsDirectory = Path.Combine(directory, "SubDomainEmail");
        string sortDomainFile = Path.Combine(directory, "sortdomain.txt");

        if (!File.Exists(inputFile))
        {
            Console.WriteLine("File lưu trữ email không tồn tại !!");
            return;
        }

        HashSet<string> popularDomains = new HashSet<string>(File.ReadAllLines(sortDomainFile));
        Directory.CreateDirectory(popularEmailsDirectory);
        Directory.CreateDirectory(subDomainEmailsDirectory);

        ConcurrentDictionary<string, List<string>> emailMap = new ConcurrentDictionary<string, List<string>>();
        ConcurrentDictionary<string, List<string>> subdomainMap = new ConcurrentDictionary<string, List<string>>();

        int processedEmailCount = ProcessInputFile(inputFile, popularDomains, emailMap, subdomainMap);

        SaveEmails(emailMap, subdomainMap, popularEmailsDirectory, subDomainEmailsDirectory);

        Console.WriteLine($"Tổng số email hiện tại : {processedEmailCount}");
        Console.WriteLine("Xử lý sắp xếp thành công !!!");
        Console.ReadLine();
    }

    static int ProcessInputFile(string inputFile, HashSet<string> popularDomains, ConcurrentDictionary<string, List<string>> emailMap, ConcurrentDictionary<string, List<string>> subdomainMap)
    {
        int processedEmailCount = 0;
        foreach (string line in File.ReadLines(inputFile))
        {
            if (!line.Contains("@"))
                continue;

            processedEmailCount++;

            Console.WriteLine($"Đang xử lý {processedEmailCount}.");

            string[] parts = line.Split(':');
            string emailAddress = parts[0];
            string domain = emailAddress.Split('@').Last().ToLower();

            if (popularDomains.Contains(domain))
            {
                emailMap.AddOrUpdate(domain, new List<string> { line }, (key, existingList) =>
                {
                    existingList.Add(line);
                    return existingList;
                });
            }
            else
            {
                string subdomain = domain.Split('.').Last();
                subdomainMap.AddOrUpdate(subdomain, new List<string> { line }, (key, existingList) =>
                {
                    existingList.Add(line);
                    return existingList;
                });
            }
        }
        return processedEmailCount;
    }

    static void SaveEmails(ConcurrentDictionary<string, List<string>> emailMap, ConcurrentDictionary<string, List<string>> subdomainMap, string popularEmailsDirectory, string subDomainEmailsDirectory)
    {
        foreach (var entry in emailMap)
        {
            string domain = entry.Key;
            string outputFile = Path.Combine(popularEmailsDirectory, $"{domain}_emails.txt");
            File.AppendAllLines(outputFile, entry.Value);
        }

        foreach (var entry in subdomainMap)
        {
            string subdomain = entry.Key;
            string outputFile = Path.Combine(subDomainEmailsDirectory, $"{subdomain}_emails.txt");
            File.AppendAllLines(outputFile, entry.Value);
        }
    }
}
