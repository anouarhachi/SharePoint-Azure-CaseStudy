using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PnP.Core.Auth;
using PnP.Core.Services;
using PnP.Core.Model.SharePoint;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("--- SharePoint List Initializer ---");

        Console.Write("Enter Client ID: ");
        string clientId = Console.ReadLine()?.Trim();

        Console.Write("Enter Tenant ID: ");
        string tenantId = Console.ReadLine()?.Trim();

        Console.Write("Enter Site URL: ");
        string siteUrl = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(siteUrl))
        {
            Console.WriteLine("Error: All fields are required. Exiting.");
            return;
        }

        var authProvider = new InteractiveAuthenticationProvider(
            clientId,
            tenantId,
            new Uri("http://localhost"));

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddPnPCore();
            })
            .Build();

        await host.StartAsync();

        using (var scope = host.Services.CreateScope())
        {
            var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();

            try 
            {
                Console.WriteLine($"Connecting to {siteUrl}...");
                
                using (var context = await pnpContextFactory.CreateAsync(new Uri(siteUrl), authProvider))
                {
                    string listName = "CaseStudyList";
                    
                    var myList = await context.Web.Lists.GetByTitleAsync(listName);
                    
                    if (myList == null)
                    {
                         Console.WriteLine($"Creating list '{listName}'...");
                         myList = await context.Web.Lists.AddAsync(listName, ListTemplateType.GenericList);
                         Console.WriteLine("List created successfully.");
                    }
                    else
                    {
                        Console.WriteLine("List already exists. Appending items.");
                    }

                    Console.WriteLine("Adding 5 items...");
                    for (int i = 1; i <= 5; i++)
                    {
                        Dictionary<string, object> itemValues = new Dictionary<string, object>
                        {
                            { "Title", $"Project Item {i}" }
                        };
                        
                        await myList.Items.AddAsync(itemValues);
                        Console.WriteLine($"Added Item {i}");
                    }
                }
                Console.WriteLine("Operation completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}