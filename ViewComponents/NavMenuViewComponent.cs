using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ninjaTax.ViewComponents
{
    public class NavMenuViewComponent : ViewComponent
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public IViewComponentResult Invoke()
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "menu.json");
            var root = JsonSerializer.Deserialize<MenuRoot>(File.ReadAllText(jsonPath), JsonOptions);
            var menu = root?.Menu ?? new();

            var currentController = ViewContext.RouteData.Values["controller"]?.ToString();
            var currentAction = ViewContext.RouteData.Values["action"]?.ToString();

            MarkActive(menu, currentController, currentAction);

            return View(menu);
        }

        private static void MarkActive(List<MenuItem> items, string? controller, string? action)
        {
            foreach (var item in items)
            {
                item.IsActive = item.Controller?.Equals(controller, System.StringComparison.OrdinalIgnoreCase) == true &&
                               item.Action?.Equals(action, System.StringComparison.OrdinalIgnoreCase) == true;

                if (item.Children is { Count: > 0 })
                {
                    MarkActive(item.Children, controller, action);
                    if (!item.IsActive)
                        item.IsActive = item.Children.Any(c => c.IsActive);
                }
            }
        }
    }

    public class MenuRoot
    {
        public List<MenuItem> Menu { get; set; } = new();
    }
}
