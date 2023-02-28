using Microsoft.AspNetCore.Mvc;
using Versioning;


public class CounterController : Controller {

    private DistributedCounterHelper helper = new DistributedCounterHelper();

    [HttpGet("Reset/{name}")]
    public async Task<IActionResult> Reset(string name){
        var gLock = await helper.RetrieveCounterAsync(name);
        await helper.ResetCounter(gLock);
        return RedirectToPage("/Index");
    }
}