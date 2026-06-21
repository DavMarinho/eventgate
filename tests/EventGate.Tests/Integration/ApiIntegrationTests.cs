using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace EventGate.Tests.Integration;

public class ApiIntegrationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private static readonly byte[] PhotoBytes = [0x89, 0x50, 0x4E, 0x47, 1, 2, 3, 4];

    [Fact]
    public async Task Health_Returns_Healthy()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("/health");
        res.EnsureSuccessStatusCode();
        Assert.Equal("Healthy", await res.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Courses_AreSeeded()
    {
        var client = _factory.CreateClient();
        var courses = await client.GetFromJsonAsync<JsonElement>("/api/courses");
        Assert.True(courses.GetArrayLength() >= 60);
    }

    [Fact]
    public async Task Protected_Endpoint_Requires_Auth()
    {
        var client = _factory.CreateClient();
        var res = await client.PostAsync("/api/checkin/validate",
            JsonContent.Create(new { accessCode = "X" }));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Full_Flow_Register_Lookup_Validate_BlocksReuse()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // cria evento
        var evRes = await client.PostAsJsonAsync("/api/events", new
        {
            name = "Evento Integração",
            startsAt = DateTimeOffset.UtcNow.AddDays(5),
            capacity = 50
        });
        evRes.EnsureSuccessStatusCode();
        var eventId = (await evRes.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString();

        // inscrição (multipart com foto)
        using var form = new MultipartFormDataContent
        {
            { new StringContent("Maria Teste"), "participantName" },
            { new StringContent("maria@aluno.usp.br"), "participantEmail" },
            { new StringContent("2003-04-12"), "birthDate" },
            { new StringContent("true"), "consentAccepted" },
            { new StringContent("Externo"), "courseOther" },
        };
        var photo = new ByteArrayContent(PhotoBytes);
        photo.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(photo, "photo", "p.png");

        var regRes = await client.PostAsync($"/api/events/{eventId}/registrations", form);
        regRes.EnsureSuccessStatusCode();
        var code = (await regRes.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessCode").GetString();
        Assert.False(string.IsNullOrWhiteSpace(code));

        // lookup mostra a foto
        var lookup = await client.GetFromJsonAsync<JsonElement>($"/api/checkin/lookup?code={code}");
        Assert.True(lookup.GetProperty("found").GetBoolean());
        Assert.StartsWith("data:image/png;base64,", lookup.GetProperty("photoDataUri").GetString());

        // valida entrada
        var v1 = await client.PostAsync("/api/checkin/validate", JsonContent.Create(new { accessCode = code }));
        var v1Body = await v1.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(v1Body.GetProperty("valid").GetBoolean());

        // reuso é bloqueado
        var v2 = await client.PostAsync("/api/checkin/validate", JsonContent.Create(new { accessCode = code }));
        var v2Body = await v2.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(v2Body.GetProperty("valid").GetBoolean());
    }

    private static async Task<string> LoginAsync(HttpClient client)
    {
        var res = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@eventgate.local",
            password = "Admin@123"
        });
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }
}
