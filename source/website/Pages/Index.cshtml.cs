using Cosmos_Patterns_DistributedCounter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Versioning;

namespace website.Pages;

public class IndexModel : PageModel
{
    public string ErrorMessage = string.Empty;

    public List<Counter> Counters = new List<Counter>();
    
    private DistributedCounterHelper helper = new DistributedCounterHelper();

    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public async Task OnGet()
    {
        await GetCounters();
    }

    public async Task<IActionResult> OnPost(){

        string name = Request.Form["Name"];
        string partitions = Request.Form["Partitions"];

        await helper.SaveCounter(name, partitions);

        //update the locks
        await GetCounters();

        return Page();
    }

    private async Task GetCounters()
    {
        Counters = (await helper.RetrieveAllCountersAsync()).ToList();
    }
}
