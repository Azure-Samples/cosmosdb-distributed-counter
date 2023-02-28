using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Versioning;

namespace website.Pages;

public class CounterModel : PageModel
{
    public string ErrorMessage = string.Empty;

    private DistributedCounterHelper helper = new DistributedCounterHelper();

    private readonly ILogger<IndexModel> _logger;

    public CounterModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public async Task OnGet(string lockName, string clientId)
    {
        try
        {
            var gLock = await helper.RetrieveCounterAsync(lockName);
            await helper.ResetCounter(gLock);
        }
        catch
        {

        }

        Redirect("Index");
    }

    
}
