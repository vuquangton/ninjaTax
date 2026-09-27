using System;
namespace ninjaTax.ViewComponents
{
    public class MenuItem
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Url { get; set; }
        public bool IsActive { get; set; }
        public List<MenuItem>? Children { get; set; }
    }
}
