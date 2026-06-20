using Bunit;
using Bunit.TestDoubles;
using BootManager.Web.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BootManager.UnitTests.Web;

/// <summary>
/// Real bUnit tests for storage navigation in NavMenu.
/// </summary>
public class NavMenuComponentTests : TestContext
{
    [Fact]
    public void NavMenu_OwnerSeesStorageMenu_WithLocationsAndTagOverview()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("owner");
        authContext.SetRoles("Owner");

        var cut = Render(RenderNavMenu);

        Assert.Contains("Opslag", cut.Markup);

        cut.Find("#storageDropdown").Click();

        var locationsLink = cut.Find("a[href='storage/locations']");
        var tagOverviewLink = cut.Find("a[href='storage/tag-overview']");

        Assert.Equal("Locaties", locationsLink.TextContent.Trim());
        Assert.Equal("Tagoverzicht", tagOverviewLink.TextContent.Trim());
    }

    [Fact]
    public void NavMenu_CrewSeesStorageMenu_WithLocationsOnly()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("crew");
        authContext.SetRoles("Crew");

        var cut = Render(RenderNavMenu);

        Assert.Contains("Opslag", cut.Markup);

        cut.Find("#storageDropdown").Click();

        var locationsLink = cut.Find("a[href='storage/locations']");
        Assert.Equal("Locaties", locationsLink.TextContent.Trim());
        Assert.Empty(cut.FindAll("a[href='storage/tag-overview']"));
    }

    private static RenderFragment RenderNavMenu => builder =>
    {
        builder.OpenComponent<CascadingAuthenticationState>(0);
        builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
        {
            childBuilder.OpenComponent<NavMenu>(0);
            childBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    };
}
