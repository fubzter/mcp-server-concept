using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AteaTimeMcp.Services;

public sealed class AteaTimeMcpService
{
    private const string ApiBase = "https://mobile.atea.com/AteaTimeRegistrationProduction//api/";
    private const string Passphrase = "ÅÉTÅ-ØNÆÅpp";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;
    private readonly ILogger<AteaTimeMcpService> _logger;

    public AteaTimeMcpService(HttpClient client, ILogger<AteaTimeMcpService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public Task<object> WhoAmI()
    {
        var session = LoadSession();
        return Task.FromResult(PublicIdentity(session));
    }

    public async Task<object> ListOpenCases(string? date = null, string? filter = null, int limit = 100)
    {
        var session = LoadSession();
        var selectedDate = string.IsNullOrWhiteSpace(date) ? Today() : date;
        var projectCases = await GetArray(session, $"projectcases?employeeId={Uri.EscapeDataString(session.UserInitial)}&fromDate={selectedDate}&toDate={selectedDate}");
        var plannedCases = await GetArray(session, $"cases?employeeId={Uri.EscapeDataString(session.UserInitial)}&fromDate={selectedDate}&toDate={selectedDate}&documentNumber=&onlyActive=false");

        return new
        {
            date = selectedDate,
            identity = PublicIdentity(session),
            projectCases = SummarizeCases(projectCases, filter, limit),
            plannedCases = SummarizeCases(plannedCases, filter, limit)
        };
    }

    public async Task<object> ListDrafts(bool details = false)
    {
        var session = LoadSession();
        var drafts = await GetArray(session, $"drafts/{Uri.EscapeDataString(session.UserInitial)}");
        return details ? SummarizeDraftsDetailed(drafts) : SummarizeDrafts(drafts);
    }

    public async Task<object> WeekSummary(string? date = null)
    {
        var session = LoadSession();
        var drafts = await GetArray(session, $"drafts/{Uri.EscapeDataString(session.UserInitial)}");
        return SummarizeWeek(drafts, string.IsNullOrWhiteSpace(date) ? Today() : date);
    }

    public async Task<object> CreateDraft(
        string job,
        string task,
        string appointment,
        string date,
        string from,
        string to,
        string description,
        bool confirm = false,
        string? internalComment = null,
        bool lastVisit = false,
        string workLocation = "2")
    {
        var session = LoadSession();
        var details = await GetObject(session, $"ProjectCaseDetails?Job={Uri.EscapeDataString(job)}&Task={Uri.EscapeDataString(task)}&Resource={Uri.EscapeDataString(session.UserInitial)}&AppointmentID={Uri.EscapeDataString(appointment)}");
        var payload = BuildProjectDraftPayload(session, details, job, task, appointment, date, from, to, description, internalComment, lastVisit, workLocation);

        if (!confirm)
        {
            return new { dryRun = true, message = "No draft was created. Re-run with confirm=true to create it.", payload };
        }

        var result = await Write(session, HttpMethod.Post, "Registration", payload);
        return new { created = true, status = result.Status, response = SummarizeWriteResponse(result.Body) };
    }

    public async Task<object> CreateInternalDraft(string date, string from, string to, string description, bool confirm = false)
    {
        var session = LoadSession();
        var payload = BuildInternalDraftPayload(session, date, from, to, description);

        if (!confirm)
        {
            return new { dryRun = true, message = "No internal draft was created. Re-run with confirm=true to create it.", payload };
        }

        var result = await Write(session, HttpMethod.Post, "Registration", payload);
        return new { created = true, status = result.Status, response = SummarizeWriteResponse(result.Body) };
    }

    public async Task<object> DeleteDraft(int id, bool confirm = false)
    {
        var session = LoadSession();
        if (!confirm)
        {
            return new { dryRun = true, message = "No draft was deleted. Re-run with confirm=true to delete it.", id };
        }

        var result = await Write(session, HttpMethod.Delete, $"Registration/{id}", null);
        return new { deleted = true, status = result.Status, id, response = result.Body };
    }

    public async Task<object> SubmitDrafts(string from, string to, bool confirm = false)
    {
        var session = LoadSession();
        var drafts = await GetArray(session, $"drafts/{Uri.EscapeDataString(session.UserInitial)}");
        var registrations = FlattenDraftRegistrations(drafts)
            .Where(item =>
            {
                var date = GetString(item, "fromDate")?.Split('T')[0] ?? "";
                return string.CompareOrdinal(date, from) >= 0
                    && string.CompareOrdinal(date, to) <= 0
                    && GetString(item, "status") == "Draft"
                    && GetBool(item, "isSubmitted") != true;
            })
            .ToList();

        if (!confirm)
        {
            return new
            {
                dryRun = true,
                message = "No drafts were submitted. Re-run with confirm=true to submit them.",
                from,
                to,
                drafts = registrations.Select(SummarizeRegistration)
            };
        }

        var results = new List<object>();
        foreach (var registration in registrations)
        {
            var result = await Write(session, HttpMethod.Put, "Registration", registration);
            results.Add(new { id = GetInt(registration, "id"), status = result.Status, response = SummarizeWriteResponse(result.Body) });
        }

        return new { submitted = true, from, to, results };
    }

    private async Task<JsonArray> GetArray(AteaSession session, string endpoint)
    {
        var node = await Send(session, HttpMethod.Get, endpoint, null);
        return node as JsonArray ?? new JsonArray();
    }

    private async Task<JsonObject> GetObject(AteaSession session, string endpoint)
    {
        var node = await Send(session, HttpMethod.Get, endpoint, null);
        return node as JsonObject ?? new JsonObject();
    }

    private async Task<(int Status, JsonNode? Body)> Write(AteaSession session, HttpMethod method, string endpoint, object? payload)
    {
        using var request = BuildRequest(session, method, endpoint, payload);
        using var response = await _client.SendAsync(request);
        var body = await ParseBody(response);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"{method} {endpoint} failed with HTTP {(int)response.StatusCode}: {body}");
        }

