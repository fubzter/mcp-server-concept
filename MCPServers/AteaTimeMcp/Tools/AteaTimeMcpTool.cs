using System.ComponentModel;
using AteaTimeMcp.Services;
using ModelContextProtocol.Server;

namespace AteaTimeMcp;

[McpServerToolType]
public class AteaTimeMcpTool
{
    private readonly AteaTimeMcpService _service;

    public AteaTimeMcpTool(AteaTimeMcpService service)
    {
        _service = service;
    }

    [McpServerTool, Description("Show the active Atea Time identity from the local browser session.")]
    public Task<object> WhoAmI() => _service.WhoAmI();

    [McpServerTool, Description("List open Atea Time cases for the signed-in user. Optional filter matches customer, job, task, and description.")]
    public Task<object> ListOpenCases(
        [Description("Date in YYYY-MM-DD. Defaults to today.")] string? date = null,
        [Description("Optional text filter, for example customer name or case description.")] string? filter = null,
        [Description("Maximum number of cases to return.")] int limit = 100)
    {
        return _service.ListOpenCases(date, filter, limit);
    }

    [McpServerTool, Description("List current Atea Time drafts. Use details=true when exact draft registrations are needed.")]
    public Task<object> ListDrafts(
        [Description("Return full draft registration payloads when true.")] bool details = false)
    {
        return _service.ListDrafts(details);
    }

    [McpServerTool, Description("Summarize draft registrations for the work week containing the supplied date.")]
    public Task<object> WeekSummary(
        [Description("Date in YYYY-MM-DD. Defaults to today.")] string? date = null)
    {
        return _service.WeekSummary(date);
    }

    [McpServerTool, Description("Create an Atea Time project draft. Dry-run by default; set confirm=true only after the user explicitly approves.")]
    public Task<object> CreateProjectDraft(
        [Description("Atea Time job/document number.")] string job,
        [Description("Atea Time task number.")] string task,
        [Description("Atea Time appointment ID.")] string appointment,
        [Description("Registration date in YYYY-MM-DD.")] string date,
        [Description("Start time in HH:mm.")] string from,
        [Description("End time in HH:mm.")] string to,
        [Description("External/customer-facing work description.")] string description,
        [Description("Must be true to create the draft. Leave false for dry-run.")] bool confirm = false,
        [Description("Optional internal comment.")] string? internalComment = null,
        [Description("Whether this is the last visit on the case.")] bool lastVisit = false,
        [Description("Atea Time work location code. Defaults to 2.")] string workLocation = "2")
    {
        return _service.CreateDraft(job, task, appointment, date, from, to, description, confirm, internalComment, lastVisit, workLocation);
    }

    [McpServerTool, Description("Create an internal Atea Time education/activity draft. Dry-run by default; set confirm=true only after the user explicitly approves.")]
    public Task<object> CreateInternalDraft(
        [Description("Registration date in YYYY-MM-DD.")] string date,
        [Description("Start time in HH:mm.")] string from,
        [Description("End time in HH:mm.")] string to,
        [Description("Internal activity description.")] string description,
        [Description("Must be true to create the draft. Leave false for dry-run.")] bool confirm = false)
    {
        return _service.CreateInternalDraft(date, from, to, description, confirm);
    }

    [McpServerTool, Description("Delete an Atea Time draft. Dry-run by default; set confirm=true only after the user explicitly approves.")]
    public Task<object> DeleteDraft(
        [Description("Draft registration ID.")] int id,
        [Description("Must be true to delete the draft. Leave false for dry-run.")] bool confirm = false)
    {
        return _service.DeleteDraft(id, confirm);
    }

    [McpServerTool, Description("Submit Atea Time drafts in a date range. Dry-run by default; set confirm=true only after showing the user the draft summary and receiving approval.")]
    public Task<object> SubmitDrafts(
        [Description("Start date in YYYY-MM-DD.")] string from,
        [Description("End date in YYYY-MM-DD.")] string to,
        [Description("Must be true to submit drafts. Leave false for dry-run.")] bool confirm = false)
    {
        return _service.SubmitDrafts(from, to, confirm);
    }
}
