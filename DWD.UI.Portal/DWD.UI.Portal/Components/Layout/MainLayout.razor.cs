using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace DWD.UI.Portal.Components.Layout
{
    public partial class MainLayout
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Example JS interop call on first render:
                await JSRuntime.InvokeVoidAsync("console.log", "MainLayout rendered");
            }

            await base.OnAfterRenderAsync(firstRender);
        }
    }
}
