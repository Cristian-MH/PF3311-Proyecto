using PF3311.Telerehab.API.Data;
using PF3311.Telerehab.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<InMemoryDatabase>();
builder.Services.AddHttpClient<MotivationService>();
builder.Services.AddHttpClient<OpenAiTherapyService>();
builder.Services.AddHttpClient<SpeechService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
