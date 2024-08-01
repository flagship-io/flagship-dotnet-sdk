//start demo
// Usage: node demo/Program.cs
using Flagship.Hit;
using Flagship.Main;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "swagger")),
    RequestPath = "/swagger"
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Documentation v1");
    });
}

// Step 1: Start the Flagship SDK by providing the environment ID and API key
Fs.Start("<ENV_ID>", "<API_KEY>");

var visitorId = "visitor-id";

// Endpoint to get an item
app.MapGet("/item", async (HttpContext context) =>
{
    var isVipQuery = context.Request.Query.FirstOrDefault(q => q.Key == "isVip").Value;
    var isVip = string.Equals(isVipQuery, "true", StringComparison.OrdinalIgnoreCase);

    // Step 2: Create a new visitor with a visitor ID and consent status
    var visitor = Fs.NewVisitor(visitorId, true)
        .SetContext(new Dictionary<string, object>
        {
            { "fs_is_vip", isVip }
        })
        .Build();

    // Step 3: Fetch the flags for the visitor
    await visitor.FetchFlags();

    // Step 4: Get the values of the flags for the visitor
    var fsEnableDiscount = visitor.GetFlag("fs_enable_discount");
    var fsAddToCartBtnColor = visitor.GetFlag("fs_add_to_cart_btn_color");

    var fsEnableDiscountValue = fsEnableDiscount.GetValue(false);
    var fsAddToCartBtnColorValue = fsAddToCartBtnColor.GetValue("blue");

        return Results.Ok(new
        {
            item = new { name = "Flagship T-shirt", price = 20 },
            fsEnableDiscount = fsEnableDiscountValue,
            fsAddToCartBtnColor = fsAddToCartBtnColorValue
        });
})
.WithName("GetItem")
.WithOpenApi();

// Endpoint to add an item to the cart
app.MapPost("/add-to-cart", (HttpContext context) =>
{
    var visitor = Fs.NewVisitor(visitorId, true)
        .SetContext(new Dictionary<string, object>
        {
            { "fs_is_vip", true }
        })
        .Build();

    // Step 5: Send a hit to track an action
    var eventHit = new Event(EventCategory.ACTION_TRACKING, "add-to-cart-clicked");
    _ = visitor.SendHit(eventHit);

    return Results.Ok();
})
.WithName("AddToCart")
.WithOpenApi();

app.Run();
//end demo