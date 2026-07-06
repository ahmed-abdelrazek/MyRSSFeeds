using MyRSSFeeds.Core.Models;
using System.Linq;
using System.Net;

namespace MyRSSFeeds.WinUI.Helpers
{
    /// <summary>
    /// Builds the HTML pages shown in the article WebView. One template for
    /// both themes - the palette (matching the app's NightTime navy/orange
    /// styles) is swapped via CSS variables.
    /// </summary>
    public static class ReaderPage
    {
        private const string DarkPalette = "color-scheme: dark; --bg:#0d2644; --fg:#f2f7fc; --muted:#8dbfdf; --accent:#ff8b4d; --rule:#2f4e73; --card:#13355c;";
        private const string LightPalette = "color-scheme: light; --bg:#f4faff; --fg:#092038; --muted:#3282a8; --accent:#b8430c; --rule:#bcd9ec; --card:#e3f1fb;";

        private static readonly string[] RtlLanguages = { "ar", "fa", "ur", "ps", "sd", "ckb", "dv", "ug", "yi" };

        public static string BuildArticleHtml(RSS item, bool isDark)
        {
            // titles are stored feed-raw and may already contain entities (see
            // RSS.PostShortTitle) - normalize by decoding before encoding
            string title = WebUtility.HtmlEncode(WebUtility.HtmlDecode(item.PostTitle));
            string siteTitle = WebUtility.HtmlEncode(item.PostSource?.SiteTitle);
            string date = WebUtility.HtmlEncode(item.CreatedAtLocalTime.ToString());
            string authors = WebUtility.HtmlEncode(string.Join(", ", item.Authors?.Select(x => x.Username) ?? Enumerable.Empty<string>()));
            string siteUrl = WebUtility.HtmlEncode(item.PostSource?.BaseUrl?.OriginalString);
            string articleUrl = WebUtility.HtmlEncode(item.LaunchURL?.OriginalString);

            string authorsHtml = string.IsNullOrWhiteSpace(authors)
                ? string.Empty
                : $"<span class=\"sep\">·</span><span>{authors}</span>";

            // Feeds declare their language (channel <language>); RTL ones get a
            // right-to-left page. When no language is declared, let the browser
            // resolve direction per element from the content itself (dir=auto)
            string language = item.PostSource?.Language?.Trim() ?? string.Empty;
            bool hasLanguage = language.Length > 0;
            bool isRtl = hasLanguage && RtlLanguages.Any(rtl =>
                language.StartsWith(rtl, System.StringComparison.OrdinalIgnoreCase)
                && (language.Length == rtl.Length || language[rtl.Length] == '-' || language[rtl.Length] == '_'));
            string htmlDir = isRtl ? "rtl" : "ltr";
            string contentDir = hasLanguage ? string.Empty : " dir=\"auto\"";
            string langAttribute = hasLanguage ? WebUtility.HtmlEncode(language) : "en";

            return $$"""
<!doctype html>
<html lang="{{langAttribute}}" dir="{{htmlDir}}">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1" />
<title>{{title}}</title>
<style>
:root { {{(isDark ? DarkPalette : LightPalette)}} }
* { box-sizing: border-box; }
html, body { margin: 0; padding: 0; background: var(--bg); color: var(--fg); }
body {
    font-family: "Segoe UI Variable Text", "Segoe UI", system-ui, sans-serif;
    font-size: 16px;
    line-height: 1.65;
    overflow-wrap: break-word;
}
main { max-width: 42rem; margin: 0 auto; padding: 24px 20px 48px; }
.meta {
    display: flex; flex-wrap: wrap; align-items: baseline; gap: 6px;
    color: var(--muted); font-size: 0.85rem;
}
.meta a { color: var(--muted); font-weight: 600; text-decoration: none; }
.meta a:hover { color: var(--accent); }
.meta .sep { opacity: 0.6; }
h1.title { margin: 6px 0 14px; font-size: 1.6rem; line-height: 1.3; font-weight: 650; }
h1.title a { color: var(--accent); text-decoration: none; }
h1.title a:hover { text-decoration: underline; }
hr.rule { border: 0; border-top: 1px solid var(--rule); margin: 0 0 20px; }
article a { color: var(--accent); }
article img, article video, article iframe, article embed {
    max-width: 100%; height: auto; border-radius: 8px;
}
article figure { margin: 16px 0; }
article figcaption { color: var(--muted); font-size: 0.85rem; }
article pre, article code {
    font-family: "Cascadia Mono", Consolas, monospace; font-size: 0.9em;
    background: var(--card); border-radius: 6px;
}
article code { padding: 1px 5px; }
article pre { padding: 12px 14px; overflow-x: auto; direction: ltr; text-align: left; }
article pre code { padding: 0; background: none; }
article blockquote {
    margin: 16px 0; padding: 2px 16px;
    border-inline-start: 3px solid var(--accent); color: var(--muted);
}
article table {
    border-collapse: collapse; display: block; overflow-x: auto; max-width: 100%;
}
article th, article td { border: 1px solid var(--rule); padding: 6px 10px; }
</style>
</head>
<body>
<main>
    <p class="meta">
        <a href="{{siteUrl}}">{{siteTitle}}</a>
        <span class="sep">·</span><span>{{date}}</span>
        {{authorsHtml}}
    </p>
    <h1 class="title"{{contentDir}}><a href="{{articleUrl}}">{{title}}</a></h1>
    <hr class="rule" />
    <article{{contentDir}}>{{item.Description}}</article>
</main>
</body>
</html>
""";
        }

        public static string BuildBlankHtml(bool isDark)
        {
            return $$"""
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1" />
<title>blank</title>
<style>:root { {{(isDark ? DarkPalette : LightPalette)}} } html, body { margin: 0; background: var(--bg); color: var(--fg); }</style>
</head>
<body></body>
</html>
""";
        }
    }
}
