using IIIF.POC.ChangeTrackingLab.Models;
using IIIF.POC.ChangeTrackingLab.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IIIF.POC.ChangeTrackingLab.Pages;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class IndexModel : PageModel
{
    private const string SessionIdKey = "ChangeTrackingLab.SessionId";
    private readonly ChangeTrackingSessionStore _store;

    public IndexModel(ChangeTrackingSessionStore store)
    {
        _store = store;
    }

    public LabViewModel Lab { get; private set; } = new();

    private string SessionId
    {
        get
        {
            var sessionId = HttpContext.Session.GetString(SessionIdKey);
            if (sessionId is not null)
                return sessionId;

            sessionId = HttpContext.Session.Id;
            HttpContext.Session.SetString(SessionIdKey, sessionId);
            return sessionId;
        }
    }

    public void OnGet() => Refresh();

    public IActionResult OnPostChangeRights() =>
        Update(ChangeTrackingLabService.ChangeRights);

    public IActionResult OnPostChangeCanvasHeight() =>
        Update(ChangeTrackingLabService.ChangeFirstCanvasHeight);

    public IActionResult OnPostRenameCanvas() =>
        Update(ChangeTrackingLabService.RenameSecondCanvas);

    public IActionResult OnPostAddCanvas() =>
        Update(ChangeTrackingLabService.AddCanvas);

    public IActionResult OnPostRemoveCanvas() =>
        Update(ChangeTrackingLabService.RemoveLastCanvas);

    public IActionResult OnPostCommit() =>
        Update(ChangeTrackingLabService.CommitDelta);

    public IActionResult OnPostAccept() =>
        Update(static state => state.Manifest.AcceptChanges());

    public IActionResult OnPostReset()
    {
        _store.Reset(SessionId);
        return RedirectToPage();
    }

    private void Refresh()
    {
        Lab = _store.Read(SessionId, ChangeTrackingLabService.BuildView);
    }

    private IActionResult Update(Action<ChangeTrackingSession> update)
    {
        _store.Update(SessionId, update);
        return RedirectToPage();
    }
}
