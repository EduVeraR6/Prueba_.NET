using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




builder.Services.AddAuthorization(policy =>
{
    policy.AddPolicy("ClientPolicy", p =>
    {
        p.RequireClaim("client_id", "movies_mvc_client");
    });
});



var app = builder.Build();


using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