        return ((int)response.StatusCode, body);
    }

    private async Task<JsonNode?> Send(AteaSession session, HttpMethod method, string endpoint, object? payload)
    {
        using var request = BuildRequest(session, method, endpoint, payload);
        using var response = await _client.SendAsync(request);
        var body = await ParseBody(response);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"{method} {endpoint} failed with HTTP {(int)response.StatusCode}: {body}");
        }

        return body;
    }

    private static HttpRequestMessage BuildRequest(AteaSession session, HttpMethod method, string endpoint, object? payload)
    {
        var request = new HttpRequestMessage(method, new Uri(new Uri(ApiBase), endpoint));
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.Authorization = new("Bearer", session.AccessToken);
        if (payload is not null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");
        }

        return request;
    }

    private static async Task<JsonNode?> ParseBody(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(text)) return new JsonObject();
        try
        {
            return JsonNode.Parse(text);
        }
        catch
        {
            return JsonValue.Create(text);
        }
    }

    private AteaSession LoadSession()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var directories = new[]
        {
            Path.Combine(home, "Library/Application Support/Codex/Default/Partitions/codex-browser-app/Local Storage/leveldb"),
            Path.Combine(home, "Library/Application Support/Google/Chrome/Default/Local Storage/leveldb"),
            Path.Combine(home, "AppData/Local/Google/Chrome/User Data/Default/Local Storage/leveldb")
        };

        var sessions = new List<AteaSession>();
        foreach (var directory in directories.Where(Directory.Exists))
        {
            foreach (var file in Directory.EnumerateFiles(directory))
            {
                TryReadSessions(file, sessions);
            }
        }

        var session = sessions
            .Where(item => item.Expires > DateTimeOffset.UtcNow)
            .OrderByDescending(item => item.Expires)
            .FirstOrDefault();

        return session ?? throw new InvalidOperationException("No valid local Atea Time session found. Log in to Atea Time in the same browser profile first.");
    }

    private void TryReadSessions(string file, List<AteaSession> sessions)
    {
        try
        {
            var data = File.ReadAllBytes(file);
            var text = Encoding.Latin1.GetString(data);
            var index = 0;
            while ((index = text.IndexOf("U2FsdGVkX1", index, StringComparison.Ordinal)) >= 0)
            {
                var end = index;
                while (end < text.Length && IsBase64Char(text[end])) end++;
                var encrypted = text[index..end];
                if (encrypted.Length > 80)
                {
                    TryAddSession(encrypted, sessions);
                }

                index = end;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not inspect local storage file {File}", file);
        }
    }

    private static void TryAddSession(string encrypted, List<AteaSession> sessions)
    {
        try
        {
            var json = DecryptCryptoJs(encrypted);
            var node = JsonNode.Parse(json)?.AsObject();
            var token = node?["token"]?.AsObject();
            var accessToken = GetString(token, "access_token");
            var expiresRaw = GetString(token, "expires");
            var userInitial = GetString(node, "userInitial");
            var userEmail = GetString(node, "userEmail");
            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(userInitial) || string.IsNullOrWhiteSpace(expiresRaw)) return;
            if (!DateTimeOffset.TryParse(expiresRaw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var expires)) return;

            sessions.Add(new AteaSession(
                userInitial,
                userEmail ?? "",
                accessToken,
                expires,
                GetString(node, "costCenterId") ?? "",
                GetString(node, "managerId") ?? "",
                GetString(node, "OvertimeModel") ?? ""));
        }
        catch
        {
            // Ignore stale or partial LevelDB records.
        }
    }

    private static string DecryptCryptoJs(string encrypted)
    {
        var raw = Convert.FromBase64String(encrypted);
        if (Encoding.ASCII.GetString(raw, 0, 8) != "Salted__") throw new InvalidOperationException("Unsupported token format");
        var salt = raw.Skip(8).Take(8).ToArray();
        var (key, iv) = EvpBytesToKey(Encoding.UTF8.GetBytes(Passphrase), salt, 32, 16);
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var decryptor = aes.CreateDecryptor();
        var cipher = raw.Skip(16).ToArray();
        var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plain);
    }

    private static (byte[] Key, byte[] Iv) EvpBytesToKey(byte[] password, byte[] salt, int keyLength, int ivLength)
    {
        var bytes = new List<byte>();
        var last = Array.Empty<byte>();
        while (bytes.Count < keyLength + ivLength)
        {
            using var md5 = MD5.Create();
            last = md5.ComputeHash(last.Concat(password).Concat(salt).ToArray());
            bytes.AddRange(last);
        }

        return (bytes.Take(keyLength).ToArray(), bytes.Skip(keyLength).Take(ivLength).ToArray());
    }

    private static object BuildProjectDraftPayload(
        AteaSession session,
        JsonObject details,
        string job,
        string task,
        string appointment,
        string date,
        string from,
        string to,
        string description,
        string? internalComment,
        bool lastVisit,
        string workLocation)
    {
        var quantity = HoursBetween(from, to);
        return new Dictionary<string, object?>
        {
            ["RegistrationType"] = "Project",
            ["DocumentNo"] = job,
            ["Taskno"] = task,
            ["AppointmentID"] = appointment,
            ["TimeMaterial"] = GetBool(details, "t_m") ?? false,
            ["WorkLocation"] = workLocation,
            ["IsEdited"] = false,
            ["FromDate"] = date,
            ["CreatedDate"] = DateTimeOffset.UtcNow.ToString("O"),
            ["TechComment"] = (internalComment ?? "").ReplaceLineEndings(" "),
            ["Description"] = description.ReplaceLineEndings(" "),
            ["FromTime"] = from,
            ["ToTime"] = to,
            ["CaseName"] = GetString(details, "name"),
            ["ServiceItemLineNo"] = task,
            ["DeadLine"] = GetString(details, "r_time"),
            ["CategoryName"] = "Work",
            ["CategoryID"] = 1,
            ["Department"] = "",
            ["DepartmentDimension"] = session.CostCenterId,
            ["EmployeeID"] = session.UserInitial,
            ["Approver"] = session.ManagerId,
            ["ApprovedOn"] = null,
            ["Status"] = "Draft",
            ["ModelName"] = session.OvertimeModel.Replace(" ", ""),
            ["ModelID"] = session.OvertimeModel.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "",
            ["Type"] = "",
            ["IsLastVisit"] = lastVisit,
            ["MilliageHour"] = 0,
            ["Quantity"] = quantity,
            ["OV150"] = "0.0",
            ["OV200"] = "0.0",
            ["IsOTSalaryModelEnabled"] = false
        };
    }

    private static object BuildInternalDraftPayload(AteaSession session, string date, string from, string to, string description)
    {
        return new Dictionary<string, object?>
        {
            ["RegistrationType"] = "Internal",
            ["DocumentNo"] = "13325-1-147",
            ["Taskno"] = "001",
            ["TimeMaterial"] = false,
            ["IsEdited"] = false,
            ["FromDate"] = date,
            ["CreatedDate"] = DateTimeOffset.UtcNow.ToString("O"),
            ["Description"] = description.ReplaceLineEndings(" "),
            ["FromTime"] = from,
            ["ToTime"] = to,
            ["ServiceItemLineNo"] = "0",
            ["CategoryName"] = "Intern Tid-Uddannelse",
            ["CategoryID"] = 11,
            ["EmployeeID"] = session.UserInitial,
            ["Status"] = "Draft",
            ["IsLastVisit"] = false,
            ["MilliageHour"] = 0,
            ["Quantity"] = HoursBetween(from, to),
            ["OV150"] = "0.0",
            ["OV200"] = "0.0",
            ["IsOTSalaryModelEnabled"] = false
        };
    }

    private static object SummarizeDrafts(JsonArray drafts)
    {
        return new
        {
            count = drafts.Count,
            items = drafts.Select(item => new
            {
                date = GetString(item, "fromDate"),
                draftCount = GetInt(item, "regCount"),
                partsCount = GetInt(item, "parts"),
                assetCount = GetInt(item, "assetHanding"),
                callTypeCount = GetInt(item, "callType")
            })
        };
    }

    private static object SummarizeDraftsDetailed(JsonArray drafts)
    {
        return new
        {
            count = drafts.Count,
            dates = drafts.Select(item => new
            {
                date = GetString(item, "fromDate"),
                internalRegistrations = item?["lstInternalReg"] as JsonArray ?? new JsonArray(),
                planned = SummarizeDraftGroups(item?["lstCaseList"] as JsonArray),
                project = SummarizeDraftGroups(item?["lstProjectCase"] as JsonArray)
            })
        };
    }

    private static object SummarizeWeek(JsonArray drafts, string referenceDate)
    {
        var reference = DateTime.Parse(referenceDate, CultureInfo.InvariantCulture);
        var day = reference.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)reference.DayOfWeek;
        var monday = reference.AddDays(1 - day);
        var registrations = FlattenDraftRegistrations(drafts).ToList();
        var days = Enumerable.Range(0, 5).Select(offset =>
        {
            var date = monday.AddDays(offset).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var dayRegistrations = registrations.Where(item => (GetString(item, "fromDate") ?? "").StartsWith(date, StringComparison.Ordinal)).ToList();
            var total = dayRegistrations.Sum(item => GetDouble(item, "quantity") + GetDouble(item, "milliageHour"));
            return new
            {
                date,
                totalHours = Math.Round(total, 2),
                remainingTo7_5 = Math.Max(0, Math.Round(7.5 - total, 2)),
                registrations = dayRegistrations.Select(SummarizeRegistration)
            };
        });

        return new { weekOf = monday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), days };
    }

    private static IEnumerable<JsonNode> FlattenDraftRegistrations(JsonArray drafts)
    {
        foreach (var draft in drafts)
        {
            foreach (var item in draft?["lstInternalReg"] as JsonArray ?? new JsonArray()) if (item is not null) yield return item;
            foreach (var group in draft?["lstCaseList"] as JsonArray ?? new JsonArray())
            {
                foreach (var item in group?["lstReg"] as JsonArray ?? new JsonArray()) if (item is not null) yield return item;
            }
            foreach (var group in draft?["lstProjectCase"] as JsonArray ?? new JsonArray())
            {
                foreach (var item in group?["lstReg"] as JsonArray ?? new JsonArray()) if (item is not null) yield return item;
            }
        }
    }

    private static IEnumerable<object> SummarizeDraftGroups(JsonArray? groups)
    {
        return (groups ?? new JsonArray()).Select(group => new
        {
            documentNo = GetString(group, "documentno"),
            taskNo = GetString(group, "lineNo"),
            registrations = group?["lstReg"] as JsonArray ?? new JsonArray()
        });
    }

    private static IEnumerable<object> SummarizeCases(JsonArray cases, string? filter, int limit)
    {
        var values = cases.Select(item => new
        {
            customer = GetString(item, "name") ?? GetString(item, "caseName"),
            job = GetString(item, "job_no") ?? GetString(item, "documentno") ?? GetString(item, "d_no") ?? GetString(item, "documentNo"),
            task = GetString(item, "task_no") ?? GetString(item, "lineNo") ?? GetString(item, "l_no") ?? GetString(item, "taskNo"),
            description = GetString(item, "task_desc") ?? GetString(item, "job_desc") ?? GetString(item, "description"),
            registrationType = GetString(item, "registrationType"),
            appointment = GetString(item, "appointment_id") ?? GetString(item, "appointmentID"),
            date = GetString(item, "fromDate") ?? GetString(item, "pl_date") ?? GetString(item, "regDate")
        });

        if (!string.IsNullOrWhiteSpace(filter))
        {
            values = values.Where(item => JsonSerializer.Serialize(item).Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        return values.Take(Math.Max(1, limit));
    }

    private static object SummarizeRegistration(JsonNode item)
    {
        return new
        {
            id = GetInt(item, "id"),
            registrationType = GetString(item, "registrationType"),
            categoryName = GetString(item, "categoryName"),
            documentNo = GetString(item, "documentNo"),
            taskNo = GetString(item, "taskNo"),
            fromDate = GetString(item, "fromDate"),
            fromTime = GetString(item, "fromTime"),
            toTime = GetString(item, "toTime"),
            quantity = GetDouble(item, "quantity"),
            description = GetString(item, "description")
        };
    }

    private static object? SummarizeWriteResponse(JsonNode? node)
    {
        if (node is null) return null;
        return new
        {
            id = GetInt(node, "id"),
            status = GetString(node, "status"),
            documentNo = GetString(node, "documentNo"),
            taskNo = GetString(node, "taskNo")
        };
    }

    private static object PublicIdentity(AteaSession session) => new
    {
        userInitial = session.UserInitial,
        userEmail = session.UserEmail,
        tokenExpires = session.Expires
    };

    private static string Today() => DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    private static double HoursBetween(string from, string to) => Math.Round((TimeSpan.Parse(to) - TimeSpan.Parse(from)).TotalHours, 2);
    private static bool IsBase64Char(char c) => char.IsAsciiLetterOrDigit(c) || c is '+' or '/' or '=';
    private static string? GetString(JsonNode? node, string name) => node?[name]?.GetValue<object>()?.ToString();
    private static int GetInt(JsonNode? node, string name) => node?[name]?.GetValue<int?>() ?? 0;
    private static double GetDouble(JsonNode? node, string name) => node?[name]?.GetValue<double?>() ?? 0;
    private static bool? GetBool(JsonNode? node, string name) => node?[name]?.GetValue<bool?>();

    private sealed record AteaSession(
        string UserInitial,
        string UserEmail,
        string AccessToken,
        DateTimeOffset Expires,
        string CostCenterId,
        string ManagerId,
        string OvertimeModel);
}
